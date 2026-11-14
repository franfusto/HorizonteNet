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
            _logger.LogDebug("VersionResources para '{PackageId}' obtenidos de caché unificada.", packageId);
            return cachedResources;
        }

        var versionResources = new VersionResources
        {
            PackageId = packageId,
            RetrievedAtUtc = DateTime.UtcNow
        };

        // 1. Escaneo de orígenes locales
        foreach (var searchPath in _searchPaths)
        {
            if (!Directory.Exists(searchPath)) continue;

            var localVersions = FindLocalVersions(searchPath, packageId, requestedVersion);
            if (localVersions.Count > 0)
            {
                versionResources.Sources.Add(new PackageSourceResource(
                    SourceName: searchPath,
                    IsLocal: true,
                    Versions: localVersions));
            }
        }

        // 2. Consulta de servidores remotos
        if (_settings.NugetServers != null)
        {
            foreach (var server in _settings.NugetServers.Where(s => s.Active).OrderBy(s => s.Order))
            {
                try
                {
                    var remoteVersions = await FindRemoteVersionsAsync(server, packageId, requestedVersion, cancellationToken);
                    if (remoteVersions.Count > 0)
                    {
                        versionResources.Sources.Add(new PackageSourceResource(
                            SourceName: server.Name ?? server.Server,
                            IsLocal: false,
                            Versions: remoteVersions));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al consultar versiones remotas para '{PackageId}' en servidor '{Server}'.",
                        packageId, server.Server);
                }
            }
        }

        _assemblyCache.SetVersionResources(packageId, versionResources);
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

        var packageId = GetPackageName(packageOrAssemblyName);
        NuGetVersion.TryParse(requestedVersion, out var requestedNuGetVersion);

        // 1. Consultar VersionResources (Caché -> Local -> Remoto)
        var versionResources = await FindVersionResourcesAsync(packageId, requestedNuGetVersion);

        // 2. Evaluar BlockedPackages
        var targetVersionStr = requestedVersion;
        if (IsVersionBlocked(packageId, requestedVersion, out var blockedItem))
        {
            _logger.LogWarning("Paquete '{PackageId}' versión '{Version}' bloqueado por directiva de seguridad: {Reason}",
                packageId, requestedVersion, blockedItem?.Reason);

            switch (_settings.BlockedPackagesPolicy)
            {
                case BlockedPackagesPolicy.Null:
                    return new PackageResolutionResult
                    {
                        Success = false,
                        Blocked = true,
                        PackageId = packageId,
                        Version = requestedVersion,
                        ErrorMessage = $"Paquete bloqueado por política de seguridad (Null): {blockedItem?.Reason}"
                    };

                case BlockedPackagesPolicy.Error:
                    var errorMsg = $"Resolución rechazada. Paquete '{packageId}' versión '{requestedVersion}' bloqueado: {blockedItem?.Reason}";
                    _logger.LogError(errorMsg);
                    throw new InvalidOperationException(errorMsg);

                case BlockedPackagesPolicy.Mayor:
                    var higherVersion = SelectLowestHigherUnblockedVersion(packageId, requestedNuGetVersion, versionResources);
                    if (higherVersion == null)
                    {
                        var noHigherMsg = $"No se encontró ninguna versión superior no bloqueada para el paquete '{packageId}' (solicitada: '{requestedVersion}').";
                        _logger.LogError(noHigherMsg);
                        return new PackageResolutionResult
                        {
                            Success = false,
                            Blocked = true,
                            PackageId = packageId,
                            Version = requestedVersion,
                            ErrorMessage = noHigherMsg
                        };
                    }

                    _logger.LogInformation("Política 'Mayor' seleccionó la versión superior '{HigherVersion}' para el paquete '{PackageId}'.",
                        higherVersion.ToNormalizedString(), packageId);
                    targetVersionStr = higherVersion.ToNormalizedString();
                    break;
            }
        }

        // 3. Resolución física local
        string? resolvedDllPath = null;
        if (!string.IsNullOrEmpty(targetVersionStr))
        {
            resolvedDllPath = ResolveLocalDllPath(packageId, targetVersionStr);
        }

        // 4. Si no se encuentra localmente, descargar e instalar
        if (resolvedDllPath == null && !string.IsNullOrEmpty(targetVersionStr))
        {
            var downloaded = await DownloadAndInstallPackageAsync(packageId, targetVersionStr);
            if (downloaded != null)
            {
                resolvedDllPath = ResolveLocalDllPath(packageId, targetVersionStr);
            }
        }

        if (resolvedDllPath != null)
        {
            return new PackageResolutionResult
            {
                Success = true,
                PackageId = packageId,
                Version = targetVersionStr,
                DllPath = resolvedDllPath
            };
        }

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
                    return versionSeleccionada.DllPath;
                }
            }

            // Búsqueda flexible si no hubo coincidencia exacta
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
                return flexibleSelection.DllPath;
            }
        }

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
            _logger.LogWarning("Versión no válida para descarga: '{Version}'.", version);
            return null;
        }

        if (_settings.NugetServers == null || !_settings.NugetServers.Any())
            return null;

        var targetDirectory = Path.Combine(_installFolder, packageName.ToLowerInvariant(), version);
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        foreach (var server in _settings.NugetServers.Where(s => s.Active).OrderBy(s => s.Order))
        {
            try
            {
                var normalizedUrl = NormalizeNugetServerUrl(server.Server, server.Version);
                var repository = Repository.Factory.GetCoreV3(normalizedUrl);
                var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

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
                        CancellationToken.None);

                    if (!copied && !string.Equals(packageName, packageName.ToLowerInvariant(), StringComparison.Ordinal))
                    {
                        copied = await resource.CopyNupkgToStreamAsync(
                            packageName.ToLowerInvariant(),
                            nugetVersion,
                            memoryStream,
                            cacheContext,
                            NullLogger.Instance,
                            CancellationToken.None);
                    }

                    if (copied)
                    {
                        memoryStream.Position = 0;
                        using var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                        zipArchive.ExtractToDirectory(targetDirectory, overwriteFiles: true);

                        _logger.LogInformation("Paquete '{PackageName}' {Version} descargado y extraído en '{TargetDirectory}'.",
                            packageName, version, targetDirectory);

                        return targetDirectory;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error descargando paquete '{PackageName}' {Version} desde servidor '{Server}'.",
                    packageName, version, server.Server);
            }
        }

        return null;
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
        var repository = Repository.Factory.GetCoreV3(normalizedUrl);
        var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

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
                cancellationToken);
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
                    cancellationToken);
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

    private static string NormalizeNugetServerUrl(string? url, int? version = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        var trimmed = url.Trim();

        // Si es una URL V3 o está marcada con versión 3, asegurar que termine en index.json si apunta a /v3
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
