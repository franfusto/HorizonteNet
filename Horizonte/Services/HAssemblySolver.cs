using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Xml.Linq;
using Horizonte.Entities;
using Horizonte.Helpers;
using Horizonte.Interfaces;
using Horizonte.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Horizonte.Services;

/// <summary>
/// Servicio encargado de la resolución de ensamblados, descarga de paquetes NuGet,
/// andamiaje de enlaces simbólicos (ISymLinkScafolder) y registro de activos web (StaticFileRegistry).
/// </summary>
public class HAssemblySolver : IHAssemblySolver
{
    private readonly ILogger<HAssemblySolver> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ModulesSettings _settings;
    private readonly ISymLinkScafolder _linkScafolder;
    private readonly IHAssemblyCache _assemblyCache;
    private readonly IPackageSolver _packageSolver;
    private readonly List<string> _searchPaths = new();
    private string _installFolder = "packages";

    private readonly ConcurrentDictionary<string, byte[]> _memoryAssemblyCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IntPtr> _registeredUnmanagedLibraries = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _assemblyToPackageMapLock = new();
    private Dictionary<string, string> _assemblyToPackageMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _processedAssemblyMappingDefinitions = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _processingAssemblyMappingDefinitions = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public StaticFileRegistry StaticFileRegistry { get; } = new();

    /// <inheritdoc/>
    public IHAssemblyCache AssemblyCache => _assemblyCache;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="HAssemblySolver"/> con todas sus dependencias explícitas.
    /// </summary>
    /// <param name="assemblyCache">Caché unificada de resolución de ensamblados.</param>
    /// <param name="packageSolver">Resolutor y descargador de paquetes NuGet.</param>
    /// <param name="serviceProvider">Proveedor de servicios para inyección de dependencias.</param>
    /// <param name="linkScafolder">Gestor de enlaces simbólicos.</param>
    /// <param name="logger">Servicio de registro de eventos.</param>
    public HAssemblySolver(
        IHAssemblyCache assemblyCache,
        IPackageSolver packageSolver,
        IServiceProvider serviceProvider,
        ISymLinkScafolder linkScafolder,
        ILogger<HAssemblySolver> logger)
    {
        _assemblyCache = assemblyCache ?? throw new ArgumentNullException(nameof(assemblyCache));
        _packageSolver = packageSolver ?? throw new ArgumentNullException(nameof(packageSolver));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _linkScafolder = linkScafolder ?? throw new ArgumentNullException(nameof(linkScafolder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = serviceProvider.GetService<ModulesSettings>()
                    ?? serviceProvider.GetService<IhContext>()?.Get<ModulesSettings>()
                    ?? new ModulesSettings();

        MapAssemblyToPackage();
        SetUpAssemblyPaths();
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="HAssemblySolver"/> resolviendo la caché y el paquete desde el proveedor de servicios.
    /// </summary>
    /// <param name="serviceProvider">Proveedor de servicios para inyección de dependencias.</param>
    /// <param name="linkScafolder">Gestor de enlaces simbólicos.</param>
    /// <param name="logger">Servicio de registro de eventos.</param>
    public HAssemblySolver(
        IServiceProvider serviceProvider,
        ISymLinkScafolder linkScafolder,
        ILogger<HAssemblySolver> logger)
        : this(
            serviceProvider.GetRequiredService<IHAssemblyCache>(),
            serviceProvider.GetRequiredService<IPackageSolver>(),
            serviceProvider,
            linkScafolder,
            logger)
    {
    }

    /// <inheritdoc/>
    public Assembly? ResolveAssemblyFromALC(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        return ResolveAssemblyFromNuGetPackages(context, new ResolveEventArgs(assemblyName.FullName));
    }

    /// <inheritdoc/>
    public Assembly? ResolveAssemblyFromNuGetPackages(object? sender, ResolveEventArgs args)
    {
        try
        {
            if (args.Name.StartsWith("ℛ*")) return null; // Ignoramos ensamblados internos

            var domainManager = _serviceProvider.GetService<IHDomainManager>();
            var domainsDict = domainManager?.Domains;

            var requesterDomain = sender is AssemblyLoadContext senderAlc
                ? (domainsDict != null
                    ? AssemblyHelpers.GetDomainNameForAssemblyLoadContext(domainsDict, senderAlc)
                    : senderAlc.Name ?? Const.DefaultDomainName)
                : Const.DefaultDomainName;

            _logger.LogInformation(
                "[RESOLVER] Solicitud de resolución: {AssemblyName} en dominio: {DomainName}",
                args.Name,
                requesterDomain);

            var (name, version) = AssemblyHelpers.ParseAssemblyName(args.Name);
            _logger.LogDebug(
                "[RESOLVER-PARSED] AssemblyName: '{Name}', Version: '{Version}', Raw: '{RawName}'",
                name,
                version ?? "any",
                args.Name);

            if (AssemblyHelpers.TryCreateAssemblyNameForRuntimeResolution(args.Name, out var requestedAssemblyName))
            {
                var sharedAssembly = AssemblyHelpers.TryGetSharedAssemblyFromDefault(requestedAssemblyName);
                if (sharedAssembly != null)
                {
                    _logger.LogInformation(
                        "[RESOLVER-SHARED] Ensamblado base compartido {AssemblyName} entregado desde dominio Default para {DomainName}.",
                        sharedAssembly.FullName,
                        requesterDomain);
                    return sharedAssembly;
                }
            }
            else
            {
                _logger.LogDebug(
                    "[RESOLVER-NOT-RUNTIME-ID] '{AssemblyName}' no es una identidad de runtime estricta. Se resolverá como paquete NuGet '{PackageName}' versión '{Version}'.",
                    args.Name,
                    name,
                    version);
            }

            var packageName = GetPackageName(name);

            if (!string.Equals(name, packageName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "[RESOLVER-MAP] Ensamblado '{AssemblyName}' mapeado a ID de paquete NuGet '{PackageName}'",
                    name,
                    packageName);
            }

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
                var loadedDomain = loadedAlc != null
                    ? (domainsDict != null
                        ? AssemblyHelpers.GetDomainNameForAssemblyLoadContext(domainsDict, loadedAlc)
                        : loadedAlc.Name ?? Const.DefaultDomainName)
                    : Const.DefaultDomainName;

                _logger.LogInformation(
                    "[RESOLVER-ALC-LOADED] Ensamblado {AssemblyName} ya cargado previamente en dominio {DomainName}",
                    alreadyLoaded.FullName,
                    loadedDomain);

                MapAssemblyToPackageFromAssembly(alreadyLoaded);
                return alreadyLoaded;
            }

            if (AssemblyHelpers.ShouldSkipNuGetResolution(name))
            {
                _logger.LogDebug(
                    "[RESOLVER-SKIP] Omitiendo resolución NuGet para ensamblado del runtime .NET: {AssemblyName} (dominio: {DomainName})",
                    name,
                    requesterDomain);
                return null;
            }

            // 1. Consultar IHAssemblyCache (Caché Positiva o Negativa)
            var cachedItem = _assemblyCache.Get(name, version, requesterDomain);
            if (cachedItem != null)
            {
                if (!cachedItem.Found && cachedItem.IsResolved)
                {
                    _logger.LogInformation(
                        "[RESOLVER-CACHE-HIT-NEG] Ensamblado {AssemblyName} en caché negativa para dominio {DomainName}. Retornando null.",
                        args.Name,
                        requesterDomain);
                    return null;
                }

                if (cachedItem.Found && cachedItem.IsRuntimePackage)
                {
                    _logger.LogInformation(
                        "[RESOLVER-CACHE-HIT-RUNTIME] Paquete de runtime nativo {AssemblyName} resuelto previamente en caché para dominio {DomainName}. Omitiendo carga ALC y retornando null.",
                        args.Name,
                        requesterDomain);
                    return null;
                }

                if (cachedItem.Found && cachedItem.ResolvedAssembly != null)
                {
                    _logger.LogInformation(
                        "[RESOLVER-CACHE-HIT-POS] Ensamblado {AssemblyName} recuperado de caché en memoria para dominio {DomainName}.",
                        args.Name,
                        requesterDomain);
                    return cachedItem.ResolvedAssembly;
                }

                if (cachedItem.Found && !string.IsNullOrEmpty(cachedItem.DllPath) && File.Exists(cachedItem.DllPath))
                {
                    _logger.LogInformation(
                        "[RESOLVER-CACHE-HIT-PATH] Cargando ensamblado desde ruta en caché {DllPath} para dominio {DomainName}.",
                        cachedItem.DllPath,
                        requesterDomain);

                    var loadedFromCache = sender is AssemblyLoadContext reqContext
                        ? reqContext.LoadFromAssemblyPath(cachedItem.DllPath)
                        : Assembly.LoadFrom(cachedItem.DllPath);
                    cachedItem.ResolvedAssembly = loadedFromCache;
                    return loadedFromCache;
                }
            }

            _logger.LogDebug(
                "[RESOLVER-CACHE-MISS] No existe entrada en caché para '{AssemblyName}', versión '{Version}' en dominio '{DomainName}'. Delegando en IPackageSolver...",
                name,
                version ?? "any",
                requesterDomain);

            // 2. Resolver paquete mediante IPackageSolver pasando el packageName ya resuelto
            var resolutionResult = Task.Run(() => _packageSolver.ResolvePackageAsync(packageName, version, requesterDomain)).GetAwaiter().GetResult();
            string? resAssemblyPath = null;
            string? packageRootDirectory = resolutionResult.PackageRootDirectory;

            if (resolutionResult.Success)
            {
                if (!string.IsNullOrEmpty(resolutionResult.DllPath))
                {
                    resAssemblyPath = resolutionResult.DllPath;
                }

                _logger.LogInformation(
                    "[RESOLVER-PKG-SUCCESS] PackageSolver resolvió '{PackageId}' v'{Version}' -> Dll: '{DllPath}', IsRuntime: {IsRuntime}, HasNative: {HasNative}",
                    resolutionResult.PackageId,
                    resolutionResult.Version,
                    resAssemblyPath ?? "none",
                    resolutionResult.IsRuntimePackage,
                    resolutionResult.HasNativeAssets);
            }
            else
            {
                _logger.LogDebug(
                    "[RESOLVER-FALLBACK-LOCAL] PackageSolver no resolvió directamente ({Error}). Ejecutando fallback local para '{PackageName}' v'{Version}'...",
                    resolutionResult.ErrorMessage,
                    packageName,
                    version);

                resAssemblyPath = ResolveNugetFromLocalDirectory(packageName, version, null, true)
                               ?? ResolveNugetFromLocalDirectory(packageName, version, null, false);
            }

            if (string.IsNullOrEmpty(resAssemblyPath) && !resolutionResult.IsRuntimePackage)
            {
                _logger.LogError(
                    "[RESOLVER-NOT-FOUND] No fue posible resolver el ensamblado: {AssemblyName} desde el paquete NuGet {PackageName} para el dominio {DomainName}",
                    args.Name,
                    packageName,
                    requesterDomain);

                _assemblyCache.Add(new AssemblyCacheItem
                {
                    AssemblyName = name,
                    Version = version ?? string.Empty,
                    DomainName = requesterDomain,
                    PackageId = packageName,
                    PackageVersion = version,
                    IsResolved = true,
                    Found = false
                });

                return null;
            }

            if (!string.IsNullOrEmpty(resAssemblyPath))
            {
                if (!Path.IsPathRooted(resAssemblyPath))
                {
                    resAssemblyPath = Path.GetFullPath(resAssemblyPath);
                }

                if (string.IsNullOrEmpty(packageRootDirectory))
                {
                    var frameworkDir = Directory.GetParent(resAssemblyPath);
                    var libDir = frameworkDir?.Parent;
                    packageRootDirectory = libDir?.Parent?.FullName;
                }

                _logger.LogInformation(
                    "[RESOLVER-RESOLVED] Ensamblado {AssemblyName} resuelto a la ruta física {AssemblyPath} para dominio {DomainName}",
                    args.Name,
                    resAssemblyPath,
                    requesterDomain);

                _logger.LogDebug("[RESOLVER-POSTPROCESS] Registrando activos estáticos web en StaticFileRegistry para dominio '{DomainName}'...", requesterDomain);
                StaticFileRegistry.RegisterPackageDirectory(resAssemblyPath, requesterDomain);
            }

            // Scaffolding de activos nativos de runtimes (archivo por archivo)
            if (!string.IsNullOrEmpty(packageRootDirectory) &&
                (resolutionResult.HasNativeAssets || resolutionResult.IsRuntimePackage || NugetHelpers.HasNativeRuntimeAssets(packageRootDirectory)))
            {
                try
                {
                    var runtimeSymLinks = GetRuntimeMappingsFromPackage(packageRootDirectory);
                    if (runtimeSymLinks.Any())
                    {
                        _logger.LogInformation(
                            "[RESOLVER-RUNTIME-SYMLINKS] Procesando {Count} enlace(s) simbólico(s) de runtime para '{PackageRoot}'...",
                            runtimeSymLinks.Count,
                            packageRootDirectory);
                        _linkScafolder.BuildScafolder(runtimeSymLinks);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[RESOLVER-RUNTIME-SYMLINKS-ERROR] Error al procesar enlaces simbólicos de runtime para '{PackageRoot}'.", packageRootDirectory);
                }
            }

            // Scaffolding de .targets y .props
            try
            {
                var symLinks = GetContentMappingsFromPackage(resAssemblyPath, packageRootDirectory);
                if (symLinks.Any())
                {
                    _logger.LogInformation(
                        "[RESOLVER-SYMLINKS] Procesando {Count} enlace(s) simbólico(s) de .targets para '{PackageRoot}'...",
                        symLinks.Count,
                        packageRootDirectory ?? resAssemblyPath);
                    _linkScafolder.BuildScafolder(symLinks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RESOLVER-SYMLINKS-ERROR] Error al procesar enlaces simbólicos MSBuild para '{PackageRoot}'.", packageRootDirectory ?? resAssemblyPath);
            }

            // Si es un paquete puramente de runtime nativo (sin DLL administrada)
            if (resolutionResult.IsRuntimePackage || string.IsNullOrEmpty(resAssemblyPath))
            {
                _logger.LogInformation(
                    "[RESOLVER-RUNTIME-PACKAGE-ONLY] Paquete '{PackageId}' v'{Version}' es puramente nativo. Omitiendo carga en ALC y registrando en HAssemblyCache.",
                    resolutionResult.PackageId ?? packageName,
                    resolutionResult.Version ?? version);

                _assemblyCache.Add(new AssemblyCacheItem
                {
                    AssemblyName = name,
                    Version = version ?? string.Empty,
                    DomainName = requesterDomain,
                    PackageId = resolutionResult.PackageId ?? packageName,
                    PackageVersion = resolutionResult.Version ?? version,
                    DllPath = string.Empty,
                    ResolvedAssembly = null,
                    IsResolved = true,
                    Found = true,
                    IsRuntimePackage = true,
                    HasNativeAssets = true
                });

                return null;
            }

            Assembly loadedAssembly;
            if (sender is AssemblyLoadContext context)
            {
                loadedAssembly = context.LoadFromAssemblyPath(resAssemblyPath);
                MapAssemblyToPackageFromAssembly(loadedAssembly);

                var targetDomain = domainsDict != null
                    ? AssemblyHelpers.GetDomainNameForAssemblyLoadContext(domainsDict, context)
                    : context.Name ?? Const.DefaultDomainName;

                _logger.LogInformation(
                    "[RESOLVER-ALC-LOAD] Ensamblado {AssemblyName} cargado exitosamente en ALC del dominio {DomainName}",
                    loadedAssembly.FullName,
                    targetDomain);
            }
            else
            {
                loadedAssembly = Assembly.LoadFrom(resAssemblyPath);
                MapAssemblyToPackageFromAssembly(loadedAssembly);

                _logger.LogInformation(
                    "[RESOLVER-ALC-LOAD] Ensamblado {AssemblyName} cargado exitosamente en contexto Default",
                    loadedAssembly.FullName);
            }

            _assemblyCache.Add(new AssemblyCacheItem
            {
                AssemblyName = name,
                Version = version ?? string.Empty,
                DomainName = requesterDomain,
                PackageId = resolutionResult.PackageId ?? packageName,
                PackageVersion = resolutionResult.Version ?? version,
                DllPath = resAssemblyPath,
                ResolvedAssembly = loadedAssembly,
                IsResolved = true,
                Found = true,
                IsRuntimePackage = false,
                HasNativeAssets = resolutionResult.HasNativeAssets
            });

            _logger.LogDebug(
                "[RESOLVER-CACHE-SAVED] Resultado de resolución para '{AssemblyName}' almacenado en HAssemblyCache (dominio: {DomainName}).",
                name,
                requesterDomain);

            return loadedAssembly;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trying to resolve assembly from repository.");
            return null;
        }
    }

    /// <inheritdoc/>
    public string? ResolveAssemblyDllPath(string packageName, string version, string? targetFramework = null)
    {
        var resolvedPackageName = GetPackageName(packageName);
        var cached = _assemblyCache.Get(resolvedPackageName, version);
        if (cached != null && cached.Found && !string.IsNullOrEmpty(cached.DllPath))
        {
            return cached.DllPath;
        }

        var resolutionResult = Task.Run(() => _packageSolver.ResolvePackageAsync(resolvedPackageName, version, null, targetFramework)).GetAwaiter().GetResult();
        if (resolutionResult.Success)
        {
            var packageRootDirectory = resolutionResult.PackageRootDirectory;
            if (string.IsNullOrEmpty(packageRootDirectory) && !string.IsNullOrEmpty(resolutionResult.DllPath))
            {
                var frameworkDir = Directory.GetParent(resolutionResult.DllPath);
                var libDir = frameworkDir?.Parent;
                packageRootDirectory = libDir?.Parent?.FullName;
            }

            if (!string.IsNullOrEmpty(packageRootDirectory))
            {
                // Scaffolding de activos nativos de runtimes
                if (resolutionResult.HasNativeAssets || resolutionResult.IsRuntimePackage || NugetHelpers.HasNativeRuntimeAssets(packageRootDirectory))
                {
                    try
                    {
                        var runtimeSymLinks = GetRuntimeMappingsFromPackage(packageRootDirectory);
                        if (runtimeSymLinks.Any())
                        {
                            _logger.LogInformation(
                                "[RESOLVER-RUNTIME-SYMLINKS] Procesando {Count} enlace(s) simbólico(s) de runtime para '{PackageRoot}' en carga forzada...",
                                runtimeSymLinks.Count,
                                packageRootDirectory);
                            _linkScafolder.BuildScafolder(runtimeSymLinks);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[RESOLVER-RUNTIME-SYMLINKS-ERROR] Error al procesar enlaces simbólicos de runtime para '{PackageRoot}'.", packageRootDirectory);
                    }
                }

                // Scaffolding de .targets y .props
                try
                {
                    var symLinks = GetContentMappingsFromPackage(resolutionResult.DllPath, packageRootDirectory);
                    if (symLinks.Any())
                    {
                        _logger.LogInformation(
                            "[RESOLVER-SYMLINKS] Procesando {Count} enlace(s) simbólico(s) de .targets para '{PackageRoot}' en carga forzada...",
                            symLinks.Count,
                            packageRootDirectory);
                        _linkScafolder.BuildScafolder(symLinks);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[RESOLVER-SYMLINKS-ERROR] Error al procesar enlaces simbólicos MSBuild para '{PackageRoot}'.", packageRootDirectory);
                }
            }

            if (resolutionResult.IsRuntimePackage || string.IsNullOrEmpty(resolutionResult.DllPath))
            {
                _assemblyCache.Add(new AssemblyCacheItem
                {
                    PackageId = resolvedPackageName,
                    PackageVersion = version,
                    AssemblyName = resolvedPackageName,
                    Version = version ?? string.Empty,
                    DomainName = Const.DefaultDomainName,
                    IsResolved = true,
                    Found = true,
                    IsRuntimePackage = true,
                    HasNativeAssets = resolutionResult.HasNativeAssets
                });
            }

            if (!string.IsNullOrEmpty(resolutionResult.DllPath))
            {
                return Path.IsPathRooted(resolutionResult.DllPath)
                    ? resolutionResult.DllPath
                    : Path.GetFullPath(resolutionResult.DllPath);
            }
        }

        var path = _packageSolver.ResolveLocalDllPath(resolvedPackageName, version, targetFramework);
        if (path == null)
        {
            path = ResolveNugetFromLocalDirectory(resolvedPackageName, version, targetFramework, true)
                ?? ResolveNugetFromLocalDirectory(resolvedPackageName, version, targetFramework, false);
        }

        if (path != null && !Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(path);
        }

        return path;
    }

    /// <inheritdoc/>
    public async Task<string?> DownloadNuGetPackageAsync(string packageName, string version, string? targetFramework = null)
    {
        var resolvedPackageName = GetPackageName(packageName);
        var downloaded = await _packageSolver.DownloadAndInstallPackageAsync(resolvedPackageName, version).ConfigureAwait(false);
        return ResolveAssemblyDllPath(resolvedPackageName, version, targetFramework);
    }

    /// <inheritdoc/>
    public string GetPackageName(string assemblyName)
    {
        lock (_assemblyToPackageMapLock)
        {
            return _assemblyToPackageMap.TryGetValue(assemblyName, out var packageName)
                ? packageName
                : assemblyName;
        }
    }

    /// <inheritdoc/>
    public void CacheAssembly(string key, byte[] assemblyBytes)
    {
        if (string.IsNullOrWhiteSpace(key) || assemblyBytes == null) return;
        _memoryAssemblyCache[key] = assemblyBytes;
    }

    /// <inheritdoc/>
    public byte[]? GetCachedAssembly(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        _memoryAssemblyCache.TryGetValue(key, out var bytes);
        return bytes;
    }

    /// <inheritdoc/>
    public bool IsAssemblyCached(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        return _memoryAssemblyCache.ContainsKey(key);
    }

    private string? ResolveNugetFromLocalDirectory(
        string name,
        string? version,
        string? framework = null,
        bool exactmatch = true)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (string.IsNullOrEmpty(version)) return null;

        string frameworkSolicitado = framework ?? AssemblyHelpers.GetRequestedFramework();

        foreach (var searchPath in _searchPaths)
        {
            var directorioPaquete = Directory.GetDirectories(searchPath, name.ToLower());
            if (!directorioPaquete.Any()) continue;

            if (exactmatch)
            {
                var pathConVersion = Path.Combine(directorioPaquete.First(), version);
                if (Directory.Exists(pathConVersion))
                {
                    var versionList =
                        NugetHelpers.GetNugetPackageVersionInformation(_logger, directorioPaquete.First());
                    var versionSeleccionada =
                        NugetHelpers.SelectedVersion(_settings.FrameworkPriorities, version, frameworkSolicitado,
                            versionList, true, exactFramework: false);

                    if (versionSeleccionada != null) return versionSeleccionada.DllPath;
                }
            }
            else
            {
                var versionList = NugetHelpers.GetNugetPackageVersionInformation(_logger, directorioPaquete.First());
                var versionSeleccionada =
                    NugetHelpers.SelectedVersion(_settings.FrameworkPriorities, version, frameworkSolicitado,
                        versionList, false, exactFramework: false);

                if (versionSeleccionada != null) return versionSeleccionada.DllPath;
            }
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

            var directUrl = NugetHelpers.GetDownloadUrl(nugetServer, packageName, version);
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
                    var normalizedUrl = NugetHelpers.GetDownloadUrl(nugetServer, packageName, normalizedVersion);

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

            var bestVersion = NugetHelpers.SelectBestRemoteVersion(version, availableVersions);
            if (bestVersion != null && bestVersion != version)
            {
                var heuristicUrl = NugetHelpers.GetDownloadUrl(nugetServer, packageName, bestVersion);

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

    private List<SymLinkDef> GetRuntimeMappingsFromPackage(string packageVersionDir, string? rid = null)
    {
        var mappings = new List<SymLinkDef>();

        try
        {
            if (string.IsNullOrWhiteSpace(packageVersionDir) || !Directory.Exists(packageVersionDir))
                return mappings;

            var runtimesDir = Path.Combine(packageVersionDir, "runtimes");
            if (!Directory.Exists(runtimesDir))
                return mappings;

            var effectiveRid = NugetHelpers.GetEffectiveRuntimeIdentifier();
            var compatibleRids = NugetHelpers.GetCompatibleRuntimeIdentifiers(rid ?? effectiveRid);

            foreach (var cRid in compatibleRids)
            {
                var nativeDir = Path.Combine(runtimesDir, cRid, "native");
                if (Directory.Exists(nativeDir))
                {
                    var files = Directory.GetFiles(nativeDir, "*", SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            var relativeSubPath = Path.GetRelativePath(nativeDir, file);
                            var destinationPath = Path.Combine("runtimes", cRid, "native", relativeSubPath);

                            mappings.Add(new SymLinkDef
                            {
                                Source = Path.GetFullPath(file),
                                Destination = destinationPath,
                                IsDirecory = false,
                                HardCoded = false
                            });
                        }

                        return mappings;
                    }
                }

                var ridDir = Path.Combine(runtimesDir, cRid);
                if (Directory.Exists(ridDir))
                {
                    var files = Directory.GetFiles(ridDir, "*", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            var fileName = Path.GetFileName(file);
                            var destinationPath = Path.Combine("runtimes", cRid, "native", fileName);

                            mappings.Add(new SymLinkDef
                            {
                                Source = Path.GetFullPath(file),
                                Destination = destinationPath,
                                IsDirecory = false,
                                HardCoded = false
                            });
                        }

                        return mappings;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo mapeos de runtime para '{PackageVersionDir}'.", packageVersionDir);
        }

        return mappings;
    }

    private List<SymLinkDef> GetContentMappingsFromPackage(string? resAssemblyPath, string? packageVersionDir = null)
    {
        var mappings = new List<SymLinkDef>();

        try
        {
            string? packagePath = null;
            string framework = Const.DefaultFramework;

            if (!string.IsNullOrEmpty(resAssemblyPath))
            {
                var frameworkDir = Directory.GetParent(resAssemblyPath);
                var libDir = frameworkDir?.Parent;
                var pkgVersionDir = libDir?.Parent;
                packagePath = pkgVersionDir?.FullName;
                framework = frameworkDir?.Name ?? Const.DefaultFramework;
            }
            else if (!string.IsNullOrEmpty(packageVersionDir))
            {
                packagePath = packageVersionDir;
            }

            if (string.IsNullOrEmpty(packagePath) || !Directory.Exists(packagePath))
                return mappings;

            string buildDir = Path.Combine(packagePath, "buildTransitive", framework);
            if (!Directory.Exists(buildDir))
            {
                buildDir = Path.Combine(packagePath, "buildTransitive");
                if (!Directory.Exists(buildDir))
                {
                    buildDir = Path.Combine(packagePath, "build", framework);
                    if (!Directory.Exists(buildDir))
                    {
                        buildDir = Path.Combine(packagePath, "build");
                        if (!Directory.Exists(buildDir)) return mappings;
                    }
                }
            }

            var definitionFiles = Directory.GetFiles(buildDir, "*.targets", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(buildDir, "*.props", SearchOption.AllDirectories))
                .Distinct();

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

    private void SetUpAssemblyPaths()
    {
        if (_settings.UseUserNugetFolder)
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

    /// <inheritdoc/>
    public void RegisterAssemblyMapping(string assemblyName, string packageName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName) || string.IsNullOrWhiteSpace(packageName))
            return;

        lock (_assemblyToPackageMapLock)
        {
            _assemblyToPackageMap[assemblyName] = packageName;
        }

        _packageSolver.RegisterAssemblyMapping(assemblyName, packageName);

        _logger.LogInformation(
            "[RESOLVER-MAPPING-REG] Mapeo de ensamblado '{AssemblyName}' a paquete '{PackageName}' registrado dinámicamente.",
            assemblyName,
            packageName);
    }

    /// <inheritdoc/>
    public void ScanAssemblyForMappings(Assembly assembly)
    {
        if (assembly != null)
        {
            MapAssemblyToPackageFromAssembly(assembly);
        }
    }

    private void MapAssemblyToPackage()
    {
        lock (_assemblyToPackageMapLock)
        {
            _assemblyToPackageMap = new Dictionary<string, string>(
                _settings.AssemblyToPackageMap,
                StringComparer.OrdinalIgnoreCase);
        }

        foreach (var assembly in AssemblyLoadContext.Default.Assemblies.ToArray())
        {
            MapAssemblyToPackageFromAssembly(assembly);
        }
    }

    private void MapAssemblyToPackageFromAssembly(Assembly assembly)
    {
        var assemblyKey = assembly.FullName
                          ?? assembly.GetName().Name
                          ?? assembly.Location;

        if (string.IsNullOrWhiteSpace(assemblyKey))
            return;

        lock (_assemblyToPackageMapLock)
        {
            if (_processedAssemblyMappingDefinitions.Contains(assemblyKey) ||
                _processingAssemblyMappingDefinitions.Contains(assemblyKey))
            {
                return;
            }

            _processingAssemblyMappingDefinitions.Add(assemblyKey);
        }

        var mappingScanCompleted = false;

        try
        {
            IEnumerable<Type> types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.OfType<Type>();

                _logger.LogDebug(
                    ex,
                    "El ensamblado {AssemblyName} no pudo cargar todos sus tipos al buscar mapeos de paquetes. Se procesarán los tipos disponibles.",
                    assembly.FullName);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(
                    ex,
                    "No se pudieron inspeccionar los tipos del ensamblado {AssemblyName} para buscar mapeos de paquetes.",
                    assembly.FullName);

                return;
            }

            var typeList = types.ToArray();

            if (typeList.Length == 0)
                return;

            foreach (var type in typeList)
            {
                var fields = type.GetFields(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static);

                foreach (var field in fields)
                {
                    if (!HasAssemblyMappingDefinition(field))
                        continue;

                    object? value;

                    try
                    {
                        value = field.GetValue(null);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "No se pudo leer el campo de mapeo {FieldName} del tipo {TypeName} en el ensamblado {AssemblyName}.",
                            field.Name,
                            type.FullName,
                            assembly.FullName);

                        continue;
                    }

                    if (value is not IEnumerable<KeyValuePair<string, string>> mappings)
                    {
                        _logger.LogWarning(
                            "El campo {FieldName} del tipo {TypeName} está marcado con {AttributeName}, pero no contiene un diccionario compatible con string/string.",
                            field.Name,
                            type.FullName,
                            nameof(AssemblyMappingDefinition));

                        continue;
                    }

                    ProcessMappings(mappings, type.FullName ?? type.Name, field.Name, assembly.FullName ?? assemblyKey);
                }

                var properties = type.GetProperties(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static);

                foreach (var prop in properties)
                {
                    if (!HasAssemblyMappingDefinition(prop) || !prop.CanRead)
                        continue;

                    object? value;

                    try
                    {
                        value = prop.GetValue(null);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "No se pudo leer la propiedad de mapeo {PropertyName} del tipo {TypeName} en el ensamblado {AssemblyName}.",
                            prop.Name,
                            type.FullName,
                            assembly.FullName);

                        continue;
                    }

                    if (value is not IEnumerable<KeyValuePair<string, string>> mappings)
                    {
                        _logger.LogWarning(
                            "La propiedad {PropertyName} del tipo {TypeName} está marcada con {AttributeName}, pero no contiene un diccionario compatible con string/string.",
                            prop.Name,
                            type.FullName,
                            nameof(AssemblyMappingDefinition));

                        continue;
                    }

                    ProcessMappings(mappings, type.FullName ?? type.Name, prop.Name, assembly.FullName ?? assemblyKey);
                }
            }

            mappingScanCompleted = true;
        }
        finally
        {
            lock (_assemblyToPackageMapLock)
            {
                _processingAssemblyMappingDefinitions.Remove(assemblyKey);

                if (mappingScanCompleted)
                {
                    _processedAssemblyMappingDefinitions.Add(assemblyKey);
                }
            }
        }
    }

    private void ProcessMappings(IEnumerable<KeyValuePair<string, string>> mappings, string typeName, string memberName, string mappingAssemblyName)
    {
        foreach (var mapping in mappings)
        {
            var assemblyName = mapping.Key;
            var packageName = mapping.Value;

            if (string.IsNullOrWhiteSpace(assemblyName) || string.IsNullOrWhiteSpace(packageName))
            {
                _logger.LogWarning(
                    "Se ha ignorado un mapeo inválido en {TypeName}.{MemberName}: AssemblyName='{AssemblyName}', PackageName='{PackageName}'.",
                    typeName,
                    memberName,
                    assemblyName,
                    packageName);

                continue;
            }

            lock (_assemblyToPackageMapLock)
            {
                if (_assemblyToPackageMap.TryGetValue(assemblyName, out var currentPackageName))
                {
                    _logger.LogInformation(
                        "El mapeo para el ensamblado {AssemblyName} ya existe como {CurrentPackageName}. Se mantiene el mapeo actual y se ignora {PackageName} definido en {MappingAssemblyName}.",
                        assemblyName,
                        currentPackageName,
                        packageName,
                        mappingAssemblyName);

                    continue;
                }

                _assemblyToPackageMap[assemblyName] = packageName;
            }

            _packageSolver.RegisterAssemblyMapping(assemblyName, packageName);

            _logger.LogInformation(
                "Añadido mapeo de ensamblado {AssemblyName} a paquete {PackageName} desde {MappingAssemblyName}.",
                assemblyName,
                packageName,
                mappingAssemblyName);
        }
    }

    private bool HasAssemblyMappingDefinition(MemberInfo member)
    {
        try
        {
            return member
                .GetCustomAttributes(inherit: false)
                .Any(attribute => string.Equals(
                    attribute.GetType().FullName,
                    typeof(AssemblyMappingDefinition).FullName,
                    StringComparison.Ordinal));
        }
        catch (Exception ex)
        {
            _logger.LogDebug(
                ex,
                "No se pudieron leer los atributos del miembro {MemberName}.",
                member.Name);

            return false;
        }
    }

    /// <inheritdoc/>
    public void RegisterUnmanagedDll(string domainName, string unmanagedDllName, IntPtr handle, string libraryPath)
    {
        if (string.IsNullOrWhiteSpace(unmanagedDllName) || handle == IntPtr.Zero)
            return;

        var effectiveDomain = string.IsNullOrWhiteSpace(domainName) ? Const.DefaultDomainName : domainName;
        var candidateNames = GetNativeCandidateNames(unmanagedDllName);

        foreach (var candidate in candidateNames)
        {
            var key = $"{effectiveDomain}::{candidate}";
            _registeredUnmanagedLibraries[key] = handle;
        }

        _logger.LogInformation(
            "[RESOLVER-NATIVE-REGISTER] Biblioteca nativa registrada: '{UnmanagedDllName}' -> Handle: {Handle}, Ruta: '{Path}', Dominio: '{Domain}'",
            unmanagedDllName,
            handle,
            libraryPath,
            effectiveDomain);
    }

    /// <inheritdoc/>
    public IntPtr ResolveUnmanagedDll(AssemblyLoadContext? context, string unmanagedDllName)
    {
        if (string.IsNullOrWhiteSpace(unmanagedDllName))
            return IntPtr.Zero;

        var contextName = context?.Name ?? Const.DefaultDomainName;
        _logger.LogInformation(
            "[RESOLVER-NATIVE] Solicitud de resolución de biblioteca no administrada: '{UnmanagedDllName}' en contexto '{ContextName}'",
            unmanagedDllName,
            contextName);

        var candidateNames = GetNativeCandidateNames(unmanagedDllName);

        // 0. Consultar registros manuales/explícitos previos
        foreach (var candidate in candidateNames)
        {
            if (_registeredUnmanagedLibraries.TryGetValue($"{contextName}::{candidate}", out var registeredHandle) && registeredHandle != IntPtr.Zero)
            {
                _logger.LogInformation(
                    "[RESOLVER-NATIVE-LOAD] Biblioteca nativa '{UnmanagedDllName}' resuelta desde registro explícito en contexto '{ContextName}'",
                    unmanagedDllName,
                    contextName);
                return registeredHandle;
            }
        }

        // Si no se encontró en el dominio específico, consultar el dominio por defecto
        if (!string.Equals(contextName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
        {
            foreach (var candidate in candidateNames)
            {
                if (_registeredUnmanagedLibraries.TryGetValue($"{Const.DefaultDomainName}::{candidate}", out var defaultHandle) && defaultHandle != IntPtr.Zero)
                {
                    _logger.LogInformation(
                        "[RESOLVER-NATIVE-LOAD] Biblioteca nativa '{UnmanagedDllName}' resuelta desde registro explícito global (Default) para contexto '{ContextName}'",
                        unmanagedDllName,
                        contextName);
                    return defaultHandle;
                }
            }
        }

        // Si ya es una ruta absoluta o relativa directa existente
        if (File.Exists(unmanagedDllName))
        {
            var fullPath = Path.GetFullPath(unmanagedDllName);
            if (NativeLibrary.TryLoad(fullPath, out var directHandle))
            {
                _logger.LogInformation(
                    "[RESOLVER-NATIVE-LOAD] Biblioteca nativa cargada directamente desde ruta '{Path}' para contexto '{ContextName}'",
                    fullPath,
                    contextName);
                return directHandle;
            }
        }

        var effectiveRid = NugetHelpers.GetEffectiveRuntimeIdentifier();
        var compatibleRids = NugetHelpers.GetCompatibleRuntimeIdentifiers(effectiveRid);

        var searchRoots = new List<string>
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        };

        foreach (var cRid in compatibleRids)
        {
            foreach (var candidateName in candidateNames)
            {
                foreach (var root in searchRoots)
                {
                    // 1. Probar en root/runtimes/{cRid}/native/{candidateName}
                    var candidatePath = Path.Combine(root, "runtimes", cRid, "native", candidateName);
                    if (File.Exists(candidatePath))
                    {
                        var fullCandidatePath = Path.GetFullPath(candidatePath);
                        if (NativeLibrary.TryLoad(fullCandidatePath, out var handle))
                        {
                            _logger.LogInformation(
                                "[RESOLVER-NATIVE-LOAD] Biblioteca nativa '{UnmanagedDllName}' cargada exitosamente desde '{CandidatePath}' para contexto '{ContextName}'",
                                unmanagedDllName,
                                fullCandidatePath,
                                contextName);
                            return handle;
                        }
                    }

                    // 2. Probar en root/runtimes/{cRid}/{candidateName}
                    candidatePath = Path.Combine(root, "runtimes", cRid, candidateName);
                    if (File.Exists(candidatePath))
                    {
                        var fullCandidatePath = Path.GetFullPath(candidatePath);
                        if (NativeLibrary.TryLoad(fullCandidatePath, out var handle))
                        {
                            _logger.LogInformation(
                                "[RESOLVER-NATIVE-LOAD] Biblioteca nativa '{UnmanagedDllName}' cargada exitosamente desde '{CandidatePath}' para contexto '{ContextName}'",
                                unmanagedDllName,
                                fullCandidatePath,
                                contextName);
                            return handle;
                        }
                    }
                }

                // 3. Probar en directorios de paquetes registrados o configurados
                foreach (var folder in _searchPaths)
                {
                    if (!Directory.Exists(folder))
                        continue;

                    var candidatePath = Path.Combine(folder, "runtimes", cRid, "native", candidateName);
                    if (File.Exists(candidatePath))
                    {
                        var fullCandidatePath = Path.GetFullPath(candidatePath);
                        if (NativeLibrary.TryLoad(fullCandidatePath, out var handle))
                        {
                            _logger.LogInformation(
                                "[RESOLVER-NATIVE-LOAD] Biblioteca nativa '{UnmanagedDllName}' cargada exitosamente desde '{CandidatePath}' para contexto '{ContextName}'",
                                unmanagedDllName,
                                fullCandidatePath,
                                contextName);
                            return handle;
                        }
                    }
                }
            }
        }

        _logger.LogDebug(
            "[RESOLVER-NATIVE-NOTFOUND] No se encontró biblioteca nativa para '{UnmanagedDllName}' en contexto '{ContextName}'. Dejando fallback al CLR.",
            unmanagedDllName,
            contextName);

        return IntPtr.Zero;
    }

    private static List<string> GetNativeCandidateNames(string unmanagedDllName)
    {
        var candidates = new List<string>();
        var cleanName = Path.GetFileName(unmanagedDllName);

        if (!string.IsNullOrWhiteSpace(cleanName))
        {
            candidates.Add(cleanName);
        }

        if (OperatingSystem.IsWindows())
        {
            if (!cleanName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add(cleanName + ".dll");
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            if (!cleanName.EndsWith(".dylib", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add(cleanName + ".dylib");
                candidates.Add("lib" + cleanName + ".dylib");
            }
            if (!cleanName.StartsWith("lib", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add("lib" + cleanName);
            }
        }
        else // Linux y otros Unix
        {
            if (!cleanName.EndsWith(".so", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add(cleanName + ".so");
                candidates.Add("lib" + cleanName + ".so");
            }
            if (!cleanName.StartsWith("lib", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add("lib" + cleanName);
            }
        }

        return candidates.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}
