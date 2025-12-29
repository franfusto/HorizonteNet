using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;
using log4net;
using log4net.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyModel;


namespace Horizonte;

public class HAssemblyManager : IhAssemblyManager
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(HAssemblyManager));
    private readonly ModulesSettings _settings;
    private List<string> _searchPaths = new List<string>();
    private readonly SymLinkScafolder _linkScafolder;
    private readonly HorizonteEnv _environment;
    private string _installFolder = String.Empty;
    public List<Assembly> Assemblies => AppDomain.CurrentDomain.GetAssemblies().ToList();

    public HAssemblyManager(ModulesSettings settings,SymLinkScafolder linkScafolder, HorizonteEnv environment)
    {
        _settings = settings;
        _linkScafolder = linkScafolder;
        _environment = environment;
        SetUpAssemblyPaths();
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyFromNuGetPackages;
        LoadModulesFromEnvironment();
    }

    private void SetUpAssemblyPaths()
    {
        string systemNugetPackagesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget", "packages");
        if (Directory.Exists(systemNugetPackagesPath))
        {
            _searchPaths.Add(systemNugetPackagesPath);
            _installFolder = systemNugetPackagesPath;
        }

        foreach (var folderItem in _settings.NugetFolders.OrderBy(x => x.Order))
        {
            if (!folderItem.Active) continue;
            if (folderItem.InstallFolder)
            {
                if (!Directory.Exists(folderItem.Folder))
                {
                    Directory.CreateDirectory(folderItem.Folder);
                }

                _installFolder = folderItem.Folder;
            }

            if (Directory.Exists(folderItem.Folder))
            {
                _searchPaths.Add(folderItem.Folder);
            }
        }
    }


    private Assembly? ResolveAssemblyFromNuGetPackages(object? sender, ResolveEventArgs args)
    {
        try
        {
            Log.Info("Resolving assembly: " + args.Name);
            var assemblyName = new AssemblyName(args.Name);
            string? resAssemblyPath = null;
            //try load from local directory
            resAssemblyPath = ResolveNugetFromLocalDirectory(assemblyName, true);
            if (resAssemblyPath == null)
            {
                // try download and install 
                DownloadAndExtractPackageFromNuGet(assemblyName);
                //try load from local directory, now approx version
                resAssemblyPath = ResolveNugetFromLocalDirectory(assemblyName, false);
            }

            if (resAssemblyPath == null)
            {
                Log.Error($"Could not resolve assembly: {args.Name}");
                return null;
            }

            //registras assets
            _environment.StaticFileRegistry.RegisterPackageDirectory(resAssemblyPath);

            // Procesar archivos .targets / .props para crear enlaces simbólicos
            try
            {
                var symLinks = GetContentMappingsFromPackage(resAssemblyPath);
                if (symLinks.Any())
                {
                    _linkScafolder.BuildScafolder(symLinks);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error processing MSBuild targets for symlinks: {ex.Message}");
            }

            // Cargar y devolver el ensamblado seleccionado
            return Assembly.LoadFrom(resAssemblyPath);
        }
        catch (Exception ex)
        {
            Log.Error($"Error trying to resolve assembly from repository: {ex.Message}");
        }

        // Devolver null si no se pudo resolver
        return null;
    }


    private List<NugetPackageVersionInformation> GetNugetPackageVersionInformation(string PackageDirectory)
    {
        var result = new List<NugetPackageVersionInformation>();
        try
        {
            // Obtenemos la lista de archivos .dll en el directorio del paquete
            var list = Directory.EnumerateFiles(PackageDirectory, "*.dll", SearchOption.AllDirectories)
                .Where(x => x.Contains("/lib/") || x.Contains(@"\lib\") ) .ToList();


            foreach (var dllPath in list)
            {
                //Console.WriteLine($"procesando: {dllPath}");
                
                // Dividimos el path en segmentos para extraer la información necesaria
                var pathSegments = dllPath.Split(Path.DirectorySeparatorChar);

                var packageId = pathSegments[pathSegments.Length - 5];
                var version = pathSegments[pathSegments.Length - 4];
                var framework = pathSegments[pathSegments.Length - 2];

                // fix preview packages
                //version=version.Replace("-beta", "");
                if (version.Contains("-beta")) version = version.Split("-")[0] ;
                if (version.Contains("-alpha")) version = version.Split("-")[0] ;
                if (version.Contains("-preview")) version = version.Split("-")[0] ;
                if(dllPath.Contains("/buildTransitive/")) continue;                    
                if(dllPath.Contains("/build/")) continue;     
                // Filtramos para quedarnos solo con los 3 primeros números (Major.Minor.Build)
                if (Version.TryParse(version, out var parsedVersion))
                {
                    version = $"{parsedVersion.Major}.{parsedVersion.Minor}.{(parsedVersion.Build != -1 ? parsedVersion.Build : 0)}";
                }
                // Añadimos la información a la lista
                result.Add(new NugetPackageVersionInformation
                {
                    PackageId = packageId,
                    Version = Version.Parse(version),
                    Framework = framework,
                    DllPath = dllPath
                });
            }
        }
        catch (Exception e)
        {
            Log.Error(e);
        }

        return result;
    }

    private NugetPackageVersionInformation? SelectedVersion(
        Version versionSolicitada,
        string frameworkSolicitado,
        List<NugetPackageVersionInformation> versionesDisponibles,
        bool exactmatch)
    {
        // Definimos un orden de prioridad para los frameworks
        var prioridadFrameworks = new List<string>
        {
            frameworkSolicitado,
            //"netstandard2.1",
            //"netstandard2.0",
            "net10.0",
            "net9.0",
            "net8.0",
            "net7.0",
            "net6.0"
        };

        NugetPackageVersionInformation? mejorCoincidencia = null;

        // Filtrar por framework siguiendo el orden de prioridad
        foreach (var framework in prioridadFrameworks)
        {
            // Buscar las versiones disponibles dentro del framework actual iterado
            var versionesFiltradas = versionesDisponibles
                .Where(info => info.Framework.Equals(framework, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Version)
                .ToList();

            if (versionesFiltradas.Any())
            {
                if (exactmatch)
                {
                    // Buscar una coincidencia exacta de versión
                    mejorCoincidencia = versionesFiltradas
                        .FirstOrDefault(info =>
                            info.Version.Major == versionSolicitada.Major &&
                            info.Version.Minor == versionSolicitada.Minor &&
                            info.Version.Build == versionSolicitada.Build);

                    if (mejorCoincidencia != null)
                    {
                        // Si encontramos una coincidencia exacta, terminamos el proceso
                        break;
                    }
                }
                else
                {
                    // Si encontramos al menos una coincidencia para este framework, seleccionamos la mejor versión
                    mejorCoincidencia = versionesFiltradas
                                            .OrderBy(info => info.Version) // Ordenamos las versiones de menor a mayor
                                            .FirstOrDefault(info =>
                                                info.Version >=
                                                versionSolicitada) // Preferimos versiones mayores o iguales a la solicitada
                                        ?? versionesFiltradas
                                            .Last(); // Si no hay mayor o igual, tomamos la más reciente disponible

                    // Si encontramos una coincidencia válida, terminamos aquí
                    break;
                }
            }
        }

        return mejorCoincidencia;
    }


    private string? ResolveNugetFromLocalDirectory(AssemblyName assemblyName, bool exactmatch = true)
    {
        if (string.IsNullOrEmpty(assemblyName.Name)) return null;
        if (assemblyName.Version == null) return null;
        foreach (var searchPath in _searchPaths)
        {
            var directoriopaquete = Directory.GetDirectories(searchPath, assemblyName.Name.ToLower());
            if (!directoriopaquete.Any()) continue;
            var versionList = GetNugetPackageVersionInformation(directoriopaquete.First());
            var versionSeleccionada =
                SelectedVersion(assemblyName.Version, "net10.0", versionList, exactmatch); //// obtener net10.0 en runtime
            if (versionSeleccionada == null) continue;
            return versionSeleccionada.DllPath;
        }

        return null;
    }


    private void DownloadAndExtractPackageFromNuGet(AssemblyName assemblyName)
    {
        if (string.IsNullOrEmpty(assemblyName.Name)) return;
        if (assemblyName.Version == null) return;
        var packageName = assemblyName.Name;
        var version = assemblyName.Version.ToString();
        // Filtramos para quedarnos solo con los 3 primeros números (Major.Minor.Build)
        if (Version.TryParse(version, out var parsedVersion))
        {
            version = $"{parsedVersion.Major}.{parsedVersion.Minor}.{(parsedVersion.Build != -1 ? parsedVersion.Build : 0)}";
        }
        var nugetDownloadUrl = string.Empty;
        var packageFileName = Path.Combine(Path.GetTempPath(), $"{packageName}.{version}.nupkg");
        try
        {
            foreach (var nugetServer in _settings.NugetServers.OrderBy(x => x.Order))
            {
                
                //url de descarga
                if (!nugetServer.Active) continue;
                switch (nugetServer.Version)
                {
                    case 1:
                    case 2:
                        nugetDownloadUrl = Path.Combine(nugetServer.Server, "package", packageName, version);
                        break;
                    case 3:
                        nugetDownloadUrl = Path.Combine(nugetServer.Server, "package", packageName, version) + "/" +
                                           packageName + "." + version + ".nupkg";
                        break;
                    default:
                        return;
                }

                Log.Info($"Downloading package '{packageName}', versión '{version}' from {nugetDownloadUrl}");
                // Descargar el paquete .nupkg
                using var httpClient = new HttpClient();
                using var response = httpClient.GetAsync(nugetDownloadUrl).Result;
                if (!response.IsSuccessStatusCode)
                {
                    Log.Error($"Error downloading package {packageName}: {response.StatusCode}");
                    continue;
                }

                // Guardar el archivo descargado
                using (var fileStream =
                       new FileStream(packageFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    response.Content.CopyToAsync(fileStream).Wait();
                }

                Log.Info($"Package downloaded successfully: {packageFileName}");
                // Extraer el contenido del paquete
                string targetDirectory = Path.Combine(_installFolder, packageName.ToLower(), version);
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(packageFileName, targetDirectory, true);
                Log.Info($"Package successfully extracted in: {targetDirectory}");

                // Borrar el archivo temporal
                if (File.Exists(packageFileName))
                    File.Delete(packageFileName);
                break;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error downloading and extracting package from repository: {ex.Message}");
        }
    }


    /// <summary>
    /// Carga los ensamblados desde el entorno utilizando la configuración
    /// y los módulos activos. También asegura que el directorio de módulos existe.
    /// </summary>
    /// <returns>El arreglo de ensamblados cargados en el dominio de la aplicación.</returns>
    private void LoadModulesFromEnvironment()
    {
        try
        {
            // Itera sobre los módulos activos en la configuración.
            foreach (var moduleItem in _settings.List.Where(item => item.Active))
            {
                LoadModuleAssembly(moduleItem);
                if (moduleItem.LoadAdditionalDlls != null && moduleItem.LoadAdditionalDlls.Value &&
                    moduleItem.Path !=
                    null) // debemos tener en cuenta que si el path es null no se carga nada.... verificar nombre y version del módulo
                {
                    LoadAdditinalAssemblies(moduleItem.Path);
                }
            }

            // Retorna todos los ensamblados actualmente cargados en el dominio de la aplicación.
        }
        catch (Exception exception)
        {
            // Manejo básico de errores; imprime el error y retorna un arreglo vacío.
            Log.Error(exception);
        }
    }


    /// <summary>
    /// Carga un ensamblado individual del módulo especificado y registra sus activos y documentación.
    /// </summary>
    /// <param name="moduleItem">El módulo que se debe procesar.</param>
    private void LoadModuleAssembly(ModulesSettingsItem moduleItem)
    {
        try
        {
            if (File.Exists(moduleItem.Path))
            {
                Log.Info($"Loading module from: {moduleItem.Path}");


                // Carga el ensamblado desde la ruta especificada.
                var loadedassembly = Assembly.LoadFrom(moduleItem.Path);

                // Agrega los activos y el archivo de documentación al entorno.
                _environment.StaticFileRegistry.RegisterModuleDirectory(moduleItem);
                //AddAssetsFolder(moduleItem);
            }
            else
            {
                var args = new ResolveEventArgs(
                    $"{moduleItem.ModuleName}, Version={moduleItem.ModuleVersion}"); //, Culture=neutral, PublicKeyToken=null

                var nugetassembly = ResolveAssemblyFromNuGetPackages(null, args);
                if (nugetassembly != null)
                    Assembly.LoadFrom(nugetassembly.Location);
                else
                    Log.Error($"Can't find: {moduleItem.Path}");
            }
        }
        catch (Exception exception)
        {
            // Manejo de errores al intentar cargar un módulo.
            Log.Error($"Error on loading {moduleItem.ModuleName}: {exception}");
        }
    }


    private void LoadAdditinalAssemblies(string modulePath)

    {
        try
        {
            // Obtiene el directorio donde se encuentra el módulo
            string directoryPath = Path.GetDirectoryName(modulePath) ?? string.Empty;
            if (Directory.Exists(directoryPath))
            {
                // Encuentra todos los archivos .dll en el directorio
                foreach (var dllFile in Directory.EnumerateFiles(directoryPath, "*.dll"))
                {
                    // Obtiene el nombre del ensamblado
                    string assemblyName = AssemblyName.GetAssemblyName(dllFile).FullName;

                    // Verifica si el ensamblado ya está cargado
                    if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == assemblyName))
                    {
                        // Si no, lo carga
                        Assembly.LoadFrom(dllFile);
                        Log.Info($"--Loading additional dll: {dllFile}");
                    }
                    else
                    {
                        // Log.Info($">>>Ensamblado adicionales ya cargado: {dllFile}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error on load additional dll from module: {modulePath}" +
                              Environment.NewLine + e);
        }
    }

    private List<SymLinkDef> GetContentMappingsFromPackage(string resAssemblyPath)
    {
        var mappings = new List<SymLinkDef>();
        try
        {
            // resAssemblyPath: [ruta nuget packages]/[nombre del paquete]/[version del paquete]/lib/[framework]/[ensamblado].dll
            // Subir tres directorios desde el archivo resAssemblyPath para obtener el path del paquete (ej: /.../porcupine/3.0.10/)
            var libDir = Directory.GetParent(resAssemblyPath); // [framework]
            var pkgVersionDir = libDir?.Parent;               // lib
            var packagePath = pkgVersionDir?.Parent?.FullName; // [version del paquete]
            
            if (string.IsNullOrEmpty(packagePath)) return mappings;

            // Determinar el framework a partir de la ruta del ensamblado
            var framework = libDir?.Name ?? "net10.0";

            // 1. Localizar la carpeta de construcción (preferiblemente buildTransitive)
            string buildDir = Path.Combine(packagePath, "buildTransitive", framework);
            if (!Directory.Exists(buildDir))
            {
                buildDir = Path.Combine(packagePath, "build", framework);
                if (!Directory.Exists(buildDir)) return mappings;
            }

            // 2. Buscar archivos de definición de MSBuild (.targets o .props)
            var definitionFiles = Directory.GetFiles(buildDir, "*.targets")
                .Concat(Directory.GetFiles(buildDir, "*.props"));

            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

            foreach (var defFile in definitionFiles)
            {
                var doc = XDocument.Load(defFile);

                // Buscamos elementos <Content> que tengan <CopyToOutputDirectory>
                var contentItems = doc.Descendants(ns + "Content")
                    .Where(c => c.Element(ns + "CopyToOutputDirectory") != null);

                foreach (var item in contentItems)
                {
                    string include = item.Attribute("Include")?.Value ?? "";
                    string linkTemplate = item.Element(ns + "Link")?.Value ?? "";

                    if (string.IsNullOrEmpty(include)) continue;

                    // Resolver la variable $(MSBuildThisFileDirectory) que apunta a la carpeta del .targets
                    string targetsFolder = buildDir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                    string resolvedInclude = include.Replace("$(MSBuildThisFileDirectory)", targetsFolder);

                    // Normalizar la ruta para resolver ".." y separadores correctos del sistema
                    resolvedInclude = Path.GetFullPath(resolvedInclude.Replace('\\', Path.DirectorySeparatorChar));

                    if (resolvedInclude.Contains('*'))
                    {
                        // --- CASO CON COMODINES (Ej: resources\**) ---
                        string baseDir = resolvedInclude.Split('*')[0];
                        if (!Directory.Exists(baseDir)) continue;

                        var files = Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            string relativePath = Path.GetRelativePath(baseDir, file);
                            string recursiveDir = Path.GetDirectoryName(relativePath) ?? "";
                            if (!string.IsNullOrEmpty(recursiveDir))
                                recursiveDir += Path.DirectorySeparatorChar;

                            // Reemplazar placeholders de MSBuild en el Link
                            string finalDest = linkTemplate
                                .Replace("%(RecursiveDir)", recursiveDir)
                                .Replace("%(Filename)", Path.GetFileNameWithoutExtension(file))
                                .Replace("%(Extension)", Path.GetExtension(file))
                                .Replace('\\', Path.DirectorySeparatorChar);

                            if (string.IsNullOrEmpty(finalDest))
                                finalDest = relativePath;

                            mappings.Add(new SymLinkDef
                            {
                                Source = file,
                                Destination = finalDest,
                                IsDirecory = false,
                                HardCoded = false
                            });
                        }
                    }
                    else
                    {
                        // --- CASO ARCHIVO INDIVIDUAL ---
                        if (File.Exists(resolvedInclude))
                        {
                            string finalDest = !string.IsNullOrEmpty(linkTemplate)
                                ? linkTemplate.Replace('\\', Path.DirectorySeparatorChar)
                                : Path.GetFileName(resolvedInclude);

                            mappings.Add(new SymLinkDef
                            {
                                Source = resolvedInclude,
                                Destination = finalDest,
                                IsDirecory = false,
                                HardCoded = false
                            });
                        }
                        else if (Directory.Exists(resolvedInclude))
                        {
                             string finalDest = !string.IsNullOrEmpty(linkTemplate)
                                ? linkTemplate.Replace('\\', Path.DirectorySeparatorChar)
                                : Path.GetFileName(resolvedInclude.TrimEnd(Path.DirectorySeparatorChar));

                            mappings.Add(new SymLinkDef
                            {
                                Source = resolvedInclude,
                                Destination = finalDest,
                                IsDirecory = true,
                                HardCoded = false
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error extracting content mappings: {ex.Message}");
        }

        return mappings;
    }

   
}