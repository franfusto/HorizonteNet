using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;
using Horizonte.Entities;
using Horizonte.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte;

public class HAssemblyManager : IhAssemblyManager
{


    private static readonly TimeSpan BackgroundServiceStopTimeout = TimeSpan.FromSeconds(30);
    

    private readonly ILogger<HAssemblyManager> _logger;
    private readonly ModulesSettings _settings;
    private readonly List<string> _searchPaths = new();
    private readonly ISymLinkScafolder _linkScafolder;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, AssemblyLoadContext> _domains = new(StringComparer.OrdinalIgnoreCase);

    private List<byte[]> _scriptAssemblyCache = new();

    private readonly Dictionary<string, BackgroundServiceState> _backgroundServices =
        new(StringComparer.OrdinalIgnoreCase);

    private string _installFolder = string.Empty;

    public List<Assembly> Assemblies => AssemblyLoadContext.Default.Assemblies
        .Concat(_domains.Values.SelectMany(x => x.Assemblies)).ToList();

    public StaticFileRegistry StaticFileRegistry { get; private set; }

    public Dictionary<string, List<Assembly>> AssembliesByDomain
    {
        get
        {
            var result = new Dictionary<string, List<Assembly>>
            {
                [Const.DefaultDomainName] = AssemblyLoadContext.Default.Assemblies.ToList()
            };

            foreach (var domain in _domains)
            {
                result[domain.Key] = domain.Value.Assemblies.ToList();
            }

            return result;
        }
    }

    public event Action? DomainChanged;

    public HAssemblyManager(ISymLinkScafolder linkScafolder, IServiceProvider serviceProvider)
    {
        StaticFileRegistry = new StaticFileRegistry();
        _logger = serviceProvider.GetService<ILogger<HAssemblyManager>>()!;
        _settings = serviceProvider.GetService<IHContext>()?.Get<ModulesSettings>() ?? new ModulesSettings();
        _linkScafolder = linkScafolder;
        _serviceProvider = serviceProvider;

        SetUpAssemblyPaths();
        SetUpDomains();
        LoadModulesFromEnvironment();
    }

    private void SetUpAssemblyPaths()
    {
        string systemNugetPackagesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages");

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
                if (string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
                {
                    AssemblyLoadContext.Default.Resolving -= ResolveAssemblyFromALC;
                    AssemblyLoadContext.Default.Resolving += ResolveAssemblyFromALC;
                    continue;
                }

                if (_domains.ContainsKey(domainName))
                    continue;

                _logger.LogInformation("Creando dominio (ALC): {DomainName}", domainName);
                var alc = new AssemblyLoadContext(domainName, isCollectible: true);
                alc.Resolving += ResolveAssemblyFromALC;
                _domains[domainName] = alc;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error configurando dominios.");
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
            var requesterDomain = sender is AssemblyLoadContext senderAlc
                ? AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domains,senderAlc)
                : Const.DefaultDomainName;

            _logger.LogInformation(
                "Resolving assembly: {AssemblyName} requested by domain {DomainName}",
                args.Name,
                requesterDomain);

            var requestedAssemblyName = new AssemblyName(args.Name);
            var sharedAssembly =AssemblyHelpers.TryGetSharedAssemblyFromDefault(requestedAssemblyName);
            if (sharedAssembly != null)
            {
                _logger.LogInformation(
                    "Sharing assembly {AssemblyName} from domain Default instead of loading it into plugin domain.",
                    sharedAssembly.FullName);
                return sharedAssembly;
            }

            var (name, version) = AssemblyHelpers.ParseAssemblyName(args.Name);
            string? resAssemblyPath;

            Assembly? alreadyLoaded = null;

            if (sender is AssemblyLoadContext requestAlc)
            {
                alreadyLoaded = requestAlc.Assemblies.FirstOrDefault(a =>
                    string.Equals(a.GetName().Name, name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                alreadyLoaded = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(a =>
                    string.Equals(a.GetName().Name, name, StringComparison.OrdinalIgnoreCase));
            }

            if (alreadyLoaded != null)
            {
                var loadedAlc = AssemblyLoadContext.GetLoadContext(alreadyLoaded);
                _logger.LogInformation(
                    "Assembly {AssemblyName} ya estaba cargado en el dominio solicitado {DomainName}",
                    alreadyLoaded.FullName,
                    loadedAlc != null ? AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domains,loadedAlc) : Const.DefaultDomainName);

                return alreadyLoaded;
            }

            if (AssemblyHelpers.ShouldSkipNuGetResolution(name))
            {
                _logger.LogInformation(
                    "Skipping NuGet resolution for runtime assembly: {AssemblyName} requested by domain {DomainName}",
                    name,
                    requesterDomain);
                return null;
            }

            resAssemblyPath = ResolveNugetFromLocalDirectory(name, version, null, true);

            if (resAssemblyPath == null)
            {
                resAssemblyPath = ResolveNugetFromLocalDirectory(name, version, null, false);
            }

            if (resAssemblyPath == null)
            {
                DownloadAndExtractPackage(name, version);
                resAssemblyPath = ResolveNugetFromLocalDirectory(name, version, null, false);
            }

            if (resAssemblyPath == null)
            {
                _logger.LogError(
                    "Could not resolve assembly: {AssemblyName} requested by domain {DomainName}",
                    args.Name,
                    requesterDomain);
                return null;
            }

            if (!Path.IsPathRooted(resAssemblyPath))
            {
                resAssemblyPath = Path.GetFullPath(resAssemblyPath);
            }

            _logger.LogInformation(
                "Assembly {AssemblyName} resolved to path {AssemblyPath} for domain {DomainName}",
                args.Name,
                resAssemblyPath,
                requesterDomain);

            StaticFileRegistry.RegisterPackageDirectory(resAssemblyPath, requesterDomain);

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
                _logger.LogError(ex, "Error processing MSBuild targets for symlinks.");
            }

            if (sender is AssemblyLoadContext context)
            {
                var loadedAssembly = context.LoadFromAssemblyPath(resAssemblyPath);
                _logger.LogInformation(
                    "Assembly {AssemblyName} loaded into domain {DomainName}",
                    loadedAssembly.FullName,
                    AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domains,context));
                return loadedAssembly;
            }

            var defaultAssembly = Assembly.LoadFrom(resAssemblyPath);
            _logger.LogInformation(
                "Assembly {AssemblyName} loaded into domain Default",
                defaultAssembly.FullName);
            return defaultAssembly;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trying to resolve assembly from repository.");
            return null;
        }
    }

    public string? ResolveAssemblyDllPath(string packageName, string version)
    {
        var path = ResolveNugetFromLocalDirectory(packageName, version, null, true);

        if (path == null)
        {
            path = ResolveNugetFromLocalDirectory(packageName, version, null, false);
        }

        if (path != null && !Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(path);
        }

        return path;
    }

    public Task UnloadDomain(string domainName)
    {
        if (string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        if (_domains.TryGetValue(domainName, out var alc))
        {
            _logger.LogInformation("Unloading domain: {DomainName}", domainName);

            UnloadService(domainName);
            UnloadModule(domainName);

            alc.Resolving -= ResolveAssemblyFromALC;
            alc.Unload();
            _domains.Remove(domainName);

            DomainChanged?.Invoke();
        }

        return Task.CompletedTask;
    }

    public Task LoadDomain(string domainName)
    {
        if (string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        if (AssemblyHelpers.IsScriptDomain(domainName))
            return LoadScriptDomain(domainName);

        if (!_domains.ContainsKey(domainName))
        {
            _logger.LogInformation("Loading/Creating domain: {DomainName}", domainName);
            var alc = new AssemblyLoadContext(domainName, isCollectible: true);
            alc.Resolving += ResolveAssemblyFromALC;
            _domains[domainName] = alc;
        }

        LoadForecedPackages(_settings.ForcedPackages.Where(x => x.Domain == domainName).ToList());

        foreach (var moduleItem in _settings.List.Where(item => item.Active && item.Domain == domainName))
        {
            LoadModuleAssembly(moduleItem);

            if (moduleItem.LoadAdditionalDlls == true && moduleItem.Path != null)
            {
                LoadAdditinalAssemblies(moduleItem.Path, domainName);
            }
        }

        LoadModule(domainName);
        LoadService(domainName);

        DomainChanged?.Invoke();

        return Task.CompletedTask;
    }

    public Task LoadDomain(string domainName, IEnumerable<byte[]> assemblies)
    {
        if (string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        if (AssemblyHelpers.IsScriptDomain(domainName))
        {
            _scriptAssemblyCache = assemblies
                .Where(x => x.Length > 0)
                .Select(x => x.ToArray())
                .ToList();

            return LoadScriptDomain(domainName);
        }

        if (!_domains.TryGetValue(domainName, out var alc))
        {
            _logger.LogInformation("Creating domain from memory: {DomainName}", domainName);
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
                _logger.LogError(ex, "Error loading assembly from stream into domain {DomainName}", domainName);
            }
        }

        LoadModule(domainName);
        LoadService(domainName);

        DomainChanged?.Invoke();

        return Task.CompletedTask;
    }

    public async Task ReloadDomain(string domainName)
    {
        if (string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
            return;

        _logger.LogInformation("Reloading domain: {DomainName}", domainName);
        await UnloadDomain(domainName);
        await LoadDomain(domainName);
    }

    private Task LoadScriptDomain(string domainName)
    {
        if (!_domains.TryGetValue(domainName, out var alc))
        {
            _logger.LogInformation("Loading/Creating script domain: {DomainName}", domainName);
            alc = new AssemblyLoadContext(domainName, isCollectible: true);
            alc.Resolving += ResolveAssemblyFromALC;
            _domains[domainName] = alc;
        }

        LoadForecedPackages(_settings.ForcedPackages
            .Where(x => string.Equals(x.Domain, domainName, StringComparison.OrdinalIgnoreCase))
            .ToList());

        LoadCachedScriptAssemblies(domainName, alc);

        LoadModule(domainName);
        LoadService(domainName);

        DomainChanged?.Invoke();

        return Task.CompletedTask;
    }

    private void LoadCachedScriptAssemblies(string domainName, AssemblyLoadContext alc)
    {
        if (_scriptAssemblyCache.Count == 0)
        {
            _logger.LogInformation("No cached script assemblies found for domain {DomainName}", domainName);
            return;
        }

        foreach (var asmData in _scriptAssemblyCache)
        {
            try
            {
                using var ms = new MemoryStream(asmData);
                var loadedAssembly = alc.LoadFromStream(ms);

                _logger.LogInformation(
                    "Cached script assembly {AssemblyName} loaded into domain {DomainName}",
                    loadedAssembly.FullName,
                    domainName);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading cached script assembly into domain {DomainName}",
                    domainName);
            }
        }
    }

    public void UnloadModule(string domainName)
    {
        _logger.LogInformation("UnloadModule for domain: {DomainName}", domainName);
        UnloadCommandsByDomain(domainName);
    }

    public void UnloadCommandsByDomain(string domainName)
    {
        var gesCom = _serviceProvider.GetService<IHGesCom>();
        gesCom?.UnloadCommandsByDomain(domainName);
    }

    public void UnloadService(string domainName)
    {
        _logger.LogInformation("Unloading services for domain: {DomainName}", domainName);

        if (string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
            return;

        var keysToProcess = _backgroundServices
            .Where(x => string.Equals(x.Value.DomainName, domainName, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Key)
            .ToList();

        foreach (var key in keysToProcess)
        {
            if (!_backgroundServices.TryGetValue(key, out var state))
                continue;

            try
            {
                if (state.Instance != null)
                {
                    _logger.LogInformation(
                        "Stopping service {WorkerType} in domain {DomainName}",
                        state.WorkerType,
                        state.DomainName);

                    state.RunCancellationTokenSource?.Cancel();

                    state.StopCancellationTokenSource?.Dispose();
                    state.StopCancellationTokenSource = new CancellationTokenSource(BackgroundServiceStopTimeout);

                    state.Instance
                        .StopAsync(state.StopCancellationTokenSource.Token)
                        .GetAwaiter()
                        .GetResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping service {WorkerType}", state.WorkerType);
            }
            finally
            {
                state.StopCancellationTokenSource?.Dispose();
                state.StopCancellationTokenSource = null;

                state.RunCancellationTokenSource?.Dispose();
                state.RunCancellationTokenSource = null;

                state.Instance = null;
                state.IsRunning = false;
            }
        }
    }

    public void LoadModule(string domainName)
    {
        _logger.LogInformation("LoadModule for domain: {DomainName}", domainName);
        LoadCommandsByDomain(domainName);
    }

    public void LoadCommandsByDomain(string domainName)
    {
        try
        {
            var processedAssemblies = new HashSet<string>();
            var assembliesByDomain = AssembliesByDomain;

            if (assembliesByDomain.TryGetValue(domainName, out var assemblies))
            {
                _logger.LogInformation("Cargando comandos para el dominio: {DomainName}", domainName);
                ProcessAssemblies(assemblies, domainName, processedAssemblies);
            }
            else
            {
                _logger.LogWarning("No se encontraron ensamblados para el dominio {DomainName}", domainName);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error en LoadCommandsByDomain para {DomainName}", domainName);
        }
    }

    private void ProcessAssemblies(List<Assembly> assemblies, string domainName, HashSet<string> processedAssemblies)
    {
        var assembliesToProcess = new Queue<Assembly>(assemblies);
        var gesCom = _serviceProvider.GetService<IHGesCom>();

        while (assembliesToProcess.Count > 0)
        {
            var assembly = assembliesToProcess.Dequeue();

            if (assembly.FullName == null || processedAssemblies.Contains(assembly.FullName))
                continue;

            processedAssemblies.Add(assembly.FullName);

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
                _logger.LogError(
                    e,
                    "Error al obtener tipos del ensamblado {AssemblyName} en dominio {DomainName}",
                    assembly.FullName,
                    domainName);
                continue;
            }

            var modulesTypes = (from type in types
                where Attribute.IsDefined(type, typeof(HorizonteModule))
                select type).ToList();

            foreach (var modType in modulesTypes)
            {
                try
                {
                    if (modType == null) continue;

                    object? modInstance;
                    try
                    {
                        _logger.LogInformation(
                            ">>>> Loading modules from '{ModuleType}' in domain '{DomainName}'",
                            modType.FullName,
                            domainName);

                        modInstance = CreateInstance(modType);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(
                            e,
                            "Error al crear instancia: {ModuleType} en dominio {DomainName}",
                            modType.FullName,
                            domainName);
                        continue;
                    }

                    var methods = modType.GetMethods().Where(t => t.IsDefined(typeof(HorizonteCommand)));
                    foreach (var method in methods)
                    {
                        if (modInstance != null)
                        {
                            AddCommandToGesCom(gesCom, method, modInstance, domainName);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error procesando módulo {ModuleType}", modType?.FullName);
                }
            }
        }
    }

    private void AddCommandToGesCom(IHGesCom? gesCom, MethodInfo method, object instance, string domain)
    {
        if (gesCom == null) return;

        var hAttrib = method.GetCustomAttribute<HorizonteCommand>();
        if (hAttrib == null) return;

        var roleAttrib = (method.GetCustomAttributes(typeof(HorizonteRole), false) as HorizonteRole[] ?? [])
            .ToList()
            .Select(x => x.Role)
            .ToList();

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
        _logger.LogInformation("LoadService for domain: {DomainName}", domainName);

        if (string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
            return;

        if (!_domains.ContainsKey(domainName))
            return;

        var hContext = _serviceProvider.GetService<IHContext>();
        var workerSettings = hContext?.Get<WorkerDef>() ?? new WorkerDef();

        foreach (var workerSetting in workerSettings.List.OrderBy(x => x.Order))
        {
            var shouldRestart = workerSetting.RunOnStart;

            if (_backgroundServices.TryGetValue(workerSetting.WorkerType, out var state))
            {
                shouldRestart = shouldRestart || state.RestartOnDomainLoad;
            }

            if (!shouldRestart)
                continue;

            var resolved = ResolveBackgroundServiceType(workerSetting.WorkerType);
            if (resolved == null)
                continue;

            var (_, resolvedDomainName) = resolved.Value;

            if (!string.Equals(resolvedDomainName, domainName, StringComparison.OrdinalIgnoreCase))
                continue;

            StartBackgroundService(workerSetting.WorkerType);
        }
    }

    public object? CreateInstance(Type type)
    {
        try
        {
            return ActivatorUtilities.CreateInstance(_serviceProvider, type);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex,
                "ActivatorUtilities no pudo crear la instancia de {TypeName}. Se intentará resolución manual.",
                type.FullName);
        }

        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .ToList();

        if (constructors.Count == 0)
        {
            throw new MissingMethodException(
                $"El tipo '{type.FullName}' no tiene constructores públicos utilizables.");
        }

        var errors = new List<string>();

        foreach (var constructor in constructors)
        {
            var parameters = constructor.GetParameters();
            var arguments = new object?[parameters.Length];
            var canBuild = true;

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                if (TryResolveConstructorParameter(type, parameter, out var argument))
                {
                    arguments[i] = argument;
                    continue;
                }

                canBuild = false;
                errors.Add(
                    $"No se pudo resolver el parámetro '{parameter.Name}' de tipo '{parameter.ParameterType.FullName}' para el constructor '{constructor}'.");
                break;
            }

            if (!canBuild)
                continue;

            try
            {
                return constructor.Invoke(arguments);
            }
            catch (Exception ex)
            {
                errors.Add(
                    $"Error invocando el constructor '{constructor}' del tipo '{type.FullName}': {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        throw new InvalidOperationException(
            $"No se pudo crear una instancia del tipo '{type.FullName}'. " +
            $"Revise las dependencias del constructor y su registro en DI. " +
            $"Detalles: {string.Join(" | ", errors)}");
    }

    private bool TryResolveConstructorParameter(Type implementationType, ParameterInfo parameter, out object? argument)
    {
        argument = null;

        if (parameter.ParameterType == typeof(IServiceProvider))
        {
            argument = _serviceProvider;
            return true;
        }

        if (parameter.ParameterType == typeof(ILogger))
        {
            var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();
            if (loggerFactory != null)
            {
                argument = loggerFactory.CreateLogger(implementationType.FullName ?? implementationType.Name);
                return true;
            }
        }

        if (parameter.ParameterType.IsGenericType &&
            parameter.ParameterType.GetGenericTypeDefinition() == typeof(ILogger<>))
        {
            var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();
            if (loggerFactory != null)
            {
                var categoryType = parameter.ParameterType.GetGenericArguments()[0];
                var createLoggerMethod = typeof(LoggerFactoryExtensions)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .First(m =>
                        m.Name == nameof(LoggerFactoryExtensions.CreateLogger) &&
                        m.IsGenericMethod &&
                        m.GetParameters().Length == 1);

                argument = createLoggerMethod
                    .MakeGenericMethod(categoryType)
                    .Invoke(null, [loggerFactory]);

                return true;
            }
        }

        var resolved = _serviceProvider.GetService(parameter.ParameterType);
        if (resolved != null)
        {
            argument = resolved;
            return true;
        }

        var defaultDomainType =AssemblyHelpers.TryGetDefaultDomainTypeEquivalent(parameter.ParameterType);
        if (defaultDomainType != null)
        {
            var defaultDomainResolved = _serviceProvider.GetService(defaultDomainType);

            if (defaultDomainResolved != null && parameter.ParameterType.IsInstanceOfType(defaultDomainResolved))
            {
                argument = defaultDomainResolved;
                return true;
            }

            if (defaultDomainResolved != null)
            {
                var parameterAssemblyAlc = AssemblyLoadContext.GetLoadContext(parameter.ParameterType.Assembly);
                var defaultTypeAssemblyAlc = AssemblyLoadContext.GetLoadContext(defaultDomainType.Assembly);
                var implementationAssemblyAlc = AssemblyLoadContext.GetLoadContext(implementationType.Assembly);

                _logger.LogWarning(
                    "Se encontró una instancia raíz para {DefaultDomainType}, pero no es asignable al parámetro {ParameterType}. " +
                    "Esto indica una duplicidad de ensamblado core entre ALCs. ParameterDomain={ParameterDomain}, DefaultTypeDomain={DefaultTypeDomain}, ImplementationDomain={ImplementationDomain}.",
                    defaultDomainType.FullName,
                    parameter.ParameterType.FullName,
                    parameterAssemblyAlc != null
                        ? AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domains,parameterAssemblyAlc)
                        : "Unknown",
                    defaultTypeAssemblyAlc != null
                        ? AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domains,defaultTypeAssemblyAlc)
                        : "Unknown",
                    implementationAssemblyAlc != null
                        ? AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domains,implementationAssemblyAlc)
                        : "Unknown");
            }
        }

        if (parameter.HasDefaultValue)
        {
            argument = parameter.DefaultValue;
            return true;
        }

        return false;
    }

    private string? ResolveNugetFromLocalDirectory(
        string name,
        string version,
        string? framework = null,
        bool exactmatch = true)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (string.IsNullOrEmpty(version)) return null;

        string frameworkSolicitado = framework ?? AssemblyHelpers.GetRequestedFramework();
        bool exactFramework = !string.IsNullOrEmpty(framework);

        foreach (var searchPath in _searchPaths)
        {
            var directorioPaquete = Directory.GetDirectories(searchPath, name.ToLower());
            if (!directorioPaquete.Any()) continue;

            if (exactmatch)
            {
                var pathConVersion = Path.Combine(directorioPaquete.First(), version);
                if (Directory.Exists(pathConVersion))
                {
                    var versionList = GetNugetPackageVersionInformation(directorioPaquete.First());
                    var versionSeleccionada =
                        SelectedVersion(version, frameworkSolicitado, versionList, true, exactFramework);

                    if (versionSeleccionada != null) return versionSeleccionada.DllPath;
                }
            }
            else
            {
                var versionList = GetNugetPackageVersionInformation(directorioPaquete.First());
                var versionSeleccionada =
                    SelectedVersion(version, frameworkSolicitado, versionList, false, exactFramework);

                if (versionSeleccionada != null) return versionSeleccionada.DllPath;
            }
        }

        return null;
    }

    private List<NugetPackageVersionInformation> GetNugetPackageVersionInformation(string packageDirectory)
    {
        var result = new List<NugetPackageVersionInformation>();

        try
        {
            var list = Directory.EnumerateFiles(packageDirectory, "*.dll", SearchOption.AllDirectories)
                .Where(x => x.Contains("/lib/") || x.Contains(@"\lib\"))
                .ToList();

            foreach (var dllPath in list)
            {
                var pathSegments = dllPath.Split(Path.DirectorySeparatorChar);

                var packageId = pathSegments[^5];
                var versionRaw = pathSegments[^4];
                var framework = pathSegments[^2];

                if (dllPath.Contains("/buildTransitive/") || dllPath.Contains(@"\buildTransitive\"))
                    continue;

                if (dllPath.Contains("/build/") || dllPath.Contains(@"\build\"))
                    continue;

                string versionForParsing = versionRaw;
                if (versionForParsing.Contains('-'))
                {
                    versionForParsing = versionForParsing.Split('-')[0];
                }

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
            _logger.LogError(e, "Error obteniendo información de versiones NuGet.");
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
        {
            prioridadFrameworks.AddRange(_settings.FrameworkPriorities);
        }

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

        string vParsable = versionSolicitadaRaw;
        if (vParsable.Contains('-'))
        {
            vParsable = vParsable.Split('-')[0];
        }

        if (!Version.TryParse(vParsable, out var vS))
            return null;

        foreach (var framework in prioridadFrameworks)
        {
            var versionesFiltradas = versionesDisponibles
                .Where(info => info.Framework.Equals(framework, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!versionesFiltradas.Any()) continue;

            var match = versionesFiltradas.FirstOrDefault(info =>
                info.Version.Major == vS.Major &&
                info.Version.Minor == vS.Minor &&
                info.Version.Build == vS.Build);

            if (match != null) return match;

            match = versionesFiltradas
                .Where(info => info.Version.Major == vS.Major && info.Version.Minor == vS.Minor)
                .OrderBy(info => info.Version)
                .FirstOrDefault(info => info.Version >= vS);

            if (match != null) return match;

            match = versionesFiltradas
                .Where(info => info.Version.Major == vS.Major)
                .OrderBy(info => info.Version)
                .FirstOrDefault(info => info.Version >= vS);

            if (match != null) return match;

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
            string targetDirectory = Path.Combine(_installFolder, packageName.ToLower(), version);

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            System.IO.Compression.ZipFile.ExtractToDirectory(packageFileName, targetDirectory, true);
            _logger.LogInformation("Package successfully extracted in: {TargetDirectory}", targetDirectory);

            if (File.Exists(packageFileName))
            {
                File.Delete(packageFileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting package from repository.");
        }
    }

    private string? ResolveNugetFromRemoteServer(string packageName, string version)
    {
        foreach (var nugetServer in _settings.NugetServers.OrderBy(x => x.Order))
        {
            if (!nugetServer.Active) continue;

            var directUrl = GetDownloadUrl(nugetServer, packageName, version);
            _logger.LogInformation(
                "Trying direct download of '{PackageName}' version '{Version}' from {Url}",
                packageName,
                version,
                directUrl);

            var result = DownloadPackage(directUrl, packageName, version);
            if (result != null) return result;

            if (version.EndsWith(".0"))
            {
                var parts = version.Split('.');
                if (parts.Length > 3)
                {
                    var normalizedVersion = string.Join(".", parts.Take(3));
                    var normalizedUrl = GetDownloadUrl(nugetServer, packageName, normalizedVersion);

                    _logger.LogInformation(
                        "Trying normalized version download of '{PackageName}' version '{Version}' from {Url}",
                        packageName,
                        normalizedVersion,
                        normalizedUrl);

                    result = DownloadPackage(normalizedUrl, packageName, normalizedVersion);
                    if (result != null) return result;
                }
            }

            _logger.LogInformation(
                "Exact version '{Version}' not found for '{PackageName}' on {ServerName}. Fetching available versions...",
                version,
                packageName,
                nugetServer.Name);

            var availableVersions = GetRemotePackageVersions(nugetServer, packageName);
            if (!availableVersions.Any()) continue;

            var bestVersion = SelectBestRemoteVersion(version, availableVersions);
            if (bestVersion != null && bestVersion != version)
            {
                var heuristicUrl = GetDownloadUrl(nugetServer, packageName, bestVersion);

                _logger.LogInformation(
                    "Heuristic match: version '{BestVersion}' for '{PackageName}'. Downloading from {Url}",
                    bestVersion,
                    packageName,
                    heuristicUrl);

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
                var baseDownloadUrl = server.Server;

                if (baseDownloadUrl.EndsWith("index.json", StringComparison.OrdinalIgnoreCase))
                {
                    baseDownloadUrl = baseDownloadUrl[..^10].TrimEnd('/');
                }

                if (!baseDownloadUrl.Contains("/package", StringComparison.OrdinalIgnoreCase) &&
                    !baseDownloadUrl.Contains("api.nuget.org", StringComparison.OrdinalIgnoreCase))
                {
                    baseDownloadUrl = baseDownloadUrl.TrimEnd('/') + "/package";
                }

                return
                    $"{baseDownloadUrl.TrimEnd('/')}/{packageName.ToLower()}/{version}/{packageName.ToLower()}.{version}.nupkg";

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

            if (!response.IsSuccessStatusCode)
                return null;

            using (var fileStream = new FileStream(packageFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                response.Content.CopyToAsync(fileStream).Wait();
            }

            _logger.LogInformation("Package downloaded successfully: {PackageFileName}", packageFileName);
            return packageFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading package from {Url}", url);
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

                        var regMatch = System.Text.RegularExpressions.Regex.Match(
                            serviceIndexResponse,
                            "\"@id\"\\s*:\\s*\"([^\"]+)\"[^}]*\"@type\"\\s*:\\s*\"RegistrationsBaseUrl(/[^\"]+)?\"");

                        if (!regMatch.Success)
                        {
                            regMatch = System.Text.RegularExpressions.Regex.Match(
                                serviceIndexResponse,
                                "\"@type\"\\s*:\\s*\"RegistrationsBaseUrl(/[^\"]+)?\"[^}]*\"@id\"\\s*:\\s*\"([^\"]+)\"");

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
                        registrationUrl = server.Server.TrimEnd('/');

                        if (registrationUrl.EndsWith("/index.json", StringComparison.OrdinalIgnoreCase))
                        {
                            registrationUrl = registrationUrl[..^11];
                        }

                        if (!registrationUrl.EndsWith("/registration", StringComparison.OrdinalIgnoreCase))
                        {
                            registrationUrl += "/registration";
                        }
                    }
                }

                if (registrationUrl != null)
                {
                    var url = $"{registrationUrl.TrimEnd('/')}/{packageName.ToLower()}/index.json";
                    using var responseMessage = httpClient.GetAsync(url).Result;

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var response = responseMessage.Content.ReadAsStringAsync().Result;
                        var matches = System.Text.RegularExpressions.Regex.Matches(
                            response,
                            "\"version\"\\s*:\\s*\"([^\"]+)\"");

                        foreach (System.Text.RegularExpressions.Match match in matches)
                        {
                            if (match.Groups.Count > 1)
                            {
                                versions.Add(match.Groups[1].Value);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Failed to fetch versions from {Url}. Status: {StatusCode}",
                            url,
                            responseMessage.StatusCode);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching versions for {PackageName} from {ServerName}", packageName,
                server.Name);
        }

        return versions.Distinct().ToList();
    }

    private string? SelectBestRemoteVersion(string versionSolicitadaRaw, List<string> versionesDisponibles)
    {
        if (!versionesDisponibles.Any()) return null;

        string vParsable = versionSolicitadaRaw;
        if (vParsable.Contains('-'))
        {
            vParsable = vParsable.Split('-')[0];
        }

        if (!Version.TryParse(vParsable, out var vS))
            return null;

        var parsedVersions = versionesDisponibles.Select(v =>
        {
            string vp = v;
            if (vp.Contains('-'))
            {
                vp = vp.Split('-')[0];
            }

            Version.TryParse(vp, out var ver);

            return new
            {
                Raw = v,
                Parsed = ver ?? new Version(0, 0, 0)
            };
        }).ToList();

        var match = parsedVersions.FirstOrDefault(v =>
            v.Parsed.Major == vS.Major &&
            v.Parsed.Minor == vS.Minor &&
            v.Parsed.Build == vS.Build);

        if (match != null) return match.Raw;

        match = parsedVersions
            .Where(v => v.Parsed.Major == vS.Major && v.Parsed.Minor == vS.Minor)
            .OrderBy(v => v.Parsed)
            .FirstOrDefault(v => v.Parsed >= vS);

        if (match != null) return match.Raw;

        match = parsedVersions
            .Where(v => v.Parsed.Major == vS.Major)
            .OrderBy(v => v.Parsed)
            .FirstOrDefault(v => v.Parsed >= vS);

        if (match != null) return match.Raw;

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

        return parsedVersions.OrderByDescending(v => v.Parsed).FirstOrDefault()?.Raw;
    }

    private void LoadModulesFromEnvironment()
    {
        try
        {
            LoadForecedPackages(_settings.ForcedPackages);

            foreach (var moduleItem in _settings.List.Where(item => item.Active))
            {
                LoadModuleAssembly(moduleItem);

                if (moduleItem.LoadAdditionalDlls == true && moduleItem.Path != null)
                {
                    LoadAdditinalAssemblies(moduleItem.Path, moduleItem.Domain);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error loading modules from environment.");
        }
    }

    private void LoadForecedPackages(List<ForcedPackageItem> forcedPackages)
    {
        foreach (var package in forcedPackages)
        {
            try
            {
                if (!package.Active)
                {
                    _logger.LogInformation(
                        "Skipping inactive forced package: {PackageId} version {Version} framework {Framework} target domain {DomainName}",
                        package.PackageId,
                        package.Version,
                        package.Framework,
                        string.IsNullOrWhiteSpace(package.Domain) ? Const.DefaultDomainName : package.Domain);

                    continue;
                }
                _logger.LogInformation(
                    "Loading forced package: {PackageId} version {Version} framework {Framework} target domain {DomainName}",
                    package.PackageId,
                    package.Version,
                    package.Framework,
                    string.IsNullOrWhiteSpace(package.Domain) ? Const.DefaultDomainName : package.Domain);

                var dllPath = ResolveNugetFromLocalDirectory(
                    package.PackageId,
                    package.Version,
                    package.Framework,
                    true);

                if (dllPath == null)
                {
                    DownloadAndExtractPackage(package.PackageId, package.Version);
                    dllPath = ResolveNugetFromLocalDirectory(
                        package.PackageId,
                        package.Version,
                        package.Framework,
                        true);
                }

                if (dllPath != null)
                {
                    if (!Path.IsPathRooted(dllPath))
                    {
                        dllPath = Path.GetFullPath(dllPath);
                    }

                    _logger.LogInformation(
                        "Forced package {PackageId} resolved to path {DllPath} for target domain {DomainName}",
                        package.PackageId,
                        dllPath,
                        string.IsNullOrWhiteSpace(package.Domain) ? Const.DefaultDomainName : package.Domain);

                    var alc = AssemblyLoadContext.Default;
                    if (!string.IsNullOrEmpty(package.Domain) &&
                        !string.Equals(package.Domain, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase) &&
                        _domains.TryGetValue(package.Domain, out var customAlc))
                    {
                        alc = customAlc;
                    }

                    var targetDomainName = AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domains,alc);
                    var assemblyName = AssemblyName.GetAssemblyName(dllPath);

                    var alreadyLoadedInTargetAlc = alc.Assemblies.FirstOrDefault(a =>
                        string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase));

                    if (alreadyLoadedInTargetAlc == null)
                    {
                        var loadedAssembly = alc.LoadFromAssemblyPath(dllPath);
                        _logger.LogInformation(
                            "Forced package {PackageId} ({AssemblyName}) loaded into domain {DomainName} from {DllPath}",
                            package.PackageId,
                            loadedAssembly.FullName,
                            targetDomainName,
                            dllPath);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Forced package {PackageId} ({AssemblyName}) ya estaba cargado en dominio {DomainName}",
                            package.PackageId,
                            alreadyLoadedInTargetAlc.FullName,
                            targetDomainName);
                    }

                    foreach (var loadedAssembly in Assemblies
                                 .Where(a => string.Equals(a.GetName().Name, assemblyName.Name,
                                     StringComparison.OrdinalIgnoreCase)))
                    {
                        var loadedAssemblyAlc = AssemblyLoadContext.GetLoadContext(loadedAssembly);
                        _logger.LogInformation(
                            "Forced package inventory => Assembly {AssemblyName} currently visible in domain {DomainName}",
                            loadedAssembly.FullName,
                            loadedAssemblyAlc != null
                                ? AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domains,loadedAssemblyAlc)
                                : Const.DefaultDomainName);
                    }

                    StaticFileRegistry.RegisterPackageDirectory(dllPath, targetDomainName);

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
                        _logger.LogError(ex, "Error processing MSBuild targets for symlinks in forced package.");
                    }
                }
                else
                {
                    _logger.LogError(
                        "Could not resolve forced package: {PackageId} {Version} for framework {Framework} target domain {DomainName}",
                        package.PackageId,
                        package.Version,
                        package.Framework,
                        string.IsNullOrWhiteSpace(package.Domain) ? Const.DefaultDomainName : package.Domain);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading forced package {PackageId}", package.PackageId);
            }
        }
    }

    private void LoadModuleAssembly(ModulesSettingsItem moduleItem)
    {
        try
        {
            var alc = AssemblyHelpers.GetAssemblyLoadContextByDomain(_domains,moduleItem.Domain);
            var domainName = AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domains,alc);

            if (File.Exists(moduleItem.Path))
            {
                var fullPath = Path.IsPathRooted(moduleItem.Path)
                    ? moduleItem.Path
                    : Path.GetFullPath(moduleItem.Path);

                _logger.LogInformation(
                    "Loading module {ModuleName} from {FullPath} into domain {DomainName}",
                    moduleItem.ModuleName,
                    fullPath,
                    domainName);

                var loadedAssembly = alc.LoadFromAssemblyPath(fullPath);

                _logger.LogInformation(
                    "Module assembly {AssemblyName} loaded into domain {DomainName}",
                    loadedAssembly.FullName,
                    domainName);

                StaticFileRegistry.RegisterModuleDirectory(moduleItem, domainName);
            }
            else
            {
                var args = new ResolveEventArgs($"{moduleItem.ModuleName}, Version={moduleItem.ModuleVersion}");
                var nugetAssembly = ResolveAssemblyFromNuGetPackages(alc, args);

                if (nugetAssembly == null)
                {
                    _logger.LogError(
                        "Can't find module {ModuleName} for domain {DomainName}. Path: {ModulePath}",
                        moduleItem.ModuleName,
                        domainName,
                        moduleItem.Path);
                }
                else
                {
                    _logger.LogInformation(
                        "Module assembly {AssemblyName} resolved for domain {DomainName}",
                        nugetAssembly.FullName,
                        domainName);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error on loading {ModuleName}", moduleItem.ModuleName);
        }
    }

    private void LoadAdditinalAssemblies(string modulePath, string? domainName)
    {
        try
        {
            string directoryPath = Path.GetDirectoryName(modulePath) ?? string.Empty;
            if (!Directory.Exists(directoryPath))
                return;

            var alc = AssemblyHelpers.GetAssemblyLoadContextByDomain(_domains,domainName);
            var targetDomainName = AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domains,alc);

            foreach (var dllFile in Directory.EnumerateFiles(directoryPath, "*.dll"))
            {
                var assemblyNameInfo = AssemblyName.GetAssemblyName(dllFile);

                if (AssemblyHelpers.ShouldShareAssemblyFromDefault(assemblyNameInfo.Name))
                {
                    _logger.LogInformation(
                        "Skipping additional dll {AssemblyName} for domain {DomainName} because it is shared from Default.",
                        assemblyNameInfo.FullName,
                        targetDomainName);
                    continue;
                }

                string assemblyName = assemblyNameInfo.FullName!;

                var isAlreadyLoadedInTargetAlc = alc.Assemblies.Any(a => a.FullName == assemblyName);
                if (isAlreadyLoadedInTargetAlc)
                {
                    _logger.LogInformation(
                        "Additional dll {AssemblyName} ya estaba cargada en dominio {DomainName}",
                        assemblyName,
                        targetDomainName);
                    continue;
                }

                var fullDllPath = Path.GetFullPath(dllFile);
                var loadedAssembly = alc.LoadFromAssemblyPath(fullDllPath);

                _logger.LogInformation(
                    "Additional dll {AssemblyName} loaded from {FullDllPath} into domain {DomainName}",
                    loadedAssembly.FullName,
                    fullDllPath,
                    targetDomainName);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on load additional dll from module: {ModulePath}", modulePath);
        }
    }

    private List<SymLinkDef> GetContentMappingsFromPackage(string resAssemblyPath)
    {
        var mappings = new List<SymLinkDef>();

        try
        {
            var libDir = Directory.GetParent(resAssemblyPath);
            var pkgVersionDir = libDir?.Parent;
            var packagePath = pkgVersionDir?.Parent?.FullName;

            if (string.IsNullOrEmpty(packagePath))
                return mappings;

            var framework = libDir?.Name ?? Const.DefaultFramework;

            string buildDir = Path.Combine(packagePath, "buildTransitive", framework);
            if (!Directory.Exists(buildDir))
            {
                buildDir = Path.Combine(packagePath, "build", framework);
                if (!Directory.Exists(buildDir)) return mappings;
            }

            var definitionFiles = Directory.GetFiles(buildDir, "*.targets")
                .Concat(Directory.GetFiles(buildDir, "*.props"));

            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

            foreach (var defFile in definitionFiles)
            {
                var doc = XDocument.Load(defFile);

                var contentItems = doc.Descendants(ns + "Content")
                    .Where(c => c.Element(ns + "CopyToOutputDirectory") != null);

                foreach (var item in contentItems)
                {
                    string include = item.Attribute("Include")?.Value ?? "";
                    string linkTemplate = item.Element(ns + "Link")?.Value ?? "";

                    if (string.IsNullOrEmpty(include)) continue;

                    string targetsFolder = buildDir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                    string resolvedInclude = include.Replace("$(MSBuildThisFileDirectory)", targetsFolder);
                    resolvedInclude = Path.GetFullPath(resolvedInclude.Replace('\\', Path.DirectorySeparatorChar));

                    if (resolvedInclude.Contains('*'))
                    {
                        string baseDir = resolvedInclude.Split('*')[0];
                        if (!Directory.Exists(baseDir)) continue;

                        var files = Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            string relativePath = Path.GetRelativePath(baseDir, file);
                            string recursiveDir = Path.GetDirectoryName(relativePath) ?? "";
                            if (!string.IsNullOrEmpty(recursiveDir))
                            {
                                recursiveDir += Path.DirectorySeparatorChar;
                            }

                            string finalDest = linkTemplate
                                .Replace("%(RecursiveDir)", recursiveDir)
                                .Replace("%(Filename)", Path.GetFileNameWithoutExtension(file))
                                .Replace("%(Extension)", Path.GetExtension(file))
                                .Replace('\\', Path.DirectorySeparatorChar);

                            if (string.IsNullOrEmpty(finalDest))
                            {
                                finalDest = relativePath;
                            }

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
            _logger.LogError(ex, "Error extracting content mappings.");
        }

        return mappings;
    }

    public bool StartBackgroundService(string workerType)
    {
        workerType = workerType.Trim();
        if (string.IsNullOrWhiteSpace(workerType))
            return false;


        if (_backgroundServices.TryGetValue(workerType, out var existingState) && existingState.IsRunning)
        {
            _logger.LogInformation(
                "El worker {WorkerType} ya está en ejecución en dominio {DomainName}.",
                workerType,
                existingState.DomainName);
            return true;
        }

        var resolved = ResolveBackgroundServiceType(workerType);
        if (resolved == null)
        {
            _logger.LogWarning("Tipo de worker '{WorkerType}' no encontrado.", workerType);
            return false;
        }

        var (serviceType, resolvedDomainName) = resolved.Value;

        _logger.LogInformation(
            "Resolved worker type {WorkerType} to runtime type {RuntimeType} in domain {DomainName}",
            workerType,
            serviceType.FullName,
            resolvedDomainName);

        if (string.Equals(resolvedDomainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "El worker {WorkerType} pertenece al dominio Default. Queda fuera del ciclo de hot reload.",
                workerType);
        }

        CancellationTokenSource? runCancellationTokenSource = null;

        try
        {
            if (CreateInstance(serviceType) is not BackgroundService worker)
            {
                _logger.LogWarning("No se pudo crear la instancia del worker {WorkerType}.", workerType);
                return false;
            }

            _logger.LogInformation(
                "Iniciando worker {WorkerType} en dominio {DomainName}",
                workerType,
                resolvedDomainName);

            runCancellationTokenSource = new CancellationTokenSource();

            worker.StartAsync(runCancellationTokenSource.Token).GetAwaiter().GetResult();

            if (_backgroundServices.TryGetValue(workerType, out var state))
            {
                state.StopCancellationTokenSource?.Dispose();
                state.StopCancellationTokenSource = null;

                state.RunCancellationTokenSource?.Dispose();
                state.DomainName = resolvedDomainName;
                state.Instance = worker;
                state.RunCancellationTokenSource = runCancellationTokenSource;
                runCancellationTokenSource = null;
                state.IsRunning = true;
                state.RestartOnDomainLoad =
                    !string.Equals(resolvedDomainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                _backgroundServices[workerType] = new BackgroundServiceState
                {
                    WorkerType = workerType,
                    DomainName = resolvedDomainName,
                    Instance = worker,
                    RunCancellationTokenSource = runCancellationTokenSource,
                    IsRunning = true,
                    RestartOnDomainLoad =
                        !string.Equals(resolvedDomainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase)
                };

                runCancellationTokenSource = null;
            }

            return true;
        }
        catch (Exception e)
        {
            runCancellationTokenSource?.Dispose();
            _logger.LogError(e, "Error al configurar el worker de tipo '{WorkerType}'", workerType);
            return false;
        }
    }

    public bool StopBackgroundService(string serviceType)
    {
        serviceType =  serviceType.Trim();
        if (string.IsNullOrWhiteSpace(serviceType))
            return false;


        if (!_backgroundServices.TryGetValue(serviceType, out var state))
        {
            _logger.LogWarning("No existe registro para el worker {WorkerType}.", serviceType);
            return false;
        }

        if (state.Instance == null || !state.IsRunning)
        {
            _logger.LogInformation(
                "El worker {WorkerType} ya no estaba ejecutándose en dominio {DomainName}.",
                serviceType,
                state.DomainName);
            state.IsRunning = false;
            state.RestartOnDomainLoad = false;
            return true;
        }

        try
        {
            _logger.LogInformation(
                "Deteniendo worker {WorkerType} en dominio {DomainName}",
                serviceType,
                state.DomainName);

            state.Instance.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
            state.Instance = null;
            state.IsRunning = false;
            state.RestartOnDomainLoad = false;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deteniendo worker {WorkerType}", serviceType);
            return false;
        }
    }

    public bool BackgroundServiceRunning(string serviceType)
    {
        serviceType = serviceType.Trim();
        if (string.IsNullOrWhiteSpace(serviceType))
            return false;


        return _backgroundServices.TryGetValue(serviceType, out var state) &&
               state.IsRunning &&
               state.Instance != null;
    }

    private (Type Type, string DomainName)? ResolveBackgroundServiceType(string workerType)
    {
        var typeName = workerType.Split(',')[0].Trim();

        foreach (var assembly in AssemblyLoadContext.Default.Assemblies)
        {
            var type = assembly.GetType(typeName, throwOnError: false, ignoreCase: false);
            if (type != null && typeof(BackgroundService).IsAssignableFrom(type) && !type.IsAbstract &&
                !type.IsInterface)
            {
                _logger.LogInformation(
                    "Worker type {WorkerType} resolved in domain Default from assembly {AssemblyName}",
                    workerType,
                    assembly.FullName);
                return (type, Const.DefaultDomainName);
            }
        }

        foreach (var domain in _domains)
        {
            foreach (var assembly in domain.Value.Assemblies)
            {
                var type = assembly.GetType(typeName, throwOnError: false, ignoreCase: false);
                if (type != null && typeof(BackgroundService).IsAssignableFrom(type) && !type.IsAbstract &&
                    !type.IsInterface)
                {
                    _logger.LogInformation(
                        "Worker type {WorkerType} resolved in domain {DomainName} from assembly {AssemblyName}",
                        workerType,
                        domain.Key,
                        assembly.FullName);
                    return (type, domain.Key);
                }
            }
        }

        var fallbackType = Type.GetType(workerType, throwOnError: false);
        if (fallbackType != null &&
            typeof(BackgroundService).IsAssignableFrom(fallbackType) &&
            !fallbackType.IsAbstract &&
            !fallbackType.IsInterface)
        {
            var alc = AssemblyLoadContext.GetLoadContext(fallbackType.Assembly) ?? AssemblyLoadContext.Default;
            var domainName = AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domains,alc);

            _logger.LogInformation(
                "Worker type {WorkerType} resolved by fallback in domain {DomainName} from assembly {AssemblyName}",
                workerType,
                domainName,
                fallbackType.Assembly.FullName);

            return (fallbackType, domainName);
        }

        _logger.LogWarning("Worker type {WorkerType} could not be resolved in any domain.", workerType);
        return null;
    }
}