using System.Reflection;
using System.Runtime.Loader;
using log4net.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyModel;


namespace Horizonte;

public class HAssemblyManager : IhAssemblyManager
{
    private readonly ModulesSettings _settings;
    private List<string> _searchPaths = new List<string>();
    private readonly HorizonteEnv _environment;
    public new List<Assembly> Assemblies;
    private string _installFolder = String.Empty;


    public HAssemblyManager(ModulesSettings settings, HorizonteEnv environment)
    {
        _settings = settings;
        _environment = environment;
        SetUpAssemblyPaths();
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyFromNuGetPackages;
        Assemblies = LoadModulesFromEnvironment();
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
            Console.WriteLine("Resolving assembly: " + args.Name);
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
                Console.WriteLine($"Could not resolve assembly: {args.Name}");
                return null;
            }

            //registras assets
            _environment.StaticFileRegistry.RegisterPackageDirectory(resAssemblyPath);
            // Cargar y devolver el ensamblado seleccionado
            return Assembly.LoadFrom(resAssemblyPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error trying to resolve assembly from repository: {ex.Message}");
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
            var list = Directory.EnumerateFiles(PackageDirectory, "*.dll", SearchOption.AllDirectories).ToList();


            foreach (var dllPath in list)
            {
                // Dividimos el path en segmentos para extraer la información necesaria
                var pathSegments = dllPath.Split(Path.DirectorySeparatorChar);

                var packageId = pathSegments[pathSegments.Length - 5];
                var version = pathSegments[pathSegments.Length - 4];
                var framework = pathSegments[pathSegments.Length - 2];

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
            Console.WriteLine(e);
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
            "netstandard2.1",
            "netstandard2.0",
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
                SelectedVersion(assemblyName.Version, "net8.0", versionList, exactmatch); //// obtener net8.0 en runtime
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

                Console.WriteLine($"Downloading package '{packageName}', versión '{version}' from {nugetDownloadUrl}");
                // Descargar el paquete .nupkg
                using var httpClient = new HttpClient();
                using var response = httpClient.GetAsync(nugetDownloadUrl).Result;
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error downloading package {packageName}: {response.StatusCode}");
                    continue;
                }

                // Guardar el archivo descargado
                using (var fileStream =
                       new FileStream(packageFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    response.Content.CopyToAsync(fileStream).Wait();
                }

                Console.WriteLine($"Package downloaded successfully: {packageFileName}");
                // Extraer el contenido del paquete
                string targetDirectory = Path.Combine(_installFolder, packageName.ToLower(), version);
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(packageFileName, targetDirectory, true);
                Console.WriteLine($"Package successfully extracted in: {targetDirectory}");

                // Borrar el archivo temporal
                if (File.Exists(packageFileName))
                    File.Delete(packageFileName);
                break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading and extracting package from repository: {ex.Message}");
        }
    }


    /// <summary>
    /// Carga los ensamblados desde el entorno utilizando la configuración
    /// y los módulos activos. También asegura que el directorio de módulos existe.
    /// </summary>
    /// <returns>El arreglo de ensamblados cargados en el dominio de la aplicación.</returns>
    private List<Assembly> LoadModulesFromEnvironment()
    {
        try
        {
            List<Assembly> result = new List<Assembly>();
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
            return result;
        }
        catch (Exception exception)
        {
            // Manejo básico de errores; imprime el error y retorna un arreglo vacío.
            Console.WriteLine(exception);
            return new List<Assembly>();
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
                Console.WriteLine($"Loading module from: {moduleItem.Path}");


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
                    Console.WriteLine($"Can't find: {moduleItem.Path}");
            }
        }
        catch (Exception exception)
        {
            // Manejo de errores al intentar cargar un módulo.
            Console.WriteLine($"Error on loading {moduleItem.ModuleName}: {exception}");
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
                        Console.WriteLine($"--Loading additional dll: {dllFile}");
                    }
                    else
                    {
                        // Console.WriteLine($">>>Ensamblado adicionales ya cargado: {dllFile}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error on load additional dll from module: {modulePath}" +
                              Environment.NewLine + e);
        }
    }
}