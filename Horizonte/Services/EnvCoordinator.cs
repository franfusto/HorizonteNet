using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Horizonte.Entities;
using Horizonte.Helpers;
using Horizonte.Interfaces;
using Horizonte.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte.Services;

/// <summary>
/// Orquestador central del entorno modular de HorizonteNet y fachada principal.
/// Coordina la gestión de dominios (IHDomainManager), ensamblados/recursos (IHAssemblySolver),
/// comandos/módulos (IHGesCom) y servicios en segundo plano (IhWorkersManager).
/// </summary>
public class EnvCoordinator : IEnvCoordinator
{
    private static readonly TimeSpan BackgroundServiceStopTimeout = TimeSpan.FromSeconds(5);

    private readonly IHDomainManager _domainManager;
    private readonly IHAssemblySolver _assemblySolver;
    private readonly IHGesCom _gesCom;
    private readonly IhWorkersManager _workersManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EnvCoordinator> _logger;
    private readonly ModulesSettings _settings;

    private readonly ConcurrentDictionary<string, BackgroundServiceState> _backgroundServices =
        new(StringComparer.OrdinalIgnoreCase);

    private List<byte[]> _scriptAssemblyCache = new();

    /// <inheritdoc/>
    public IHAssemblySolver AssemblySolver => _assemblySolver;

    /// <inheritdoc/>
    public IHDomainManager DomainManager => _domainManager;

    /// <inheritdoc/>
    public IHGesCom GesCom => _gesCom;

    /// <inheritdoc/>
    public IhWorkersManager WorkersManager => _workersManager;

    /// <inheritdoc/>
    public IReadOnlyList<Assembly> Assemblies => AssemblyLoadContext.Default.Assemblies
        .Concat(_domainManager.Domains.Values.SelectMany(x => x.Assemblies))
        .Distinct()
        .ToList();

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, List<Assembly>> AssembliesByDomain
    {
        get
        {
            var result = new Dictionary<string, List<Assembly>>(StringComparer.OrdinalIgnoreCase)
            {
                [Const.DefaultDomainName] = AssemblyLoadContext.Default.Assemblies.ToList()
            };

            foreach (var domain in _domainManager.Domains)
            {
                result[domain.Key] = domain.Value.Assemblies.ToList();
            }

            return result;
        }
    }

    /// <inheritdoc/>
    public StaticFileRegistry StaticFileRegistry => _assemblySolver.StaticFileRegistry;

    /// <inheritdoc/>
    public event Action? DomainChanged;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="EnvCoordinator"/>.
    /// </summary>
    /// <param name="domainManager">Gestor de dominios y contextos de carga.</param>
    /// <param name="assemblySolver">Resolutor de ensamblados y paquetes NuGet.</param>
    /// <param name="gesCom">Gestor de comandos de la aplicación.</param>
    /// <param name="workersManager">Gestor de trabajadores y servicios en segundo plano.</param>
    /// <param name="serviceProvider">Proveedor de servicios para inyección de dependencias.</param>
    /// <param name="logger">Servicio de registro de eventos para el coordinador.</param>
    public EnvCoordinator(
        IHDomainManager domainManager,
        IHAssemblySolver assemblySolver,
        IHGesCom gesCom,
        IhWorkersManager workersManager,
        IServiceProvider serviceProvider,
        ILogger<EnvCoordinator> logger)
    {
        _domainManager = domainManager;
        _assemblySolver = assemblySolver;
        _gesCom = gesCom;
        _workersManager = workersManager;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = serviceProvider.GetService<IhContext>()?.Get<ModulesSettings>() ?? new ModulesSettings();

        LoadModulesFromEnvironment();
    }

    /// <inheritdoc/>
    public async Task UnloadDomainAsync(string domainName)
    {
        if (string.IsNullOrWhiteSpace(domainName) ||
            string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _logger.LogInformation("Unloading domain: {DomainName}", domainName);

        UnloadService(domainName);
        UnloadModule(domainName);

        await _domainManager.UnloadDomainContextAsync(domainName);

        DomainChanged?.Invoke();
    }

    /// <inheritdoc/>
    public async Task LoadDomainAsync(string domainName)
    {
        if (string.IsNullOrWhiteSpace(domainName) ||
            string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (AssemblyHelpers.IsScriptDomain(domainName))
        {
            await LoadScriptDomainAsync(domainName);
            return;
        }

        _domainManager.CreateDomain(domainName, isCollectible: true);

        LoadForcedPackages(_settings.ForcedPackages.Where(x => string.Equals(x.Domain, domainName, StringComparison.OrdinalIgnoreCase)).ToList());

        foreach (var moduleItem in _settings.List.Where(item => item.Active && string.Equals(item.Domain, domainName, StringComparison.OrdinalIgnoreCase)))
        {
            LoadModuleAssembly(moduleItem);

            if (moduleItem.LoadAdditionalDlls == true && moduleItem.Path != null)
            {
                LoadAdditionalAssemblies(moduleItem.Path, domainName);
            }
        }

        LoadModule(domainName);
        LoadService(domainName);

        DomainChanged?.Invoke();
    }

    /// <inheritdoc/>
    public async Task LoadDomainAsync(string domainName, IEnumerable<byte[]> assemblies)
    {
        if (string.IsNullOrWhiteSpace(domainName) ||
            string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (AssemblyHelpers.IsScriptDomain(domainName))
        {
            _scriptAssemblyCache = assemblies
                .Where(x => x.Length > 0)
                .Select(x => x.ToArray())
                .ToList();

            await LoadScriptDomainAsync(domainName);
            return;
        }

        await _domainManager.LoadDomainContextAsync(domainName, assemblies);

        LoadModule(domainName);
        LoadService(domainName);

        DomainChanged?.Invoke();
    }

    /// <inheritdoc/>
    public async Task ReloadDomainAsync(string domainName)
    {
        if (string.IsNullOrWhiteSpace(domainName) ||
            string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _logger.LogInformation("Reloading domain: {DomainName}", domainName);
        await UnloadDomainAsync(domainName);
        await LoadDomainAsync(domainName);
    }

    /// <inheritdoc/>
    public void UnloadModule(string domainName)
    {
        _logger.LogInformation("UnloadModule for domain: {DomainName}", domainName);
        UnloadCommandsByDomain(domainName);
    }

    /// <inheritdoc/>
    public void UnloadCommandsByDomain(string domainName)
    {
        _gesCom.UnloadCommandsByDomain(domainName);
    }

    /// <inheritdoc/>
    public void LoadModule(string domainName)
    {
        _logger.LogInformation("LoadModule for domain: {DomainName}", domainName);
        LoadCommandsByDomain(domainName);
    }

    /// <inheritdoc/>
    public void LoadCommandsByDomain(string domainName)
    {
        try
        {
            var processedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var domainAssemblies = _domainManager.GetDomainAssemblies(domainName).ToList();

            if (domainAssemblies.Count > 0)
            {
                _logger.LogInformation("Cargando comandos para el dominio: {DomainName}", domainName);
                ProcessAssemblies(domainAssemblies, domainName, processedAssemblies);
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

    /// <summary>
    /// Inicia los servicios en segundo plano configurados para un dominio.
    /// </summary>
    public void LoadService(string domainName)
    {
        _logger.LogInformation("LoadService for domain: {DomainName}", domainName);

        if (string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
            return;

        var hContext = _serviceProvider.GetService<IhContext>();
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

    /// <summary>
    /// Detiene y descarga los servicios en segundo plano asociados a un dominio.
    /// </summary>
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

    /// <inheritdoc/>
    public bool StartBackgroundService(string serviceType)
    {
        serviceType = serviceType.Trim();
        if (string.IsNullOrWhiteSpace(serviceType))
            return false;

        if (_backgroundServices.TryGetValue(serviceType, out var existingState) && existingState.IsRunning)
        {
            _logger.LogInformation(
                "El worker {WorkerType} ya está en ejecución en dominio {DomainName}.",
                serviceType,
                existingState.DomainName);
            return true;
        }

        var resolved = ResolveBackgroundServiceType(serviceType);
        if (resolved == null)
        {
            _logger.LogWarning("Tipo de worker '{WorkerType}' no encontrado.", serviceType);
            return false;
        }

        var (runtimeType, resolvedDomainName) = resolved.Value;

        _logger.LogInformation(
            "Resolved worker type {WorkerType} to runtime type {RuntimeType} in domain {DomainName}",
            serviceType,
            runtimeType.FullName,
            resolvedDomainName);

        if (string.Equals(resolvedDomainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "El worker {WorkerType} pertenece al dominio Default. Queda fuera del ciclo de hot reload.",
                serviceType);
        }

        CancellationTokenSource? runCancellationTokenSource = null;

        try
        {
            if (CreateInstance(runtimeType) is not BackgroundService worker)
            {
                _logger.LogWarning("No se pudo crear la instancia del worker {WorkerType}.", serviceType);
                return false;
            }

            _logger.LogInformation(
                "Iniciando worker {WorkerType} en dominio {DomainName}",
                serviceType,
                resolvedDomainName);

            runCancellationTokenSource = new CancellationTokenSource();

            worker.StartAsync(runCancellationTokenSource.Token).GetAwaiter().GetResult();

            if (_backgroundServices.TryGetValue(serviceType, out var state))
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
                _backgroundServices[serviceType] = new BackgroundServiceState
                {
                    WorkerType = serviceType,
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
            _logger.LogError(e, "Error al configurar el worker de tipo '{WorkerType}'", serviceType);
            return false;
        }
    }

    /// <inheritdoc/>
    public bool StopBackgroundService(string serviceType)
    {
        serviceType = serviceType.Trim();
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

    /// <inheritdoc/>
    public bool BackgroundServiceRunning(string serviceType)
    {
        serviceType = serviceType.Trim();
        if (string.IsNullOrWhiteSpace(serviceType))
            return false;

        return _backgroundServices.TryGetValue(serviceType, out var state) &&
               state.IsRunning &&
               state.Instance != null;
    }

    /// <inheritdoc/>
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

        var defaultDomainType = AssemblyHelpers.TryGetDefaultDomainTypeEquivalent(parameter.ParameterType);
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
                    "Se encontró una instancia raíz para {DefaultDomainType}, pero no es asignable al parámetro {ParameterType}. ParameterDomain={ParameterDomain}, DefaultTypeDomain={DefaultTypeDomain}, ImplementationDomain={ImplementationDomain}.",
                    defaultDomainType.FullName,
                    parameter.ParameterType.FullName,
                    parameterAssemblyAlc?.Name ?? "Default",
                    defaultTypeAssemblyAlc?.Name ?? "Default",
                    implementationAssemblyAlc?.Name ?? "Default");
            }
        }

        if (parameter.HasDefaultValue)
        {
            argument = parameter.DefaultValue;
            return true;
        }

        return false;
    }

    private void LoadModulesFromEnvironment()
    {
        try
        {
            LoadForcedPackages(_settings.ForcedPackages);

            foreach (var moduleItem in _settings.List.Where(item => item.Active))
            {
                LoadModuleAssembly(moduleItem);

                if (moduleItem.LoadAdditionalDlls == true && moduleItem.Path != null)
                {
                    LoadAdditionalAssemblies(moduleItem.Path, moduleItem.Domain);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error loading modules from environment.");
        }
    }

    private void LoadForcedPackages(List<ForcedPackageItem> forcedPackages)
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

                var dllPath = _assemblySolver.ResolveAssemblyDllPath(package.PackageId, package.Version, package.Framework);

                if (dllPath == null)
                {
                    dllPath = _assemblySolver.DownloadNuGetPackageAsync(package.PackageId, package.Version, package.Framework).GetAwaiter().GetResult();
                }

                var cachedRuntime = _assemblySolver.AssemblyCache?.Get(package.PackageId, package.Version);
                if (cachedRuntime != null && cachedRuntime.Found && cachedRuntime.IsRuntimePackage)
                {
                    _logger.LogInformation(
                        "Forced runtime package {PackageId} {Version} resolved and scaffolded for target domain {DomainName}",
                        package.PackageId,
                        package.Version,
                        string.IsNullOrWhiteSpace(package.Domain) ? Const.DefaultDomainName : package.Domain);
                    continue;
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

                    var alc = _domainManager.GetDomain(package.Domain) ?? AssemblyLoadContext.Default;
                    var targetDomainName = AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domainManager.Domains, alc);
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

                    _assemblySolver.StaticFileRegistry.RegisterPackageDirectory(dllPath, targetDomainName);
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
            var alc = _domainManager.GetDomain(moduleItem.Domain ?? Const.DefaultDomainName) ?? AssemblyLoadContext.Default;
            var domainName = AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domainManager.Domains, alc);

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

                _assemblySolver.StaticFileRegistry.RegisterModuleDirectory(moduleItem, domainName);
            }
            else
            {
                var args = new ResolveEventArgs($"{moduleItem.ModuleName}, Version={moduleItem.ModuleVersion}");
                var nugetAssembly = _assemblySolver.ResolveAssemblyFromNuGetPackages(alc, args);

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

    private void LoadAdditionalAssemblies(string modulePath, string? domainName)
    {
        try
        {
            string directoryPath = Path.GetDirectoryName(modulePath) ?? string.Empty;
            if (!Directory.Exists(directoryPath))
                return;

            var alc = _domainManager.GetDomain(domainName ?? Const.DefaultDomainName) ?? AssemblyLoadContext.Default;
            var targetDomainName = AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domainManager.Domains, alc);

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
            _logger.LogError(e, "Error loading additional assemblies for module {ModulePath} in domain {DomainName}",
                modulePath, domainName);
        }
    }

    private void ProcessAssemblies(List<Assembly> assemblies, string domainName, HashSet<string> processedAssemblies)
    {
        var assembliesToProcess = new Queue<Assembly>(assemblies);

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
                            AddCommandToGesCom(_gesCom, method, modInstance, domainName);
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

        foreach (var domain in _domainManager.Domains)
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
            var domainName = AssemblyHelpers.GetDomainNameForAssemblyLoadContext(_domainManager.Domains, alc);

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

    private Task LoadScriptDomainAsync(string domainName)
    {
        _domainManager.CreateDomain(domainName, isCollectible: true);

        LoadForcedPackages(_settings.ForcedPackages
            .Where(x => string.Equals(x.Domain, domainName, StringComparison.OrdinalIgnoreCase))
            .ToList());

        var alc = _domainManager.GetDomain(domainName) ?? AssemblyLoadContext.Default;
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

    /// <inheritdoc/>
    public IntPtr LoadUnmanagedDll(string? domainName, string unmanagedDllName, string libraryPath)
    {
        if (string.IsNullOrWhiteSpace(unmanagedDllName))
        {
            _logger.LogWarning("[ENV-NATIVE-LOAD] Nombre de biblioteca no administrada inválido o vacío.");
            return IntPtr.Zero;
        }

        if (string.IsNullOrWhiteSpace(libraryPath))
        {
            _logger.LogWarning("[ENV-NATIVE-LOAD] Ruta de biblioteca no administrada inválida o vacía para '{UnmanagedDllName}'.", unmanagedDllName);
            return IntPtr.Zero;
        }

        if (!File.Exists(libraryPath))
        {
            _logger.LogWarning("[ENV-NATIVE-LOAD] El archivo de biblioteca nativa no existe en la ruta especificada: '{Path}' (Biblioteca: '{UnmanagedDllName}').", libraryPath, unmanagedDllName);
            return IntPtr.Zero;
        }

        var fullPath = Path.GetFullPath(libraryPath);
        var effectiveDomain = string.IsNullOrWhiteSpace(domainName) ? Const.DefaultDomainName : domainName;

        try
        {
            if (NativeLibrary.TryLoad(fullPath, out var handle))
            {
                _assemblySolver.RegisterUnmanagedDll(effectiveDomain, unmanagedDllName, handle, fullPath);
                _logger.LogInformation(
                    "[ENV-NATIVE-LOAD] Biblioteca nativa '{UnmanagedDllName}' cargada exitosamente desde '{Path}' y registrada en dominio '{Domain}'. Handle: {Handle}",
                    unmanagedDllName,
                    fullPath,
                    effectiveDomain,
                    handle);
                return handle;
            }
            else
            {
                _logger.LogError(
                    "[ENV-NATIVE-LOAD-FAIL] NativeLibrary.TryLoad falló al cargar la biblioteca nativa '{UnmanagedDllName}' desde '{Path}' para dominio '{Domain}'.",
                    unmanagedDllName,
                    fullPath,
                    effectiveDomain);
                return IntPtr.Zero;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[ENV-NATIVE-LOAD-ERROR] Excepción al cargar la biblioteca nativa '{UnmanagedDllName}' desde '{Path}' para dominio '{Domain}'.",
                unmanagedDllName,
                fullPath,
                effectiveDomain);
            return IntPtr.Zero;
        }
    }
}
