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
    /// Consulta y consolida los recursos de versiones disponibles para un paquete tanto en repositorios locales como en servidores NuGet remotos.
    /// </summary>
    public async Task<VersionResources> FindVersionResourcesAsync(
        string packageId,
        NuGetVersion? requestedVersion = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        var cachedResources = _assemblyCache.GetVersionResources(packageId);
        if (cachedResources != null)
        {
            _logger.LogDebug("[PKGSOLVER-VR-CACHE-HIT] VersionResources para '{PackageId}' obtenidos de caché unificada ({Count} orígenes).",
                packageId,
                cachedResources.Sources.Count);
            return cachedResources;
        }

        _logger.LogDebug("[PKGSOLVER-VR-FETCH] Consultando orígenes de versiones para '{PackageId}' (versión solicitada: {RequestedVersion})...",
            packageId,
            requestedVersion?.ToNormalizedString() ?? "any");

        var versionResources = new VersionResources
        {
            PackageId = packageId,
            RetrievedAtUtc = DateTime.UtcNow
        };

        // 1. Escaneo de orígenes locales
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

        // 2. Consulta de servidores remotos en paralelo con timeouts individuales
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
                    if (remoteVersions.Count > 0)
                    {
                        _logger.LogInformation("[PKGSOLVER-REMOTE-VERSIONS] Servidor remoto '{ServerName}': {Count} versión(es) encontrada(s) para '{PackageId}'.",
                            server.Name ?? server.Server,
                            remoteVersions.Count,
                            packageId);

                        versionResources.Sources.Add(new PackageSourceResource(
                            SourceName: server.Name ?? server.Server,
                            IsLocal: false,
                            Versions: remoteVersions));
                    }
                    else
                    {
                        _logger.LogDebug("[PKGSOLVER-REMOTE-VERSIONS-NONE] Servidor remoto '{ServerName}': no se encontraron versiones para '{PackageId}'.",
                            server.Name ?? server.Server,
                            packageId);
                    }
                }
            }
        }

        _assemblyCache.SetVersionResources(packageId, versionResources);
        _logger.LogDebug("[PKGSOLVER-VR-SAVED] VersionResources consolidados y guardados en caché para '{PackageId}' ({Count} orígenes).",
            packageId,
            versionResources.Sources.Count);
        return versionResources;
    }

    /// <summary>
    /// Resuelve un paquete aplicando el mapeo de nombres, búsqueda local/remota y directivas de BlockedPackagesPolicy.
    /// </summary>
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

        // 1. Consultar VersionResources (Caché -> Local -> Remoto)
        var versionResources = await FindVersionResourcesAsync(packageId, requestedNuGetVersion).ConfigureAwait(false);

        // 2. Seleccionar la mejor versión candidata descubierta en VersionResources
        var targetVersionStr = SelectBestMatchingVersion(packageId, requestedVersion, versionResources);
        if (!string.Equals(targetVersionStr, requestedVersion, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("[PKGSOLVER-VERSION-SELECTED] Versión solicitada '{RequestedVersion}' ajustada a versión candidata '{SelectedVersion}' para '{PackageId}'.",
                requestedVersion ?? "any",
                targetVersionStr ?? "none",
                packageId);
        }

        // 3. Evaluar BlockedPackages
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
                    _logger.LogDebug("[PKGSOLVER-POLICY-MAYOR] Buscando menor versión superior no bloqueada en VersionResources para '{PackageId}'...", packageId);
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

        // 4. Resolución física local
        string? resolvedDllPath = null;
        if (!string.IsNullOrEmpty(targetVersionStr))
        {
            _logger.LogDebug("[PKGSOLVER-LOCAL-CHECK] Comprobando existencia de DLL local para '{PackageId}' v'{Version}'...",
                packageId, targetVersionStr);
            resolvedDllPath = ResolveLocalDllPath(packageId, targetVersionStr);
        }

        // 4. Si no se encuentra localmente, descargar e instalar
        if (resolvedDllPath == null && !string.IsNullOrEmpty(targetVersionStr))
        {
            _logger.LogInformation("[PKGSOLVER-REMOTE-DOWNLOAD-TRIGGER] Paquete '{PackageId}' v'{Version}' no localizado localmente. Iniciando descarga remota...",
                packageId, targetVersionStr);

            var downloaded = await DownloadAndInstallPackageAsync(packageId, targetVersionStr).ConfigureAwait(false);
            if (downloaded != null)
            {
                _logger.LogDebug("[PKGSOLVER-POST-DOWNLOAD-RESOLVE] Localizando DLL tras descarga e instalación en '{DownloadedDir}'...", downloaded);
                resolvedDllPath = ResolveLocalDllPath(packageId, targetVersionStr);
            }
        }

        if (resolvedDllPath != null)
        {
            _logger.LogInformation("[PKGSOLVER-RESOLVED-SUCCESS] Paquete '{PackageId}' v'{Version}' resuelto exitosamente -> '{DllPath}'",
                packageId, targetVersionStr, resolvedDllPath);

            return new PackageResolutionResult
            {
                Success = true,
                PackageId = packageId,
                Version = targetVersionStr,
                DllPath = resolvedDllPath
            };
        }

        _logger.LogWarning("[PKGSOLVER-RESOLVED-FAILED] No fue posible resolver o localizar la librería DLL para '{PackageId}' v'{Version}'.",
            packageId, targetVersionStr);

        return new PackageResolutionResult
        {
            Success = false,
            PackageId = packageId,
            Version = targetVersionStr,
            ErrorMessage = $"No fue posible resolver o localizar la librería DLL para '{packageId}' versión '{targetVersionStr}'."
        };
    }

    /// <summary>
    /// Localiza la ruta a la librería DLL de un paquete instalado en las carpetas locales.
    /// </summary>
    public string? ResolveLocalDllPath(string packageName, string version)
    {
        if (string.IsNullOrWhiteSpace(packageName) || string.IsNullOrWhiteSpace(version))
            return null;

        var frameworkSolicitado = AssemblyHelpers.GetRequestedFramework();
        _logger.LogTrace("[PKGSOLVER-LOCAL-DLL] Buscando DLL local para '{PackageName}' v'{Version}' (TFM solicitado: '{Framework}')...",
            packageName,
            version,
            frameworkSolicitado);

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
                _logger.LogTrace("[PKGSOLVER-LOCAL-VERSION-DIR] Directorio de versión encontrado: '{VersionDir}'. Inspeccionando TFM...", versionDir);
                var versionList = NugetHelpers.GetNugetPackageVersionInformation(_logger, targetPackageDir);
                var versionSeleccionada = NugetHelpers.SelectedVersion(
                    _settings.FrameworkPriorities,
                    version,
                    frameworkSolicitado,
                    versionList,
                    exactmatch: true,
                    exactFramework: false);

                if (versionSeleccionada != null && File.Exists(versionSeleccionada.DllPath))
                {
                    _logger.LogDebug("[PKGSOLVER-LOCAL-DLL-FOUND] Coincidencia exacta de DLL: '{DllPath}' (Framework: '{Framework}')",
                        versionSeleccionada.DllPath,
                        versionSeleccionada.Framework);
                    return versionSeleccionada.DllPath;
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

            if (flexibleSelection != null && File.Exists(flexibleSelection.DllPath))
            {
                _logger.LogInformation("[PKGSOLVER-LOCAL-FLEX-FOUND] Coincidencia flexible de DLL: '{DllPath}' (Versión: '{Version}', Framework: '{Framework}')",
                    flexibleSelection.DllPath,
                    flexibleSelection.VersionString,
                    flexibleSelection.Framework);
                return flexibleSelection.DllPath;
            }
        }

        _logger.LogTrace("[PKGSOLVER-LOCAL-DLL-NOTFOUND] No se encontró DLL local para '{PackageName}' v'{Version}'.", packageName, version);
        return null;
    }

    /// <summary>
    /// Descarga un paquete NuGet desde los servidores configurados y lo extrae en la carpeta de instalación.
    /// </summary>
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
