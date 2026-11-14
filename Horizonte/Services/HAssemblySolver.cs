using System.Collections.Concurrent;
using System.Reflection;
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
    private readonly List<string> _searchPaths = new();
    private string _installFolder = "packages";

    private readonly ConcurrentDictionary<string, byte[]> _memoryAssemblyCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _assemblyToPackageMapLock = new();
    private Dictionary<string, string> _assemblyToPackageMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _processedAssemblyMappingDefinitions = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _processingAssemblyMappingDefinitions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registro de archivos estáticos y activos web indexados por dominio.
    /// </summary>
    public StaticFileRegistry StaticFileRegistry { get; } = new();

    public HAssemblySolver(
        IServiceProvider serviceProvider,
        ISymLinkScafolder linkScafolder,
        ILogger<HAssemblySolver> logger)
    {
        _serviceProvider = serviceProvider;
        _linkScafolder = linkScafolder;
        _logger = logger;
        _settings = serviceProvider.GetService<IhContext>()?.Get<ModulesSettings>() ?? new ModulesSettings();

        MapAssemblyToPackage();
        SetUpAssemblyPaths();
    }

    /// <summary>
    /// Intenta resolver y cargar un ensamblado dado su contexto de carga y su nombre.
    /// </summary>
    public Assembly? ResolveAssemblyFromALC(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        return ResolveAssemblyFromNuGetPackages(context, new ResolveEventArgs(assemblyName.FullName));
    }

    /// <summary>
    /// Resuelve y carga un ensamblado a partir de los repositorios NuGet locales o remotos.
    /// </summary>
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
                "Resolving assembly: {AssemblyName} requested by domain {DomainName}",
                args.Name,
                requesterDomain);

            var (name, version) = AssemblyHelpers.ParseAssemblyName(args.Name);

            if (AssemblyHelpers.TryCreateAssemblyNameForRuntimeResolution(args.Name, out var requestedAssemblyName))
            {
                var sharedAssembly = AssemblyHelpers.TryGetSharedAssemblyFromDefault(requestedAssemblyName);
                if (sharedAssembly != null)
                {
                    _logger.LogInformation(
                        "Sharing assembly {AssemblyName} from domain Default instead of loading it into plugin domain.",
                        sharedAssembly.FullName);
                    return sharedAssembly;
                }
            }
            else
            {
                _logger.LogDebug(
                    "Skipping AssemblyName parsing for {AssemblyName} because it is not a valid runtime assembly identity. It will be resolved as NuGet package {PackageName} {Version}.",
                    args.Name,
                    name,
                    version);
            }

            var packageName = GetPackageName(name);

            if (!string.Equals(name, packageName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "Assembly {AssemblyName} mapped to NuGet package {PackageName}",
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
                _logger.LogInformation(
                    "Assembly {AssemblyName} ya estaba cargado en el dominio solicitado {DomainName}",
                    alreadyLoaded.FullName,
                    loadedAlc != null
                        ? (domainsDict != null
                            ? AssemblyHelpers.GetDomainNameForAssemblyLoadContext(domainsDict, loadedAlc)
                            : loadedAlc.Name ?? Const.DefaultDomainName)
                        : Const.DefaultDomainName);

                MapAssemblyToPackageFromAssembly(alreadyLoaded);
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

            var resAssemblyPath = ResolveNugetFromLocalDirectory(packageName, version, null, true);

            if (resAssemblyPath == null)
            {
                resAssemblyPath = ResolveNugetFromLocalDirectory(packageName, version, null, false);
            }

            if (resAssemblyPath == null)
            {
                DownloadAndExtractPackage(packageName, version);
                resAssemblyPath = ResolveNugetFromLocalDirectory(packageName, version, null, false);
            }

            if (resAssemblyPath == null)
            {
                _logger.LogError(
                    "Could not resolve assembly: {AssemblyName} from NuGet package {PackageName} requested by domain {DomainName}",
                    args.Name,
                    packageName,
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
                    domainsDict != null
                        ? AssemblyHelpers.GetDomainNameForAssemblyLoadContext(domainsDict, context)
                        : context.Name ?? Const.DefaultDomainName);
                return loadedAssembly;
            }

            var defaultAssembly = Assembly.LoadFrom(resAssemblyPath);
            MapAssemblyToPackageFromAssembly(defaultAssembly);

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

    /// <summary>
    /// Resuelve la ruta del archivo DLL de un paquete NuGet específico, dado su nombre y versión.
    /// </summary>
    public string? ResolveAssemblyDllPath(string packageName, string version)
    {
        var resolvedPackageName = GetPackageName(packageName);
        var path = ResolveNugetFromLocalDirectory(resolvedPackageName, version, null, true);

        if (path == null)
        {
            path = ResolveNugetFromLocalDirectory(resolvedPackageName, version, null, false);
        }

        if (path != null && !Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(path);
        }

        return path;
    }

    /// <summary>
    /// Descarga de forma asíncrona un paquete NuGet si no existe localmente.
    /// </summary>
    public Task<string?> DownloadNuGetPackageAsync(string packageName, string version)
    {
        return Task.Run(() =>
        {
            var resolvedPackageName = GetPackageName(packageName);
            DownloadAndExtractPackage(resolvedPackageName, version);
            return ResolveAssemblyDllPath(resolvedPackageName, version);
        });
    }

    /// <summary>
    /// Obtiene el nombre del paquete mapeado para un nombre de ensamblado.
    /// </summary>
    public string GetPackageName(string assemblyName)
    {
        lock (_assemblyToPackageMapLock)
        {
            return _assemblyToPackageMap.TryGetValue(assemblyName, out var packageName)
                ? packageName
                : assemblyName;
        }
    }

    /// <summary>
    /// Guarda un ensamblado en bytes en la memoria caché rápida.
    /// </summary>
    public void CacheAssembly(string key, byte[] assemblyBytes)
    {
        if (string.IsNullOrWhiteSpace(key) || assemblyBytes == null) return;
        _memoryAssemblyCache[key] = assemblyBytes;
    }

    /// <summary>
    /// Obtiene un ensamblado en bytes desde la caché en memoria.
    /// </summary>
    public byte[]? GetCachedAssembly(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        _memoryAssemblyCache.TryGetValue(key, out var bytes);
        return bytes;
    }

    /// <summary>
    /// Verifica si un ensamblado se encuentra en la caché en memoria.
    /// </summary>
    public bool IsAssemblyCached(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        return _memoryAssemblyCache.ContainsKey(key);
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
                    var versionList =
                        NugetHelpers.GetNugetPackageVersionInformation(_logger, directorioPaquete.First());
                    var versionSeleccionada =
                        NugetHelpers.SelectedVersion(_settings.FrameworkPriorities, version, frameworkSolicitado,
                            versionList, true, exactFramework);

                    if (versionSeleccionada != null) return versionSeleccionada.DllPath;
                }
            }
            else
            {
                var versionList = NugetHelpers.GetNugetPackageVersionInformation(_logger, directorioPaquete.First());
                var versionSeleccionada =
                    NugetHelpers.SelectedVersion(_settings.FrameworkPriorities, version, frameworkSolicitado,
                        versionList, false, exactFramework);

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

                    foreach (var mapping in mappings)
                    {
                        var assemblyName = mapping.Key;
                        var packageName = mapping.Value;

                        if (string.IsNullOrWhiteSpace(assemblyName) || string.IsNullOrWhiteSpace(packageName))
                        {
                            _logger.LogWarning(
                                "Se ha ignorado un mapeo inválido en {TypeName}.{FieldName}: AssemblyName='{AssemblyName}', PackageName='{PackageName}'.",
                                type.FullName,
                                field.Name,
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
                                    assembly.FullName);

                                continue;
                            }

                            _assemblyToPackageMap[assemblyName] = packageName;

                            _logger.LogInformation(
                                "Añadido mapeo de ensamblado {AssemblyName} a paquete {PackageName} desde {MappingAssemblyName}.",
                                assemblyName,
                                packageName,
                                assembly.FullName);
                        }
                    }
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

    private bool HasAssemblyMappingDefinition(FieldInfo field)
    {
        try
        {
            return field
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
                "No se pudieron leer los atributos del campo {FieldName}.",
                field.Name);

            return false;
        }
    }
}
