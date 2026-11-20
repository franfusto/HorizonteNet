using System.IO.Compression;
using Horizonte.Entities;
using Horizonte.Helpers;
using Horizonte.Interfaces;
using Horizonte.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Horizonte.Services;

/// <summary>
/// Servicio encargado de la búsqueda, evaluación semántica y descarga de paquetes NuGet,
/// integrando la caché unificada (VersionResources) y la aplicación de políticas de seguridad (BlockedPackagesPolicy).
/// </summary>
public class PackageSolver : IPackageSolver
{
    private readonly IHAssemblyCache _assemblyCache;
    private readonly ModulesSettings _settings;
    private readonly ILogger<PackageSolver> _logger;
    private readonly List<string> _searchPaths = new();
    private string _installFolder = "packages";
    private static readonly TimeSpan DefaultRemoteQueryTimeout = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan DefaultRemoteDownloadTimeout = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PackageSolver"/> utilizando el proveedor de servicios para obtener la configuración.
    /// </summary>
    /// <param name="assemblyCache">Caché unificada de resolución de ensamblados y versiones.</param>
    /// <param name="serviceProvider">Proveedor de servicios para inyección de dependencias.</param>
    /// <param name="logger">Servicio de registro de eventos para <see cref="PackageSolver"/>.</param>
    public PackageSolver(
        IHAssemblyCache assemblyCache,
        IServiceProvider serviceProvider,
        ILogger<PackageSolver> logger)
    {
        _assemblyCache = assemblyCache ?? throw new ArgumentNullException(nameof(assemblyCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = serviceProvider.GetService<IhContext>()?.Get<ModulesSettings>() ?? new ModulesSettings();

        SetUpSearchPaths();
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PackageSolver"/> con la configuración explícita proporcionada.
    /// </summary>
    /// <param name="assemblyCache">Caché unificada de resolución de ensamblados y versiones.</param>
    /// <param name="settings">Configuración modular y de paquetes NuGet.</param>
    /// <param name="logger">Servicio de registro de eventos para <see cref="PackageSolver"/>.</param>
    public PackageSolver(
        IHAssemblyCache assemblyCache,
        ModulesSettings settings,
        ILogger<PackageSolver> logger)
    {
        _assemblyCache = assemblyCache ?? throw new ArgumentNullException(nameof(assemblyCache));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        SetUpSearchPaths();
    }

    private void SetUpSearchPaths()
    {
        _searchPaths.Clear();

        if (_settings.NugetFolders != null && _settings.NugetFolders.Any())
        {
            var installFolderItem = _settings.NugetFolders.FirstOrDefault(f => f.InstallFolder && f.Active)
                                   ?? _settings.NugetFolders.FirstOrDefault(f => f.Active);

            if (installFolderItem != null && !string.IsNullOrWhiteSpace(installFolderItem.Folder))
            {
                _installFolder = installFolderItem.Folder;
            }

            foreach (var folderItem in _settings.NugetFolders.Where(f => f.Active).OrderBy(f => f.Order))
            {
                if (!string.IsNullOrWhiteSpace(folderItem.Folder))
                {
                    _searchPaths.Add(folderItem.Folder);
                }
            }
        }
        else
        {
            _searchPaths.Add(_installFolder);
        }

        if (_settings.UseUserNugetFolder)
        {
            var userNugetPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget",
                "packages");

            if (Directory.Exists(userNugetPath) && !_searchPaths.Contains(userNugetPath, StringComparer.OrdinalIgnoreCase))
            {
                _searchPaths.Add(userNugetPath);
            }
        }
    }

    /// <summary>
    /// Escanea exclusivamente los repositorios y carpetas locales configuradas para descubrir las versiones disponibles de un paquete.
    /// </summary>
    /// <param name="packageId">Identificador del paquete NuGet.</param>
    /// <param name="requestedVersion">Versión específica solicitada, si aplica.</param>
    /// <returns>Estructura <see cref="VersionResources"/> con las versiones locales encontradas.</returns>
    public VersionResources FindLocalVersionResources(
        string packageId,
        NuGetVersion? requestedVersion = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        var versionResources = new VersionResources
        {
            PackageId = packageId,
            RetrievedAtUtc = DateTime.UtcNow
        };

        foreach (var searchPath in _searchPaths)
        {
            if (!Directory.Exists(searchPath))
            {
                _logger.LogTrace("[PKGSOLVER-LOCAL-SCAN-SKIP] Directorio local '{SearchPath}' no existe.", searchPath);
                continue;
            }

            var localVersions = FindLocalVersions(searchPath, packageId, requestedVersion);
            if (localVersions.Count > 0)
            {
                _logger.LogInformation("[PKGSOLVER-LOCAL-VERSIONS] Origen local '{SearchPath}': {Count} versión(es) encontrada(s) para '{PackageId}'.",
                    searchPath,
                    localVersions.Count,
                    packageId);

                versionResources.Sources.Add(new PackageSourceResource(
                    SourceName: searchPath,
                    IsLocal: true,
                    Versions: localVersions));
            }
            else
            {
                _logger.LogDebug("[PKGSOLVER-LOCAL-VERSIONS-NONE] Origen local '{SearchPath}': no se encontraron versiones para '{PackageId}'.",
                    searchPath,
                    packageId);
            }
        }

        return versionResources;
    }

    /// <summary>
    /// Consulta los servidores NuGet remotos configurados y fusiona las versiones descubiertas en la estructura <see cref="VersionResources"/> existente.
    /// </summary>
    /// <param name="packageId">Identificador del paquete NuGet.</param>
    /// <param name="versionResources">Instancia de <see cref="VersionResources"/> donde fusionar los orígenes remotos.</param>
    /// <param name="requestedVersion">Versión específica solicitada, si aplica.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>La instancia actualizada de <see cref="VersionResources"/> con los orígenes remotos consolidados.</returns>
    public async Task<VersionResources> FetchAndMergeRemoteVersionResourcesAsync(
        string packageId,
        VersionResources versionResources,
        NuGetVersion? requestedVersion = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        versionResources ??= new VersionResources { PackageId = packageId, RetrievedAtUtc = DateTime.UtcNow };

        if (_settings.NugetServers != null)
        {
            var activeServers = _settings.NugetServers.Where(s => s.Active).OrderBy(s => s.Order).ToList();
            if (activeServers.Count > 0)
            {
                var queryTasks = activeServers.Select(async server =>
                {
                    try
                    {
                        _logger.LogDebug("[PKGSOLVER-REMOTE-QUERY] Consultando servidor remoto '{ServerName}' ({ServerUrl}) para '{PackageId}'...",
                            server.Name ?? "NuGet",
                            server.Server,
                            packageId);

                        var remoteVersions = await FindRemoteVersionsAsync(server, packageId, requestedVersion, cancellationToken).ConfigureAwait(false);
                        return (server, remoteVersions);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[PKGSOLVER-REMOTE-ERROR] Error al consultar versiones remotas para '{PackageId}' en servidor '{Server}'.",
                            packageId, server.Server);
                        return (server, (IReadOnlyList<PackageVersionInfo>)Array.Empty<PackageVersionInfo>());
                    }
                }).ToList();

                var results = await Task.WhenAll(queryTasks).ConfigureAwait(false);
                foreach (var (server, remoteVersions) in results)
                {
                    var sourceName = server.Name ?? server.Server;
                    versionResources.Sources.RemoveAll(s => !s.IsLocal && string.Equals(s.SourceName, sourceName, StringComparison.OrdinalIgnoreCase));

                    if (remoteVersions.Count > 0)
                    {
                        _logger.LogInformation("[PKGSOLVER-REMOTE-VERSIONS] Servidor remoto '{ServerName}': {Count} versión(es) encontrada(s) para '{PackageId}'.",
                            sourceName,
                            remoteVersions.Count,
                            packageId);

                        versionResources.Sources.Add(new PackageSourceResource(
                            SourceName: sourceName,
                            IsLocal: false,
                            Versions: remoteVersions));
                    }
                    else
                    {
                        _logger.LogDebug("[PKGSOLVER-REMOTE-VERSIONS-NONE] Servidor remoto '{ServerName}': no se encontraron versiones para '{PackageId}'.",
                            sourceName,
                            packageId);
                    }
                }
            }
        }

        versionResources.RetrievedAtUtc = DateTime.UtcNow;
        _assemblyCache.SetVersionResources(packageId, versionResources);
        _logger.LogDebug("[PKGSOLVER-VR-SAVED] VersionResources consolidados y actualizados en caché para '{PackageId}' ({Count} orígenes).",
            packageId,
            versionResources.Sources.Count);

        return versionResources;
    }

    /// <summary>
    /// Consulta y consolida los recursos de versiones disponibles para un paquete tanto en repositorios locales como en servidores NuGet remotos.
    /// </summary>
    /// <param name="packageId">Identificador del paquete NuGet.</param>
    /// <param name="requestedVersion">Versión específica solicitada, si aplica.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una tarea que devuelve la estructura <see cref="VersionResources"/> con las versiones descubiertas.</returns>
    public async Task<VersionResources> FindVersionResourcesAsync(
        string packageId,
        NuGetVersion? requestedVersion = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        var cachedResources = _assemblyCache.GetVersionResources(packageId);
        if (cachedResources != null && cachedResources.Sources.Any(s => !s.IsLocal))
        {
            _logger.LogDebug("[PKGSOLVER-VR-CACHE-HIT] VersionResources completos para '{PackageId}' obtenidos de caché unificada ({Count} orígenes).",
                packageId,
                cachedResources.Sources.Count);
            return cachedResources;
        }

        _logger.LogDebug("[PKGSOLVER-VR-FETCH] Consultando orígenes de versiones para '{PackageId}' (versión solicitada: {RequestedVersion})...",
            packageId,
            requestedVersion?.ToNormalizedString() ?? "any");

        var versionResources = cachedResources ?? FindLocalVersionResources(packageId, requestedVersion);
        versionResources = await FetchAndMergeRemoteVersionResourcesAsync(packageId, versionResources, requestedVersion, cancellationToken).ConfigureAwait(false);

        return versionResources;
    }

    /// <inheritdoc/>
    public async Task<PackageResolutionResult> ResolvePackageAsync(
        string packageOrAssemblyName,
        string? requestedVersion = null,
        string? domainName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageOrAssemblyName);

        _logger.LogDebug("[PKGSOLVER-RESOLVE] Iniciando resolución de paquete para '{PackageOrAssembly}' (versión: {RequestedVersion}, dominio: {Domain})...",
            packageOrAssemblyName,
            requestedVersion ?? "any",
            domainName ?? "Default");

        var packageId = GetPackageName(packageOrAssemblyName);
        NuGetVersion.TryParse(requestedVersion, out var requestedNuGetVersion);

        // 1. Obtener VersionResources desde caché o escanear orígenes locales prioritariamente
        var versionResources = _assemblyCache.GetVersionResources(packageId);
        if (versionResources == null)
        {
            _logger.LogDebug("[PKGSOLVER-LOCAL-FIRST-SCAN] Escaneando orígenes locales inicialmente para '{PackageId}'...", packageId);
            versionResources = FindLocalVersionResources(packageId, requestedNuGetVersion);
            _assemblyCache.SetVersionResources(packageId, versionResources);
        }

        // 2. Intentar resolución local inicial si se descubrieron versiones locales
        string? targetVersionStr = null;
        bool requiresRemoteFallback = false;

        if (versionResources.Sources.Count > 0)
        {
            targetVersionStr = SelectBestMatchingVersion(packageId, requestedVersion, versionResources);
            if (!string.Equals(targetVersionStr, requestedVersion, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("[PKGSOLVER-VERSION-SELECTED-LOCAL] Versión solicitada '{RequestedVersion}' ajustada a versión candidata local '{SelectedVersion}' para '{PackageId}'.",
                    requestedVersion ?? "any",
                    targetVersionStr ?? "none",
                    packageId);
            }

            // Evaluar BlockedPackages sobre candidato local
            if (IsVersionBlocked(packageId, targetVersionStr, out var blockedItem))
            {
                _logger.LogWarning("[PKGSOLVER-BLOCKED] Paquete '{PackageId}' versión '{Version}' BLOQUEADO por directiva: {Reason} (Política activa: {Policy})",
                    packageId,
                    targetVersionStr,
                    blockedItem?.Reason,
                    _settings.BlockedPackagesPolicy);

                switch (_settings.BlockedPackagesPolicy)
                {
                    case BlockedPackagesPolicy.Null:
                        _logger.LogInformation("[PKGSOLVER-POLICY-NULL] Aplicando política Null: devolviendo fallo y registrando caché negativa para '{PackageId}'.", packageId);
                        return new PackageResolutionResult
                        {
                            Success = false,
                            Blocked = true,
                            PackageId = packageId,
                            Version = targetVersionStr,
                            ErrorMessage = $"Paquete bloqueado por política de seguridad (Null): {blockedItem?.Reason}"
                        };

                    case BlockedPackagesPolicy.Error:
                        var errorMsg = $"Resolución rechazada. Paquete '{packageId}' versión '{targetVersionStr}' bloqueado: {blockedItem?.Reason}";
                        _logger.LogError("[PKGSOLVER-POLICY-ERROR] {ErrorMsg}", errorMsg);
                        throw new InvalidOperationException(errorMsg);

                    case BlockedPackagesPolicy.Mayor:
                        _logger.LogDebug("[PKGSOLVER-POLICY-MAYOR-LOCAL] Buscando menor versión superior no bloqueada en orígenes locales para '{PackageId}'...", packageId);
                        var higherLocal = SelectLowestHigherUnblockedVersion(packageId, requestedNuGetVersion, versionResources);
                        if (higherLocal != null)
                        {
                            _logger.LogInformation("[PKGSOLVER-POLICY-MAYOR-LOCAL-SELECTED] Política 'Mayor' seleccionó versión superior local '{HigherVersion}' para '{PackageId}'.",
                                higherLocal.ToNormalizedString(), packageId);
                            targetVersionStr = higherLocal.ToNormalizedString();
                        }
                        else
                        {
                            _logger.LogInformation("[PKGSOLVER-POLICY-MAYOR-LOCAL-NONE] No se encontró versión superior en local. Se requerirá sondeo remoto para '{PackageId}'.", packageId);
                            requiresRemoteFallback = true;
                        }
                        break;
                }
            }

            // Si tenemos versión candidata no bloqueada y no se requiere fallback remoto explícito, intentar resolver paquete localmente
            if (!requiresRemoteFallback && !string.IsNullOrEmpty(targetVersionStr))
            {
                _logger.LogDebug("[PKGSOLVER-LOCAL-CHECK] Comprobando existencia local para '{PackageId}' v'{Version}'...",
                    packageId, targetVersionStr);

                var localInfo = ResolveLocalPackageInfo(packageId, targetVersionStr);
                if (localInfo != null)
                {
                    bool hasManagedDll = !string.IsNullOrEmpty(localInfo.DllPath) && File.Exists(localInfo.DllPath);
                    bool hasNative = localInfo.HasNativeAssets ||
                                     (!string.IsNullOrEmpty(localInfo.PackageRootDirectory) && NugetHelpers.HasNativeRuntimeAssets(localInfo.PackageRootDirectory));

                    if (hasManagedDll || hasNative)
                    {
                        bool isPureNative = !hasManagedDll && hasNative;
                        _logger.LogInformation("[PKGSOLVER-LOCAL-HIT-SUCCESS] Paquete '{PackageId}' v'{Version}' resuelto localmente (IsRuntime: {IsRuntime}, HasNative: {HasNative}) -> DLL: '{DllPath}', Root: '{PackageRoot}'",
                            packageId, targetVersionStr, isPureNative, hasNative, localInfo.DllPath, localInfo.PackageRootDirectory);

                        return new PackageResolutionResult
                        {
                            Success = true,
                            PackageId = packageId,
                            Version = targetVersionStr,
                            DllPath = hasManagedDll ? localInfo.DllPath : string.Empty,
                            IsRuntimePackage = isPureNative,
                            HasNativeAssets = hasNative,
                            PackageRootDirectory = localInfo.PackageRootDirectory
                        };
                    }
                }
                else
                {
                    _logger.LogDebug("[PKGSOLVER-LOCAL-DLL-MISS] Recursos físicos no encontrados localmente para '{PackageId}' v'{Version}'. Procediendo a fallback remoto.",
                        packageId, targetVersionStr);
                }
            }
        }

        // 3. Fallback Remoto: Consultar servidores NuGet remotos si el paquete no se resolvió localmente
        var hasRemoteSources = versionResources.Sources.Any(s => !s.IsLocal);
        if (!hasRemoteSources)
        {
            _logger.LogInformation("[PKGSOLVER-REMOTE-FALLBACK] Iniciando consulta a servidores NuGet remotos para '{PackageId}'...", packageId);
            versionResources = await FetchAndMergeRemoteVersionResourcesAsync(packageId, versionResources, requestedNuGetVersion).ConfigureAwait(false);
        }

        // 4. Reevaluar mejor versión descubierta con fuentes remotas
        targetVersionStr = SelectBestMatchingVersion(packageId, requestedVersion, versionResources);
        if (!string.Equals(targetVersionStr, requestedVersion, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("[PKGSOLVER-VERSION-SELECTED-REMOTE] Versión solicitada '{RequestedVersion}' ajustada a versión descubierta '{SelectedVersion}' para '{PackageId}'.",
                requestedVersion ?? "any",
                targetVersionStr ?? "none",
                packageId);
        }

        // 5. Reevaluar BlockedPackages
        if (IsVersionBlocked(packageId, targetVersionStr, out var remoteBlockedItem))
        {
            _logger.LogWarning("[PKGSOLVER-BLOCKED] Paquete '{PackageId}' versión '{Version}' BLOQUEADO por directiva: {Reason} (Política activa: {Policy})",
                packageId,
                targetVersionStr,
                remoteBlockedItem?.Reason,
                _settings.BlockedPackagesPolicy);

            switch (_settings.BlockedPackagesPolicy)
            {
                case BlockedPackagesPolicy.Null:
                    _logger.LogInformation("[PKGSOLVER-POLICY-NULL] Aplicando política Null: devolviendo fallo y registrando caché negativa para '{PackageId}'.", packageId);
                    return new PackageResolutionResult
                    {
                        Success = false,
                        Blocked = true,
                        PackageId = packageId,
                        Version = targetVersionStr,
                        ErrorMessage = $"Paquete bloqueado por política de seguridad (Null): {remoteBlockedItem?.Reason}"
                    };

                case BlockedPackagesPolicy.Error:
                    var errorMsg = $"Resolución rechazada. Paquete '{packageId}' versión '{targetVersionStr}' bloqueado: {remoteBlockedItem?.Reason}";
                    _logger.LogError("[PKGSOLVER-POLICY-ERROR] {ErrorMsg}", errorMsg);
                    throw new InvalidOperationException(errorMsg);

                case BlockedPackagesPolicy.Mayor:
                    _logger.LogDebug("[PKGSOLVER-POLICY-MAYOR-REMOTE] Buscando menor versión superior no bloqueada en VersionResources para '{PackageId}'...", packageId);
                    var higherVersion = SelectLowestHigherUnblockedVersion(packageId, requestedNuGetVersion, versionResources);
                    if (higherVersion == null)
                    {
                        var noHigherMsg = $"No se encontró ninguna versión superior no bloqueada para el paquete '{packageId}' (solicitada: '{targetVersionStr}').";
                        _logger.LogError("[PKGSOLVER-POLICY-MAYOR-NONE] {NoHigherMsg}", noHigherMsg);
                        return new PackageResolutionResult
                        {
                            Success = false,
                            Blocked = true,
                            PackageId = packageId,
                            Version = targetVersionStr,
                            ErrorMessage = noHigherMsg
                        };
                    }

                    _logger.LogInformation("[PKGSOLVER-POLICY-MAYOR-SELECTED] Política 'Mayor' seleccionó versión superior '{HigherVersion}' para '{PackageId}'.",
                        higherVersion.ToNormalizedString(), packageId);
                    targetVersionStr = higherVersion.ToNormalizedString();
                    break;
            }
        }

        // 6. Comprobar si tras la resolución el paquete ya existía o descargarlo
        NugetPackageVersionInformation? postLocalInfo = null;
        if (!string.IsNullOrEmpty(targetVersionStr))
        {
            postLocalInfo = ResolveLocalPackageInfo(packageId, targetVersionStr);
        }

        if (postLocalInfo == null && !string.IsNullOrEmpty(targetVersionStr))
        {
            _logger.LogInformation("[PKGSOLVER-REMOTE-DOWNLOAD-TRIGGER] Paquete '{PackageId}' v'{Version}' no localizado localmente. Iniciando descarga remota...",
                packageId, targetVersionStr);

            var downloaded = await DownloadAndInstallPackageAsync(packageId, targetVersionStr).ConfigureAwait(false);
            if (downloaded != null)
            {
                _logger.LogDebug("[PKGSOLVER-POST-DOWNLOAD-RESOLVE] Localizando recursos tras descarga e instalación en '{DownloadedDir}'...", downloaded);
                postLocalInfo = ResolveLocalPackageInfo(packageId, targetVersionStr);
            }
        }

        if (postLocalInfo != null)
        {
            bool hasManagedDll = !string.IsNullOrEmpty(postLocalInfo.DllPath) && File.Exists(postLocalInfo.DllPath);
            bool hasNative = postLocalInfo.HasNativeAssets ||
                             (!string.IsNullOrEmpty(postLocalInfo.PackageRootDirectory) && NugetHelpers.HasNativeRuntimeAssets(postLocalInfo.PackageRootDirectory));

            if (hasManagedDll || hasNative)
            {
                bool isPureNative = !hasManagedDll && hasNative;
                _logger.LogInformation("[PKGSOLVER-RESOLVED-SUCCESS] Paquete '{PackageId}' v'{Version}' resuelto exitosamente (IsRuntime: {IsRuntime}, HasNative: {HasNative}) -> DLL: '{DllPath}', Root: '{PackageRoot}'",
                    packageId, targetVersionStr, isPureNative, hasNative, postLocalInfo.DllPath, postLocalInfo.PackageRootDirectory);

                return new PackageResolutionResult
                {
                    Success = true,
                    PackageId = packageId,
                    Version = targetVersionStr,
                    DllPath = hasManagedDll ? postLocalInfo.DllPath : string.Empty,
                    IsRuntimePackage = isPureNative,
                    HasNativeAssets = hasNative,
                    PackageRootDirectory = postLocalInfo.PackageRootDirectory
                };
            }
        }

        _logger.LogWarning("[PKGSOLVER-RESOLVED-FAILED] No fue posible resolver o localizar la librería DLL o activos de runtime para '{PackageId}' v'{Version}'.",
            packageId, targetVersionStr);

        return new PackageResolutionResult
        {
            Success = false,
            PackageId = packageId,
            Version = targetVersionStr,
            ErrorMessage = $"No fue posible resolver o localizar la librería DLL o activos de runtime para '{packageId}' versión '{targetVersionStr}'."
        };
    }

    private NugetPackageVersionInformation? ResolveLocalPackageInfo(string packageName, string version)
    {
        if (string.IsNullOrWhiteSpace(packageName) || string.IsNullOrWhiteSpace(version))
            return null;

        var frameworkSolicitado = AssemblyHelpers.GetRequestedFramework();

        foreach (var searchPath in _searchPaths)
        {
            if (!Directory.Exists(searchPath)) continue;

            var packageDirs = Directory.GetDirectories(searchPath, packageName.ToLowerInvariant());
            if (!packageDirs.Any())
            {
                packageDirs = Directory.GetDirectories(searchPath, packageName);
            }

            if (!packageDirs.Any()) continue;

            var targetPackageDir = packageDirs.First();
            var versionDir = Path.Combine(targetPackageDir, version);

            if (Directory.Exists(versionDir))
            {
                _logger.LogTrace("[PKGSOLVER-LOCAL-VERSION-DIR] Directorio de versión encontrado: '{VersionDir}'. Inspeccionando TFM y runtimes...", versionDir);
                var versionList = NugetHelpers.GetNugetPackageVersionInformation(_logger, targetPackageDir);
                var versionSeleccionada = NugetHelpers.SelectedVersion(
                    _settings.FrameworkPriorities,
                    version,
                    frameworkSolicitado,
                    versionList,
                    exactmatch: true,
                    exactFramework: false);

                if (versionSeleccionada != null)
                {
                    return versionSeleccionada;
                }
            }

            // Búsqueda flexible si no hubo coincidencia exacta
            _logger.LogTrace("[PKGSOLVER-LOCAL-FLEX-SEARCH] Evaluando coincidencia flexible para '{PackageName}' en '{TargetPackageDir}'...",
                packageName, targetPackageDir);

            var allVersionInfo = NugetHelpers.GetNugetPackageVersionInformation(_logger, targetPackageDir);
            var flexibleSelection = NugetHelpers.SelectedVersion(
                _settings.FrameworkPriorities,
                version,
                frameworkSolicitado,
                allVersionInfo,
                exactmatch: false,
                exactFramework: false);

            if (flexibleSelection != null)
            {
                return flexibleSelection;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public string? ResolveLocalDllPath(string packageName, string version)
    {
        var info = ResolveLocalPackageInfo(packageName, version);
        if (info != null && !string.IsNullOrEmpty(info.DllPath) && File.Exists(info.DllPath))
        {
            return info.DllPath;
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<string?> DownloadAndInstallPackageAsync(string packageName, string version)
    {
        if (string.IsNullOrWhiteSpace(packageName) || string.IsNullOrWhiteSpace(version))
            return null;

        if (!NuGetVersion.TryParse(version, out var nugetVersion))
        {
            _logger.LogWarning("[PKGSOLVER-DOWNLOAD-INVALID-VER] Versión no válida para descarga: '{Version}'.", version);
            return null;
        }

        if (_settings.NugetServers == null || !_settings.NugetServers.Any())
        {
            _logger.LogDebug("[PKGSOLVER-DOWNLOAD-NO-SERVERS] No hay servidores NuGet remotos configurados.");
            return null;
        }

        var targetDirectory = Path.Combine(_installFolder, packageName.ToLowerInvariant(), version);
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        foreach (var server in _settings.NugetServers.Where(s => s.Active).OrderBy(s => s.Order))
        {
            using var downloadCts = new CancellationTokenSource(DefaultRemoteDownloadTimeout);
            try
            {
                var normalizedUrl = NormalizeNugetServerUrl(server.Server, server.Version);
                _logger.LogDebug("[PKGSOLVER-DOWNLOAD-ATTEMPT] Intentando descargar '{PackageName}' {Version} desde '{ServerName}' ({NormalizedUrl})...",
                    packageName, version, server.Name ?? "NuGet", normalizedUrl);

                var repository = Repository.Factory.GetCoreV3(normalizedUrl);
                var resource = await repository.GetResourceAsync<FindPackageByIdResource>(downloadCts.Token)
                    .WaitAsync(DefaultRemoteDownloadTimeout, downloadCts.Token)
                    .ConfigureAwait(false);

                if (resource != null)
                {
                    using var cacheContext = new SourceCacheContext();
                    using var memoryStream = new MemoryStream();

                    var copied = await resource.CopyNupkgToStreamAsync(
                        packageName,
                        nugetVersion,
                        memoryStream,
                        cacheContext,
                        NullLogger.Instance,
                        downloadCts.Token)
                        .WaitAsync(DefaultRemoteDownloadTimeout, downloadCts.Token)
                        .ConfigureAwait(false);

                    if (!copied && !string.Equals(packageName, packageName.ToLowerInvariant(), StringComparison.Ordinal))
                    {
                        _logger.LogTrace("[PKGSOLVER-DOWNLOAD-FALLBACK-LOWER] Reintentando descarga con ID en minúsculas: '{PackageNameLower}'...",
                            packageName.ToLowerInvariant());

                        copied = await resource.CopyNupkgToStreamAsync(
                            packageName.ToLowerInvariant(),
                            nugetVersion,
                            memoryStream,
                            cacheContext,
                            NullLogger.Instance,
                            downloadCts.Token)
                            .WaitAsync(DefaultRemoteDownloadTimeout, downloadCts.Token)
                            .ConfigureAwait(false);
                    }

                    if (copied)
                    {
                        memoryStream.Position = 0;
                        using var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                        zipArchive.ExtractToDirectory(targetDirectory, overwriteFiles: true);

                        _logger.LogInformation("[PKGSOLVER-DOWNLOAD-SUCCESS] Paquete '{PackageName}' {Version} ({ByteCount} bytes) descargado y extraído en '{TargetDirectory}'.",
                            packageName, version, memoryStream.Length, targetDirectory);

                        return targetDirectory;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[PKGSOLVER-DOWNLOAD-TIMEOUT] Tiempo de espera agotado ({TimeoutSeconds}s) al descargar paquete '{PackageName}' {Version} desde servidor '{Server}'.",
                    DefaultRemoteDownloadTimeout.TotalSeconds, packageName, version, server.Server);
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("[PKGSOLVER-DOWNLOAD-TIMEOUT] Tiempo de espera estricto agotado ({TimeoutSeconds}s) al descargar paquete '{PackageName}' {Version} desde servidor '{Server}'.",
                    DefaultRemoteDownloadTimeout.TotalSeconds, packageName, version, server.Server);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[PKGSOLVER-DOWNLOAD-ERROR] Error descargando paquete '{PackageName}' {Version} desde servidor '{Server}'.",
                    packageName, version, server.Server);
            }
        }

        _logger.LogWarning("[PKGSOLVER-DOWNLOAD-FAIL-ALL] Ningún servidor remoto pudo suministrar el paquete '{PackageName}' versión '{Version}'.",
            packageName, version);
        return null;
    }

    private string? SelectBestMatchingVersion(
        string packageId,
        string? requestedVersion,
        VersionResources versionResources)
    {
        if (versionResources == null || versionResources.Sources.Count == 0)
            return requestedVersion;

        var allDiscoveredVersions = versionResources.Sources
            .SelectMany(s => s.Versions.Select(v => v.Version))
            .Distinct()
            .OrderByDescending(v => v)
            .ToList();

        if (allDiscoveredVersions.Count == 0)
            return requestedVersion;

        // 1. Si no se solicitó versión explícita (null, vacía, o "any")
        if (string.IsNullOrWhiteSpace(requestedVersion) ||
            string.Equals(requestedVersion, "any", StringComparison.OrdinalIgnoreCase))
        {
            var unblockedLatest = allDiscoveredVersions.FirstOrDefault(v => !IsVersionBlocked(packageId, v.ToNormalizedString(), out _));
            return (unblockedLatest ?? allDiscoveredVersions.First()).ToNormalizedString();
        }

        // 2. Coincidencia exacta de cadena o NuGetVersion
        if (NuGetVersion.TryParse(requestedVersion, out var reqNugetVersion))
        {
            var exactMatch = allDiscoveredVersions.FirstOrDefault(v => v.Equals(reqNugetVersion));
            if (exactMatch != null)
            {
                return exactMatch.ToNormalizedString();
            }
        }
        else
        {
            var exactStrMatch = allDiscoveredVersions.FirstOrDefault(v =>
                string.Equals(v.ToNormalizedString(), requestedVersion, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(v.OriginalVersion, requestedVersion, StringComparison.OrdinalIgnoreCase));

            if (exactStrMatch != null)
            {
                return exactStrMatch.ToNormalizedString();
            }
        }

        // 3. Normalización de versiones de ensamblado de 4 partes (.NET System.Version) ej: 10.0.7.0 -> 10.0.7
        int? reqMajor = reqNugetVersion?.Major;
        int? reqMinor = reqNugetVersion?.Minor;
        int? reqPatch = reqNugetVersion?.Patch;

        if (Version.TryParse(requestedVersion, out var sysVersion))
        {
            reqMajor = sysVersion.Major;
            reqMinor = sysVersion.Minor;
            reqPatch = sysVersion.Build >= 0 ? sysVersion.Build : 0;

            if (sysVersion.Revision == 0 && sysVersion.Build >= 0)
            {
                var threePartString = $"{sysVersion.Major}.{sysVersion.Minor}.{sysVersion.Build}";
                if (NuGetVersion.TryParse(threePartString, out var nuget3Part))
                {
                    var match3 = allDiscoveredVersions.FirstOrDefault(v => v.Equals(nuget3Part));
                    if (match3 != null)
                    {
                        return match3.ToNormalizedString();
                    }
                }
            }
        }

        // 4. Coincidencia en Major.Minor.Patch (con o sin prerelease, ej: 10.0.7.0 o 10.0.7 coincide con 10.0.7-beta)
        if (reqMajor.HasValue && reqMinor.HasValue && reqPatch.HasValue)
        {
            var samePatchCandidates = allDiscoveredVersions
                .Where(v => v.Major == reqMajor.Value && v.Minor == reqMinor.Value && v.Patch == reqPatch.Value)
                .ToList();

            if (samePatchCandidates.Count > 0)
            {
                // Preferir versiones estables si existen, luego prereleases ordenadas descendentemente
                var stableMatch = samePatchCandidates.FirstOrDefault(v => !v.IsPrerelease);
                if (stableMatch != null)
                {
                    return stableMatch.ToNormalizedString();
                }

                return samePatchCandidates.First().ToNormalizedString();
            }

            // 5. Coincidencia en Major.Minor (versión más alta compatible >= requestedVersion)
            var sameMinorCandidates = allDiscoveredVersions
                .Where(v => v.Major == reqMajor.Value && v.Minor == reqMinor.Value && (reqNugetVersion == null || v >= reqNugetVersion))
                .OrderBy(v => v)
                .ToList();

            if (sameMinorCandidates.Count > 0)
            {
                return sameMinorCandidates.First().ToNormalizedString();
            }

            // 6. Coincidencia en Major
            var sameMajorCandidates = allDiscoveredVersions
                .Where(v => v.Major == reqMajor.Value && (reqNugetVersion == null || v >= reqNugetVersion))
                .OrderBy(v => v)
                .ToList();

            if (sameMajorCandidates.Count > 0)
            {
                return sameMajorCandidates.First().ToNormalizedString();
            }
        }

        // 7. Heurística de fallback usando NugetHelpers
        var rawDiscovered = allDiscoveredVersions.Select(v => v.ToNormalizedString()).ToList();
        var bestHeuristic = NugetHelpers.SelectBestRemoteVersion(requestedVersion, rawDiscovered);
        if (!string.IsNullOrEmpty(bestHeuristic))
        {
            var matchedCandidate = allDiscoveredVersions.FirstOrDefault(v =>
                string.Equals(v.ToNormalizedString(), bestHeuristic, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(v.OriginalVersion, bestHeuristic, StringComparison.OrdinalIgnoreCase));

            if (matchedCandidate != null)
            {
                return matchedCandidate.ToNormalizedString();
            }
        }

        // Si ninguna regla previa coincidió pero existen versiones descubiertas, retornar la más reciente no bloqueada
        var fallbackUnblocked = allDiscoveredVersions.FirstOrDefault(v => !IsVersionBlocked(packageId, v.ToNormalizedString(), out _));
        return (fallbackUnblocked ?? allDiscoveredVersions.First()).ToNormalizedString();
    }

    private string GetPackageName(string assemblyName)
    {
        if (_settings.AssemblyToPackageMap != null &&
            _settings.AssemblyToPackageMap.TryGetValue(assemblyName, out var mappedPackage))
        {
            return mappedPackage;
        }

        return assemblyName;
    }

    private bool IsVersionBlocked(string packageId, string? version, out BlockedPackageItem? matchedItem)
    {
        matchedItem = null;
        if (_settings.BlockedPackages == null || !_settings.BlockedPackages.Any() || string.IsNullOrEmpty(version))
            return false;

        foreach (var blocked in _settings.BlockedPackages.Where(b => b.Active))
        {
            if (string.Equals(blocked.PackageId, packageId, StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(blocked.Version, version, StringComparison.OrdinalIgnoreCase) ||
                    (NuGetVersion.TryParse(blocked.Version, out var blockedVer) &&
                     NuGetVersion.TryParse(version, out var targetVer) &&
                     blockedVer.Equals(targetVer)))
                {
                    matchedItem = blocked;
                    return true;
                }
            }
        }

        return false;
    }

    private NuGetVersion? SelectLowestHigherUnblockedVersion(
        string packageId,
        NuGetVersion? requestedVersion,
        VersionResources versionResources)
    {
        if (requestedVersion == null) return null;

        var allDiscoveredVersions = versionResources.Sources
            .SelectMany(s => s.Versions.Select(v => v.Version))
            .Distinct()
            .OrderBy(v => v)
            .ToList();

        var candidateVersions = allDiscoveredVersions
            .Where(v => v > requestedVersion)
            .ToList();

        foreach (var candidate in candidateVersions)
        {
            var candidateStr = candidate.ToNormalizedString();
            if (!IsVersionBlocked(packageId, candidateStr, out _))
            {
                return candidate;
            }
        }

        return null;
    }

    private static IReadOnlyList<PackageVersionInfo> FindLocalVersions(
        string basePath,
        string packageId,
        NuGetVersion? requestedVersion)
    {
        var packageDirectory = Path.Combine(basePath, packageId.ToLowerInvariant());
        if (!Directory.Exists(packageDirectory))
        {
            packageDirectory = Path.Combine(basePath, packageId);
        }

        if (!Directory.Exists(packageDirectory))
            return Array.Empty<PackageVersionInfo>();

        var versions = new List<PackageVersionInfo>();

        foreach (var directory in Directory.EnumerateDirectories(packageDirectory))
        {
            var versionText = Path.GetFileName(directory);
            if (!NuGetVersion.TryParse(versionText, out var version))
                continue;

            var normalizedId = packageId.ToLowerInvariant();
            var normalizedVersion = version.ToNormalizedString().ToLowerInvariant();

            var nupkgPath = Path.Combine(directory, $"{normalizedId}.{normalizedVersion}.nupkg");
            var localPath = File.Exists(nupkgPath) ? nupkgPath : directory;

            versions.Add(new PackageVersionInfo(
                Version: version,
                IsRequestedVersion: requestedVersion != null && version.Equals(requestedVersion),
                LocalPath: localPath));
        }

        return versions
            .DistinctBy(v => v.Version)
            .OrderByDescending(v => v.Version)
            .ToArray();
    }

    private static async Task<IReadOnlyList<PackageVersionInfo>> FindRemoteVersionsAsync(
        NugetServerItem server,
        string packageId,
        NuGetVersion? requestedVersion,
        CancellationToken cancellationToken)
    {
        var normalizedUrl = NormalizeNugetServerUrl(server.Server, server.Version);
        if (string.IsNullOrWhiteSpace(normalizedUrl))
            return Array.Empty<PackageVersionInfo>();

        using var queryCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        queryCts.CancelAfter(DefaultRemoteQueryTimeout);

        try
        {
            var repository = Repository.Factory.GetCoreV3(normalizedUrl);
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>(queryCts.Token)
                .WaitAsync(DefaultRemoteQueryTimeout, queryCts.Token)
                .ConfigureAwait(false);

            if (resource == null)
                return Array.Empty<PackageVersionInfo>();

            using var cacheContext = new SourceCacheContext();

            IEnumerable<NuGetVersion>? versions = null;
            try
            {
                versions = await resource.GetAllVersionsAsync(
                    packageId,
                    cacheContext,
                    NullLogger.Instance,
                    queryCts.Token)
                    .WaitAsync(DefaultRemoteQueryTimeout, queryCts.Token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (TimeoutException)
            {
                throw;
            }
            catch
            {
                // Fallback a minúsculas si el endpoint remoto es sensible
            }

            if ((versions == null || !versions.Any()) && !string.Equals(packageId, packageId.ToLowerInvariant(), StringComparison.Ordinal))
            {
                try
                {
                    versions = await resource.GetAllVersionsAsync(
                        packageId.ToLowerInvariant(),
                        cacheContext,
                        NullLogger.Instance,
                        queryCts.Token)
                        .WaitAsync(DefaultRemoteQueryTimeout, queryCts.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (TimeoutException)
                {
                    throw;
                }
                catch
                {
                    // Ignorar error de fallback
                }
            }

            if (versions == null)
                return Array.Empty<PackageVersionInfo>();

            return versions
                .Distinct()
                .OrderByDescending(v => v)
                .Select(v => new PackageVersionInfo(
                    Version: v,
                    IsRequestedVersion: requestedVersion != null && v.Equals(requestedVersion),
                    LocalPath: null))
                .ToArray();
        }
        catch (OperationCanceledException)
        {
            return Array.Empty<PackageVersionInfo>();
        }
        catch (TimeoutException)
        {
            return Array.Empty<PackageVersionInfo>();
        }
    }

    private static string NormalizeNugetServerUrl(string? url, int? version = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        var trimmed = url.Trim();

        // 1. Normalización automática de nuget.org a su feed oficial V3 HTTPS
        if (trimmed.Contains("nuget.org", StringComparison.OrdinalIgnoreCase))
        {
            return "https://api.nuget.org/v3/index.json";
        }

        // 2. Si es una URL V3 o está marcada con versión 3, asegurar que termine en index.json si apunta a /v3
        if (version == 3 || trimmed.EndsWith("/v3", StringComparison.OrdinalIgnoreCase) || trimmed.EndsWith("/v3/", StringComparison.OrdinalIgnoreCase))
        {
            if (!trimmed.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.TrimEnd('/') + "/index.json";
            }
        }

        return trimmed;
    }
}
