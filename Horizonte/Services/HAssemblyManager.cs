using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;
using log4net;
using log4net.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace Horizonte;

public class HAssemblyManager : IhAssemblyManager
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(HAssemblyManager));
    private readonly ModulesSettings _settings;
    private List<string> _searchPaths = new List<string>();
    private readonly SymLinkScafolder _linkScafolder;
    private readonly HorizonteEnv _environment;
    private string _installFolder = String.Empty;
    private Dictionary<string, AssemblyLoadContext> _domains = new Dictionary<string, AssemblyLoadContext>();
    private List<BackgroundService> _dynamicServices = new List<BackgroundService>();
    public List<Assembly> Assemblies => AssemblyLoadContext.Default.Assemblies.Concat(_domains.Values.SelectMany(x => x.Assemblies)).ToList();

    public Dictionary<string, List<Assembly>> AssembliesByDomain
    {
        get
        {
            var result = new Dictionary<string, List<Assembly>>();
            result["Default"] = AssemblyLoadContext.Default.Assemblies.ToList();
            foreach (var domain in _domains)
            {
                result[domain.Key] = domain.Value.Assemblies.ToList();
            }
            return result;
        }
    }

    public event Action? DomainChanged;

    public HAssemblyManager(ModulesSettings settings,SymLinkScafolder linkScafolder, HorizonteEnv environment)
    {
        _settings = settings;
        _linkScafolder = linkScafolder;
        _environment = environment;
        SetUpAssemblyPaths();
        SetUpDomains();
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

    private void SetUpDomains()
    {
        try
        {
            foreach (var domainName in _settings.Domains)
            {
                if (domainName == "Default")
                {
                    // Default es el ALC por defecto, nos aseguramos de que tenga el resolve
                    AssemblyLoadContext.Default.Resolving -= ResolveAssemblyFromALC;
                    AssemblyLoadContext.Default.Resolving += ResolveAssemblyFromALC;
                }
                else
                {
                    if (!_domains.ContainsKey(domainName))
                    {
                        Log.Info($"Creando dominio (ALC): {domainName}");
                        var alc = new AssemblyLoadContext(domainName, isCollectible: true);
                        alc.Resolving += ResolveAssemblyFromALC;
                        _domains[domainName] = alc;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }


    public Assembly? ResolveAssemblyFromALC(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        return ResolveAssemblyFromNuGetPackages(context, new ResolveEventArgs(assemblyName.FullName));
    }


    public Assembly? ResolveAssemblyFromNuGetPackages(object? sender, ResolveEventArgs args)
    {
        try
        {
            Log.Info("Resolving assembly: " + args.Name);
            string? resAssemblyPath = null;
            
            var (name, version) = ParseAssemblyName(args.Name);
            
            //try load from local directory
            resAssemblyPath = ResolveNugetFromLocalDirectory(name, version, null, true);
            //
            if (resAssemblyPath == null)
            {
                resAssemblyPath = ResolveNugetFromLocalDirectory(name, version, null, false);
                
            }

            //
            if (resAssemblyPath == null)
            {
                // try download and install 
                DownloadAndExtractPackage(name, version);
                //try load from local directory, now approx version
                resAssemblyPath = ResolveNugetFromLocalDirectory(name, version, null, false);
            }

            if (resAssemblyPath == null)
            {
                Log.Error($"Could not resolve assembly: {args.Name}");
                return null;
            }

            // Asegurarse de que el path sea absoluto
            if (!Path.IsPathRooted(resAssemblyPath))
            {
                // Intentar resolverlo relativo al directorio de ejecución o a los search paths
                // Pero lo más seguro para ALC es Path.GetFullPath
                resAssemblyPath = Path.GetFullPath(resAssemblyPath);
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
            if (sender is AssemblyLoadContext context)
            {
                return context.LoadFromAssemblyPath(resAssemblyPath);
            }
            
            return Assembly.LoadFrom(resAssemblyPath);
        }
        catch (Exception ex)
        {
            Log.Error($"Error trying to resolve assembly from repository: {ex.Message}");
        }

        // Devolver null si no se pudo resolver
        return null;
    }


    public string? ResolveAssemblyDllPath(string packageName, string version)
    {
        // Intentar resolver localmente con versión exacta primero
        var path = ResolveNugetFromLocalDirectory(packageName, version, null, true);
        if (path == null)
        {
            // Intentar con coincidencia aproximada si falla la exacta
            path = ResolveNugetFromLocalDirectory(packageName, version, null, false);
        }

        if (path != null && !Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(path);
        }

        return path;
    }

    public void UnloadDomain(string domainName)
    {
        if (domainName == "Default") return;
        if (_domains.TryGetValue(domainName, out var alc))
        {
            Log.Info($"Unloading domain: {domainName}");
            UnloadService(domainName);
            UnloadModule(domainName);
            alc.Unload();
            _domains.Remove(domainName);
            DomainChanged?.Invoke();
        }
    }

    public void LoadDomain(string domainName)
    {
        if (domainName == "Default") return;
        
        // Si no existe, lo creamos
        if (!_domains.ContainsKey(domainName))
        {
            Log.Info($"Loading/Creating domain: {domainName}");
            var alc = new AssemblyLoadContext(domainName, isCollectible: true);
            alc.Resolving += ResolveAssemblyFromALC;
            _domains[domainName] = alc;
        }

        // Cargamos los módulos y paquetes forzados para este dominio
        LoadForecedPackages(_settings.ForcedPackages.Where(x => x.Domain == domainName).ToList());
        foreach (var moduleItem in _settings.List.Where(item => item.Active && item.Domain == domainName))
        {
            LoadModuleAssembly(moduleItem);
            if (moduleItem.LoadAdditionalDlls == true && moduleItem.Path != null)
            {
                LoadAdditinalAssemblies(moduleItem.Path);
            }
        }

        LoadModule(domainName);
        LoadService(domainName);

        DomainChanged?.Invoke();
    }

    public void LoadDomain(string domainName, IEnumerable<byte[]> assemblies)
    {
        if (domainName == "Default") return;

        // Si no existe, lo creamos
        if (!_domains.TryGetValue(domainName, out var alc))
        {
            Log.Info($"Creating domain from memory: {domainName}");
            alc = new AssemblyLoadContext(domainName, isCollectible: true);
            alc.Resolving += ResolveAssemblyFromALC;
            _domains[domainName] = alc;
        }

        foreach (var asmData in assemblies)
        {
            try
            {
                using var ms = new MemoryStream(asmData);
                alc.LoadFromStream(ms);
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading assembly from stream into domain {domainName}: {ex.Message}");
            }
        }

        LoadModule(domainName);
        LoadService(domainName);

        DomainChanged?.Invoke();
    }

    public void ReloadDomain(string domainName)
    {
        if (domainName == "Default") return;
        Log.Info($"Reloading domain: {domainName}");
        UnloadDomain(domainName);
        LoadDomain(domainName);
    }

    public void UnloadModule(string domainName)
    {
        Log.Info($"UnloadModule for domain: {domainName}");
        UnloadCommandsByDomain(domainName);
    }

    /// <summary>
    /// Elimina de la lista de comandos aquellos que pertenezcan al dominio especificado.
    /// </summary>
    /// <param name="domainName">Nombre del dominio (ALC).</param>
    public void UnloadCommandsByDomain(string domainName)
    {
        var gesCom = _environment.HHost?.Services?.GetService(typeof(IHGesCom)) as IHGesCom
                     ?? _environment.GetService<IHGesCom>();
        gesCom?.UnloadCommandsByDomain(domainName);
    }

    public void UnloadService(string domainName)
    {
        Log.Info($"Unloading services for domain: {domainName}");
        if (domainName == "Default") return;

        if (_domains.TryGetValue(domainName, out var alc))
        {
            var domainAssemblies = alc.Assemblies.ToList();
            var services = _environment.HHost.Services.GetServices<BackgroundService>().ToList();

            foreach (var service in services)
            {
                var serviceType = service.GetType();
                var serviceAssembly = serviceType.Assembly;

                if (domainAssemblies.Any(a => a.FullName == serviceAssembly.FullName))
                {
                    Log.Info($"Stopping service: {serviceType.FullName} in domain {domainName}");
                    try
                    {
                        service.StopAsync(CancellationToken.None).Wait();

                        if (_dynamicServices.Contains(service))
                        {
                            _dynamicServices.Remove(service);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error stopping service {serviceType.FullName}: {ex.Message}");
                    }
                }
            }
        }
    }

    public void LoadModule(string domainName)
    {
        Log.Info($"LoadModule for domain: {domainName}");
        LoadCommandsByDomain(domainName);
    }

    /// <summary>
    /// Cargamos los comandos de los ensamblados asociados a un dominio específico.
    /// </summary>
    /// <param name="domainName">El nombre del dominio (ALC).</param>
    public void LoadCommandsByDomain(string domainName)
    {
        try
        {
            var processedAssemblies = new HashSet<string>();
            var assembliesByDomain = this.AssembliesByDomain;

            if (assembliesByDomain.TryGetValue(domainName, out var assemblies))
            {
                Log.Info($"Cargando comandos para el dominio: {domainName}");
                ProcessAssemblies(assemblies, domainName, processedAssemblies);
            }
            else
            {
                Log.Warn($"No se encontraron ensamblados para el dominio {domainName}");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error en LoadCommandsByDomain para {domainName}: {e.Message}");
        }
    }

    /// <summary>
    /// Procesa una lista de ensamblados para buscar y cargar módulos de Horizonte.
    /// </summary>
    /// <param name="assemblies">Lista de ensamblados a procesar.</param>
    /// <param name="domainName">Nombre del dominio (Application Load Context) al que pertenecen los ensamblados.</param>
    /// <param name="processedAssemblies">Conjunto de nombres completos de ensamblados que ya han sido procesados para evitar duplicados.</param>
    private void ProcessAssemblies(List<Assembly> assemblies, string domainName, HashSet<string> processedAssemblies)
    {
        var assembliesToProcess = new Queue<Assembly>(assemblies);
        var gesCom = _environment.HHost?.Services?.GetService(typeof(IHGesCom)) as IHGesCom 
                     ?? _environment.GetService<IHGesCom>();

        while (assembliesToProcess.Count > 0)
        {
            var assembly = assembliesToProcess.Dequeue();
            if (processedAssemblies.Contains(assembly.FullName!)) continue;
            processedAssemblies.Add(assembly.FullName!);

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }
            catch (Exception e)
            {
                Log.Error($"Error al obtener tipos del ensamblado {assembly.FullName} en dominio {domainName}: {e.Message}");
                continue;
            }

            var modulostypes = (from type in types
                                where Attribute.IsDefined(type, typeof(HorizonteModule))
                                select type).ToList();

            foreach (var modtype in modulostypes)
            {
                try
                {
                    if (modtype == null) continue;
                    object? modInstance;
                    try
                    {
                        Log.Info($">>>> Loading modules from '{modtype.FullName}' in domain '{domainName}'");

                        modInstance = CreateInstance(modtype);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error al crear instancia: {modtype.FullName} en dominio {domainName}", e);
                        continue;
                    }

                    var metodos = modtype.GetMethods().Where(t => t.IsDefined(typeof(HorizonteCommand)));
                    foreach (var method in metodos)
                    {
                        if (modInstance != null)
                        {
                            AddCommandToGesCom(gesCom, method, modInstance, domainName);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
        }
    }

    private void AddCommandToGesCom(IHGesCom? gesCom, MethodInfo method, object instance, string domain)
    {
        if (gesCom == null) return;

        var hAttrib = method.GetCustomAttribute<HorizonteCommand>();
        if (hAttrib == null) return;

        var roleAttrib = (method.GetCustomAttributes(typeof(HorizonteRole), false)
            as HorizonteRole[] ?? []).ToList().Select(x => x.Role).ToList();

        var miCmd = new HCommand
        {
            CommandKey = hAttrib.Key,
            Description = hAttrib.Description,
            CommandAction = method,
            Instance = instance,
            InTypes = method.GetParameters().Select(p => p.ParameterType).ToList(),
            InNames = method.GetParameters()
                            .Where(x => x.Name != null)
                            .Select(parameter => parameter.Name!)
                            .ToList(),
            OutType = method.ReturnType,
            Roles = roleAttrib,
            IsAsync = method.ReturnType == typeof(Task) ||
                      (method.ReturnType.IsGenericType &&
                       method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)),
            Domain = domain
        };
        gesCom.RegisterCommand(miCmd);
    }

    public void LoadService(string domainName)
    {
        Log.Info($"LoadService for domain: {domainName}");
        if (domainName == "Default") return;

        if (_domains.TryGetValue(domainName, out var alc))
        {
            var workerSettings = _environment.GetService<IHContext>()?.Get<WorkerSettings>() ?? new WorkerSettings();
            
            foreach (var workerSetting in workerSettings.List)
            {
                if (!workerSetting.RunOnStart) continue;

                var typeName = workerSetting.WorkerType.Split(',')[0].Trim();
                var type = alc.Assemblies.Select(a => a.GetType(typeName)).FirstOrDefault(t => t != null);

                if (type != null && typeof(IHorizonteBackgroundService).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    try
                    {
                        Log.Info($"Instantiating and starting service: {workerSetting.ServiceName} in domain {domainName}");
                        if (CreateInstance(type) is BackgroundService worker)
                        {
                            worker.StartAsync(CancellationToken.None).Wait();
                            _dynamicServices.Add(worker);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error starting service {type.FullName} in domain {domainName}: {ex.Message}");
                    }
                }
            }
        }
    }

    public object? CreateInstance(Type type)
    {
        try
        {
            // Intentar crear instancia con el constructor que acepta HorizonteEnv
            return Activator.CreateInstance(type, _environment);
        }
        catch
        {
            // Fallback a constructor por defecto si falla
            return Activator.CreateInstance(type);
        }
    }




    private string GetRequestedFramework()
    {
        var frameworkName = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
        // FrameworkDescription suele ser ".NET 10.0.0" o similar.
        // AppContext.TargetFrameworkName suele ser ".NETCoreApp,Version=v10.0"
        var targetFramework = AppContext.TargetFrameworkName;
        
        if (string.IsNullOrEmpty(targetFramework))
        {
            // Fallback si no está disponible
            return "net10.0";
        }

        // ".NETCoreApp,Version=v10.0" -> "net10.0"
        var parts = targetFramework.Split(',');
        if (parts.Length > 1 && parts[1].Trim().StartsWith("Version=v"))
        {
            var version = parts[1].Trim().Substring("Version=v".Length);
            if (parts[0].Contains(".NETCoreApp"))
            {
                return $"net{version}";
            }
            if (parts[0].Contains(".NETStandard"))
            {
                return $"netstandard{version}";
            }
        }
        
        return "net10.0";
    }

    private (string Name, string Version) ParseAssemblyName(string assemblyFullName)
    {
        string name = assemblyFullName.Split(',')[0].Trim();
        string version = string.Empty;
        var parts = assemblyFullName.Split(',');
        foreach (var part in parts)
        {
            if (part.Trim().StartsWith("Version="))
            {
                version = part.Trim().Substring("Version=".Length);
                break;
            }
        }
        return (name, version);
    }

    private string? ResolveNugetFromLocalDirectory(string name, string version, string? framework = null, bool exactmatch = true)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (string.IsNullOrEmpty(version)) return null;

        string frameworkSolicitado = framework ?? GetRequestedFramework();
        bool exactFramework = !string.IsNullOrEmpty(framework);

        foreach (var searchPath in _searchPaths)
        {
            var directoriopaquete = Directory.GetDirectories(searchPath, name.ToLower());
            if (!directoriopaquete.Any()) continue;

            if (exactmatch)
            {
                // Buscar directamente en el directorio con el nombre completo (package/version)
                var pathConVersion = Path.Combine(directoriopaquete.First(), version);
                if (Directory.Exists(pathConVersion))
                {
                    var versionList = GetNugetPackageVersionInformation(directoriopaquete.First());
                    var versionSeleccionada = SelectedVersion(version, frameworkSolicitado, versionList, true, exactFramework);
                    if (versionSeleccionada != null) return versionSeleccionada.DllPath;
                }
            }
            else
            {
                var versionList = GetNugetPackageVersionInformation(directoriopaquete.First());
                var versionSeleccionada = SelectedVersion(version, frameworkSolicitado, versionList, false, exactFramework);
                if (versionSeleccionada != null) return versionSeleccionada.DllPath;
            }
        }

        return null;
    }

    private class NugetPackageVersionInformation
    {
        public string PackageId { get; set; } = string.Empty;
        public string VersionString { get; set; } = string.Empty;
        public Version Version { get; set; } = new Version(0, 0, 0);
        public string Framework { get; set; } = string.Empty;
        public string DllPath { get; set; } = string.Empty;
    }

    private List<NugetPackageVersionInformation> GetNugetPackageVersionInformation(string PackageDirectory)
    {
        var result = new List<NugetPackageVersionInformation>();
        try
        {
            // Obtenemos la lista de archivos .dll en el directorio del paquete
            var list = Directory.EnumerateFiles(PackageDirectory, "*.dll", SearchOption.AllDirectories)
                .Where(x => x.Contains("/lib/") || x.Contains(@"\lib\")).ToList();

            foreach (var dllPath in list)
            {
                // Dividimos el path en segmentos para extraer la información necesaria
                var pathSegments = dllPath.Split(Path.DirectorySeparatorChar);

                var packageId = pathSegments[pathSegments.Length - 5];
                var versionRaw = pathSegments[pathSegments.Length - 4];
                var framework = pathSegments[pathSegments.Length - 2];

                if (dllPath.Contains("/buildTransitive/") || dllPath.Contains(@"\buildTransitive\")) continue;
                if (dllPath.Contains("/build/") || dllPath.Contains(@"\build\")) continue;

                string versionForParsing = versionRaw;
                if (versionForParsing.Contains("-")) versionForParsing = versionForParsing.Split("-")[0];

                if (Version.TryParse(versionForParsing, out var parsedVersion))
                {
                    result.Add(new NugetPackageVersionInformation
                    {
                        PackageId = packageId,
                        VersionString = versionRaw,
                        Version = parsedVersion,
                        Framework = framework,
                        DllPath = dllPath
                    });
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e);
        }

        return result;
    }

    private NugetPackageVersionInformation? SelectedVersion(
        string versionSolicitadaRaw,
        string frameworkSolicitado,
        List<NugetPackageVersionInformation> versionesDisponibles,
        bool exactmatch,
        bool exactFramework = false)
    {
        var prioridadFrameworks = new List<string> { frameworkSolicitado };
        if (!exactFramework)
            prioridadFrameworks.AddRange(_settings.FrameworkPriorities);

        if (exactmatch)
        {
            foreach (var framework in prioridadFrameworks)
            {
                var encontrada = versionesDisponibles.FirstOrDefault(info =>
                    info.Framework.Equals(framework, StringComparison.OrdinalIgnoreCase) &&
                    info.VersionString.Equals(versionSolicitadaRaw, StringComparison.OrdinalIgnoreCase));
                if (encontrada != null) return encontrada;
            }
            return null;
        }

        // Heurística para no exactmatch
        string vParsable = versionSolicitadaRaw;
        if (vParsable.Contains("-")) vParsable = vParsable.Split("-")[0];
        if (!Version.TryParse(vParsable, out var vS)) return null;

        foreach (var framework in prioridadFrameworks)
        {
            var versionesFiltradas = versionesDisponibles
                .Where(info => info.Framework.Equals(framework, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!versionesFiltradas.Any()) continue;

            // Intentar 1.0.0 (Exacta Major.Minor.Build)
            var match = versionesFiltradas.FirstOrDefault(info =>
                info.Version.Major == vS.Major && info.Version.Minor == vS.Minor && info.Version.Build == vS.Build);
            if (match != null) return match;

            // Intentar 1.0.? (Superior más cercana en el mismo Major.Minor)
            match = versionesFiltradas
                .Where(info => info.Version.Major == vS.Major && info.Version.Minor == vS.Minor)
                .OrderBy(info => info.Version)
                .FirstOrDefault(info => info.Version >= vS);
            if (match != null) return match;

            // Intentar 1.? (Superior más cercana en el mismo Major)
            match = versionesFiltradas
                .Where(info => info.Version.Major == vS.Major)
                .OrderBy(info => info.Version)
                .FirstOrDefault(info => info.Version >= vS);
            if (match != null) return match;

            // Si no, la más reciente de ese framework
            return versionesFiltradas.OrderByDescending(info => info.Version).First();
        }

        return null;
    }


    private void DownloadAndExtractPackage(string packageName, string version)
    {
        if (string.IsNullOrEmpty(packageName)) return;
        if (string.IsNullOrEmpty(version)) return;

        var packageFileName = ResolveNugetFromRemoteServer(packageName, version);
        
        if (string.IsNullOrEmpty(packageFileName)) return;

        try
        {
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
        }
        catch (Exception ex)
        {
            Log.Error($"Error extracting package from repository: {ex.Message}");
        }
    }

    private string? ResolveNugetFromRemoteServer(string packageName, string version)
    {
        foreach (var nugetServer in _settings.NugetServers.OrderBy(x => x.Order))
        {
            if (!nugetServer.Active) continue;

            // 1. Intentar descarga directa (Exact Match)
            var directUrl = GetDownloadUrl(nugetServer, packageName, version);
            Log.Info($"Trying direct download of '{packageName}' version '{version}' from {directUrl}");
            var result = DownloadPackage(directUrl, packageName, version);
            if (result != null) return result;

            // 2. Si falla, intentar normalizar la versión (ej. 1.0.0.0 -> 1.0.0)
            if (version.EndsWith(".0"))
            {
                var parts = version.Split('.');
                if (parts.Length > 3)
                {
                    var normalizedVersion = string.Join(".", parts.Take(3));
                    var normalizedUrl = GetDownloadUrl(nugetServer, packageName, normalizedVersion);
                    Log.Info($"Trying normalized version download of '{packageName}' version '{normalizedVersion}' from {normalizedUrl}");
                    result = DownloadPackage(normalizedUrl, packageName, normalizedVersion);
                    if (result != null) return result;
                }
            }

            // 3. Si falla, aplicar heurística consultando versiones al servidor
            Log.Info($"Exact version '{version}' not found for '{packageName}' on {nugetServer.Name}. Fetching available versions...");
            var availableVersions = GetRemotePackageVersions(nugetServer, packageName);
            if (!availableVersions.Any()) continue;

            var bestVersion = SelectBestRemoteVersion(version, availableVersions);
            if (bestVersion != null && bestVersion != version)
            {
                var heuristicUrl = GetDownloadUrl(nugetServer, packageName, bestVersion);
                Log.Info($"Heuristic match: version '{bestVersion}' for '{packageName}'. Downloading from {heuristicUrl}");
                result = DownloadPackage(heuristicUrl, packageName, bestVersion);
                if (result != null) return result;
            }
        }

        return null;
    }

    private string GetDownloadUrl(NugetServerItem server, string packageName, string version)
    {
        switch (server.Version)
        {
            case 1:
            case 2:
                return Path.Combine(server.Server, "package", packageName, version);
            case 3:
                // Nota: V3 suele requerir una estructura más compleja.
                // Usamos una ruta común para el recurso FlatContainer que es lo que espera BaGet y NuGet.org para descargas directas.
                // server.Server suele ser la base de la API V3 o el service index.
                var baseDownloadUrl = server.Server;
                if (baseDownloadUrl.EndsWith("index.json", StringComparison.OrdinalIgnoreCase))
                {
                    baseDownloadUrl = baseDownloadUrl.Substring(0, baseDownloadUrl.Length - 10).TrimEnd('/');
                }
                
                // Si la URL no contiene 'package' y no es nuget.org, intentamos añadirlo como fallback común para BaGet
                if (!baseDownloadUrl.Contains("/package", StringComparison.OrdinalIgnoreCase) && 
                    !baseDownloadUrl.Contains("api.nuget.org", StringComparison.OrdinalIgnoreCase))
                {
                    baseDownloadUrl = baseDownloadUrl.TrimEnd('/') + "/package";
                }

                return $"{baseDownloadUrl.TrimEnd('/')}/{packageName.ToLower()}/{version}/{packageName.ToLower()}.{version}.nupkg";
            default:
                return string.Empty;
        }
    }

    private string? DownloadPackage(string url, string packageName, string version)
    {
        if (string.IsNullOrEmpty(url)) return null;
        var packageFileName = Path.Combine(Path.GetTempPath(), $"{packageName}.{version}.nupkg");
        try
        {
            using var httpClient = new HttpClient();
            using var response = httpClient.GetAsync(url).Result;
            if (!response.IsSuccessStatusCode) return null;

            using (var fileStream = new FileStream(packageFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                response.Content.CopyToAsync(fileStream).Wait();
            }

            Log.Info($"Package downloaded successfully: {packageFileName}");
            return packageFileName;
        }
        catch (Exception ex)
        {
            Log.Error($"Error downloading package from {url}: {ex.Message}");
            return null;
        }
    }

    private List<string> GetRemotePackageVersions(NugetServerItem server, string packageName)
    {
        var versions = new List<string>();
        try
        {
            using var httpClient = new HttpClient();
            if (server.Version <= 2)
            {
                // NuGet V2: FindPackagesById
                // Usualmente: {server.Server}/FindPackagesById()?id='{packageName}'
                // Pero basándonos en el código previo, server.Server parece ser la base para /package/
                // Intentamos una ruta común para OData
                var url = $"{server.Server.Replace("/package", "")}/FindPackagesById()?id='{packageName}'";
                var response = httpClient.GetStringAsync(url).Result;
                var doc = XDocument.Parse(response);
                XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
                XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
                versions = doc.Descendants(m + "properties")
                              .Select(p => p.Element(d + "Version")?.Value)
                              .Where(v => v != null)
                              .Cast<string>()
                              .ToList();
            }
            else if (server.Version == 3)
            {
                // NuGet V3: Registration Resource
                var serviceIndexUrl = server.Server;
                if (!serviceIndexUrl.EndsWith("index.json", StringComparison.OrdinalIgnoreCase))
                {
                    serviceIndexUrl = serviceIndexUrl.TrimEnd('/') + "/index.json";
                }

                string? registrationUrl = null;
                if (serviceIndexUrl.Contains("api.nuget.org", StringComparison.OrdinalIgnoreCase))
                {
                    registrationUrl = "https://api.nuget.org/v3/registration5-semver1";
                }
                else
                {
                    try
                    {
                        var serviceIndexResponse = httpClient.GetStringAsync(serviceIndexUrl).Result;
                        // Buscamos "@type": "RegistrationsBaseUrl" o similar
                        var regMatch = System.Text.RegularExpressions.Regex.Match(serviceIndexResponse, "\"@id\"\\s*:\\s*\"([^\"]+)\"[^}]*\"@type\"\\s*:\\s*\"RegistrationsBaseUrl(/[^\"]+)?\"");
                        if (!regMatch.Success)
                        {
                            regMatch = System.Text.RegularExpressions.Regex.Match(serviceIndexResponse, "\"@type\"\\s*:\\s*\"RegistrationsBaseUrl(/[^\"]+)?\"[^}]*\"@id\"\\s*:\\s*\"([^\"]+)\"");
                            if (regMatch.Success && regMatch.Groups.Count > 2)
                            {
                                registrationUrl = regMatch.Groups[2].Value;
                            }
                        }
                        else if (regMatch.Groups.Count > 1)
                        {
                            registrationUrl = regMatch.Groups[1].Value;
                        }
                    }
                    catch
                    {
                        // Fallback heurístico para BaGet: si el server es .../v3, la registración suele estar en .../v3/registration
                        registrationUrl = server.Server.TrimEnd('/');
                        if (registrationUrl.EndsWith("/index.json", StringComparison.OrdinalIgnoreCase))
                            registrationUrl = registrationUrl.Substring(0, registrationUrl.Length - 11);
                        
                        if (!registrationUrl.EndsWith("/registration", StringComparison.OrdinalIgnoreCase))
                            registrationUrl += "/registration";
                    }
                }

                if (registrationUrl != null)
                {
                    var url = $"{registrationUrl.TrimEnd('/')}/{packageName.ToLower()}/index.json";
                    using var responseMessage = httpClient.GetAsync(url).Result;
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var response = responseMessage.Content.ReadAsStringAsync().Result;
                        var matches = System.Text.RegularExpressions.Regex.Matches(response, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                        foreach (System.Text.RegularExpressions.Match match in matches)
                        {
                            if (match.Groups.Count > 1)
                                versions.Add(match.Groups[1].Value);
                        }
                    }
                    else
                    {
                        Log.Warn($"Failed to fetch versions from {url}. Status: {responseMessage.StatusCode}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error fetching versions for {packageName} from {server.Name}: {ex.Message}");
        }
        return versions.Distinct().ToList();
    }

    private string? SelectBestRemoteVersion(string versionSolicitadaRaw, List<string> versionesDisponibles)
    {
        if (!versionesDisponibles.Any()) return null;

        string vParsable = versionSolicitadaRaw;
        if (vParsable.Contains("-")) vParsable = vParsable.Split("-")[0];
        if (!Version.TryParse(vParsable, out var vS)) return null;

        var parsedVersions = versionesDisponibles.Select(v => {
            string vp = v;
            if (vp.Contains("-")) vp = vp.Split("-")[0];
            Version.TryParse(vp, out var ver);
            return new { Raw = v, Parsed = ver ?? new Version(0,0,0) };
        }).ToList();

        // 1. Intentar Exacta Major.Minor.Build
        var match = parsedVersions.FirstOrDefault(v => 
            v.Parsed.Major == vS.Major && v.Parsed.Minor == vS.Minor && v.Parsed.Build == vS.Build);
        if (match != null) return match.Raw;

        // 2. Superior más cercana en el mismo Major.Minor
        match = parsedVersions
            .Where(v => v.Parsed.Major == vS.Major && v.Parsed.Minor == vS.Minor)
            .OrderBy(v => v.Parsed)
            .FirstOrDefault(v => v.Parsed >= vS);
        if (match != null) return match.Raw;

        // 3. Superior más cercana en el mismo Major
        match = parsedVersions
            .Where(v => v.Parsed.Major == vS.Major)
            .OrderBy(v => v.Parsed)
            .FirstOrDefault(v => v.Parsed >= vS);
        if (match != null) return match.Raw;

        // 4. Si es una versión 1.0.0.0, intentar buscar 1.0.0 exactamente
        if (versionSolicitadaRaw.EndsWith(".0"))
        {
            var parts = versionSolicitadaRaw.Split('.');
            if (parts.Length > 3)
            {
                var v3 = string.Join(".", parts.Take(3));
                var match3 = parsedVersions.FirstOrDefault(v => v.Raw == v3);
                if (match3 != null) return match3.Raw;
            }
        }

        // 5. La más reciente
        return parsedVersions.OrderByDescending(v => v.Parsed).FirstOrDefault()?.Raw;
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
            //Primero cargamos los paquetes forzados
            LoadForecedPackages(_settings.ForcedPackages);
            
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

    private void LoadForecedPackages(List<ForcedPackageItem> ForcedPackages)
    {
        foreach (var package in ForcedPackages)
        {
            try
            {
                Log.Info($"Loading forced package: {package.PackageId} version {package.Version} framework {package.Framework}");
                
                // Intentar resolver localmente con versión y framework exactos
                var dllPath = ResolveNugetFromLocalDirectory(package.PackageId, package.Version, package.Framework, true);
                
                if (dllPath == null)
                {
                    // Intentar descargar si no se encuentra localmente
                    DownloadAndExtractPackage(package.PackageId, package.Version);
                    // Volver a intentar resolver después de la descarga
                    dllPath = ResolveNugetFromLocalDirectory(package.PackageId, package.Version, package.Framework, true);
                }

                if (dllPath != null)
                {
                    if (!Path.IsPathRooted(dllPath))
                        dllPath = Path.GetFullPath(dllPath);

                    Log.Info($"Forced package {package.PackageId} resolved to: {dllPath}");
                    
                    var alc = AssemblyLoadContext.Default;
                    if (!string.IsNullOrEmpty(package.Domain) && package.Domain != "Default")
                    {
                        if (_domains.TryGetValue(package.Domain, out var customAlc))
                        {
                            alc = customAlc;
                        }
                    }
                    
                    alc.LoadFromAssemblyPath(dllPath);
                    
                    // Registrar assets del paquete
                    _environment.StaticFileRegistry.RegisterPackageDirectory(dllPath);

                    // Procesar archivos .targets / .props para crear enlaces simbólicos
                    try
                    {
                        var symLinks = GetContentMappingsFromPackage(dllPath);
                        if (symLinks.Any())
                        {
                            _linkScafolder.BuildScafolder(symLinks);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error processing MSBuild targets for symlinks in forced package: {ex.Message}");
                    }
                }
                else
                {
                    Log.Error($"Could not resolve forced package: {package.PackageId} {package.Version} for framework {package.Framework}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading forced package {package.PackageId}: {ex.Message}");
            }
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
            var alc = AssemblyLoadContext.Default;
            if (!string.IsNullOrEmpty(moduleItem.Domain) && moduleItem.Domain != "Default")
            {
                if (_domains.TryGetValue(moduleItem.Domain, out var customAlc))
                {
                    alc = customAlc;
                }
            }

            if (File.Exists(moduleItem.Path))
            {
                var fullPath = Path.IsPathRooted(moduleItem.Path) 
                    ? moduleItem.Path 
                    : Path.GetFullPath(moduleItem.Path);

                Log.Info($"Loading module from: {fullPath}");

                // Carga el ensamblado desde la ruta especificada.
                var loadedassembly = alc.LoadFromAssemblyPath(fullPath);

                // Agrega los activos y el archivo de documentación al entorno.
                _environment.StaticFileRegistry.RegisterModuleDirectory(moduleItem);
                //AddAssetsFolder(moduleItem);
            }
            else
            {
                var args = new ResolveEventArgs(
                    $"{moduleItem.ModuleName}, Version={moduleItem.ModuleVersion}"); //, Culture=neutral, PublicKeyToken=null

                var nugetassembly = ResolveAssemblyFromNuGetPackages(alc, args);
                if (nugetassembly == null)
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
                    if (!this.Assemblies.Any(a => a.FullName == assemblyName))
                    {
                        var fullDllPath = Path.GetFullPath(dllFile);
                        // Si no, lo carga en el ALC por defecto (comportamiento legacy para DLLs adicionales en la misma carpeta)
                        AssemblyLoadContext.Default.LoadFromAssemblyPath(fullDllPath);
                        Log.Info($"--Loading additional dll: {fullDllPath}");
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