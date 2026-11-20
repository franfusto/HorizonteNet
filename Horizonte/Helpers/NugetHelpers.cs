using System.Runtime.InteropServices;
using Horizonte.Settings;
using Microsoft.Extensions.Logging;

namespace Horizonte.Helpers;

/// <summary>
/// La clase <c>NugetHelpers</c> proporciona métodos auxiliares para la gestión de paquetes NuGet.
/// Incluye funcionalidades para obtener URLs de descarga, seleccionar versiones específicas compatibles
/// y obtener información de versiones de paquetes NuGet.
/// </summary>
public static class NugetHelpers
{
    /// <summary>
    /// Genera y retorna la URL de descarga para un paquete NuGet específico, basado en la versión del servidor.
    /// </summary>
    /// <param name="server">El servidor NuGet que provee la información necesaria para formatear la URL.</param>
    /// <param name="packageName">El nombre del paquete cuyo URL de descarga se desea obtener.</param>
    /// <param name="version">La versión específica del paquete para la cual se debe construir la URL de descarga.</param>
    /// <returns>Un string que representa la URL directamente formateada para descargar el paquete solicitado. Retorna una cadena vacía si la versión del servidor no es compatible.</returns>
    public static string GetDownloadUrl(NugetServerItem server, string packageName, string version)
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

    /// <summary>
    /// Selecciona la mejor versión del paquete Nuget de acuerdo con las prioridades de frameworks y versiones solicitadas.
    /// </summary>
    /// <param name="frameworkspriorities">Lista de prioridades de frameworks a considerar.</param>
    /// <param name="versionSolicitadaRaw">Versión del paquete solicitada en formato de cadena.</param>
    /// <param name="frameworkSolicitado">Framework específico solicitado.</param>
    /// <param name="versionesDisponibles">Lista de versiones de paquetes disponibles para seleccionar.</param>
    /// <param name="exactmatch">Indica si se debe hacer una coincidencia exacta con la versión solicitada.</param>
    /// <param name="exactFramework">Indica si se debe usar exclusivamente el framework solicitado, sin considerar otras prioridades.</param>
    /// <returns>
    /// Un objeto de tipo <c>NugetPackageVersionInformation</c> que representa la versión seleccionada del paquete,
    /// o <c>null</c> si no se encuentra ninguna versión apropiada.
    /// </returns>
    public static NugetPackageVersionInformation? SelectedVersion(
        List<string> frameworkspriorities,
        string versionSolicitadaRaw,
        string frameworkSolicitado,
        List<NugetPackageVersionInformation> versionesDisponibles,
        bool exactmatch,
        bool exactFramework = false)
    {
        var prioridadFrameworks = new List<string> { frameworkSolicitado };

        if (!exactFramework)
        {
            prioridadFrameworks.AddRange(frameworkspriorities);
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

            var nativeExact = versionesDisponibles.FirstOrDefault(info =>
                info.IsRuntimePackage &&
                info.VersionString.Equals(versionSolicitadaRaw, StringComparison.OrdinalIgnoreCase));

            return nativeExact;
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

        var nativeCandidates = versionesDisponibles.Where(info => info.IsRuntimePackage).ToList();
        if (nativeCandidates.Any())
        {
            var match = nativeCandidates.FirstOrDefault(info =>
                info.Version.Major == vS.Major &&
                info.Version.Minor == vS.Minor &&
                info.Version.Build == vS.Build);

            if (match != null) return match;

            match = nativeCandidates
                .Where(info => info.Version.Major == vS.Major && info.Version.Minor == vS.Minor)
                .OrderBy(info => info.Version)
                .FirstOrDefault(info => info.Version >= vS);

            if (match != null) return match;

            match = nativeCandidates
                .Where(info => info.Version.Major == vS.Major)
                .OrderBy(info => info.Version)
                .FirstOrDefault(info => info.Version >= vS);

            if (match != null) return match;

            return nativeCandidates.OrderByDescending(info => info.Version).First();
        }

        return null;
    }

    /// <summary>
    /// Selecciona la mejor versión remota disponible de un paquete NuGet a partir de una versión solicitada y una lista de versiones disponibles.
    /// </summary>
    /// <param name="versionSolicitadaRaw">
    /// La versión original solicitada como una cadena de texto que se comparará con las versiones disponibles.
    /// </param>
    /// <param name="versionesDisponibles">
    /// Una lista de cadenas que representan las versiones disponibles del paquete remoto.
    /// </param>
    /// <returns>
    /// La versión seleccionada de la lista que mejor se corresponde con la versión solicitada, o <c>null</c> si no se encuentra una coincidencia adecuada.
    /// </returns>
    public static string? SelectBestRemoteVersion(string versionSolicitadaRaw, List<string> versionesDisponibles)
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

    /// <summary>
    /// Obtiene el identificador de runtime (RID) efectivo y normalizado para el entorno actual.
    /// </summary>
    /// <returns>Identificador de runtime normalizado (ej. "linux-x64", "win-x64", "osx-arm64").</returns>
    public static string GetEffectiveRuntimeIdentifier()
    {
        var rid = RuntimeInformation.RuntimeIdentifier;
        return NormalizeRuntimeIdentifier(rid);
    }

    /// <summary>
    /// Normaliza un RID de distribución específica (ej. ubuntu.22.04-x64) a los estándares comunes de NuGet (ej. linux-x64).
    /// </summary>
    /// <param name="rid">El identificador de runtime original o null.</param>
    /// <returns>RID normalizado estándar.</returns>
    public static string NormalizeRuntimeIdentifier(string? rid)
    {
        if (string.IsNullOrWhiteSpace(rid))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return RuntimeInformation.OSArchitecture switch
                {
                    Architecture.Arm64 => "linux-arm64",
                    Architecture.Arm => "linux-arm",
                    _ => "linux-x64"
                };
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return RuntimeInformation.OSArchitecture switch
                {
                    Architecture.Arm64 => "win-arm64",
                    Architecture.X86 => "win-x86",
                    _ => "win-x64"
                };
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return RuntimeInformation.OSArchitecture switch
                {
                    Architecture.Arm64 => "osx-arm64",
                    _ => "osx-x64"
                };
            }
            return "linux-x64";
        }

        if (rid.StartsWith("linux-", StringComparison.OrdinalIgnoreCase) ||
            rid.StartsWith("win-", StringComparison.OrdinalIgnoreCase) ||
            rid.StartsWith("osx-", StringComparison.OrdinalIgnoreCase))
        {
            return rid.ToLowerInvariant();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.OSArchitecture switch
            {
                Architecture.Arm64 => "linux-arm64",
                Architecture.Arm => "linux-arm",
                _ => "linux-x64"
            };
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return RuntimeInformation.OSArchitecture switch
            {
                Architecture.Arm64 => "win-arm64",
                Architecture.X86 => "win-x86",
                _ => "win-x64"
            };
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RuntimeInformation.OSArchitecture switch
            {
                Architecture.Arm64 => "osx-arm64",
                _ => "osx-x64"
            };
        }

        return rid.ToLowerInvariant();
    }

    /// <summary>
    /// Obtiene la lista ordenada de identificadores de runtime compatibles con la plataforma actual.
    /// </summary>
    /// <param name="baseRid">RID base opcional. Si no se especifica, se utiliza el RID efectivo actual.</param>
    /// <returns>Lista de RIDs compatibles ordenados de mayor a menor especificidad.</returns>
    public static List<string> GetCompatibleRuntimeIdentifiers(string? baseRid = null)
    {
        var rid = string.IsNullOrWhiteSpace(baseRid) ? GetEffectiveRuntimeIdentifier() : NormalizeRuntimeIdentifier(baseRid);
        var rids = new List<string> { rid };

        if (rid.StartsWith("linux-", StringComparison.OrdinalIgnoreCase))
        {
            if (!rids.Contains("linux", StringComparer.OrdinalIgnoreCase))
                rids.Add("linux");
            if (!rids.Contains("unix", StringComparer.OrdinalIgnoreCase))
                rids.Add("unix");
        }
        else if (rid.StartsWith("win-", StringComparison.OrdinalIgnoreCase))
        {
            if (!rids.Contains("win", StringComparer.OrdinalIgnoreCase))
                rids.Add("win");
        }
        else if (rid.StartsWith("osx-", StringComparison.OrdinalIgnoreCase))
        {
            if (!rids.Contains("osx", StringComparer.OrdinalIgnoreCase))
                rids.Add("osx");
            if (!rids.Contains("unix", StringComparer.OrdinalIgnoreCase))
                rids.Add("unix");
        }

        if (!rids.Contains("any", StringComparer.OrdinalIgnoreCase))
            rids.Add("any");

        return rids;
    }

    /// <summary>
    /// Determina si un directorio de versión de paquete contiene activos nativos bajo /runtimes/{rid}/native/ o /runtimes/{rid}/.
    /// </summary>
    /// <param name="packageVersionDir">Ruta al directorio de la versión del paquete.</param>
    /// <param name="rid">Identificador de runtime opcional a evaluar.</param>
    /// <returns>True si existen archivos nativos para el RID compatible; de lo contrario, false.</returns>
    public static bool HasNativeRuntimeAssets(string packageVersionDir, string? rid = null)
    {
        if (string.IsNullOrWhiteSpace(packageVersionDir) || !Directory.Exists(packageVersionDir))
            return false;

        var compatibleRids = GetCompatibleRuntimeIdentifiers(rid);
        var runtimesDir = Path.Combine(packageVersionDir, "runtimes");
        if (!Directory.Exists(runtimesDir))
            return false;

        foreach (var cRid in compatibleRids)
        {
            var nativeDir = Path.Combine(runtimesDir, cRid, "native");
            if (Directory.Exists(nativeDir) && Directory.EnumerateFiles(nativeDir, "*", SearchOption.AllDirectories).Any())
            {
                return true;
            }

            var ridDir = Path.Combine(runtimesDir, cRid);
            if (Directory.Exists(ridDir) && Directory.EnumerateFiles(ridDir, "*", SearchOption.TopDirectoryOnly).Any())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Obtiene la lista de archivos nativos individuales correspondientes al RID compatible en el paquete.
    /// </summary>
    /// <param name="packageVersionDir">Ruta al directorio de la versión del paquete.</param>
    /// <param name="rid">Identificador de runtime opcional.</param>
    /// <returns>Lista de rutas absolutas de archivos nativos encontrados.</returns>
    public static List<string> GetNativeRuntimeFiles(string packageVersionDir, string? rid = null)
    {
        var files = new List<string>();
        if (string.IsNullOrWhiteSpace(packageVersionDir) || !Directory.Exists(packageVersionDir))
            return files;

        var runtimesDir = Path.Combine(packageVersionDir, "runtimes");
        if (!Directory.Exists(runtimesDir))
            return files;

        var compatibleRids = GetCompatibleRuntimeIdentifiers(rid);
        foreach (var cRid in compatibleRids)
        {
            var nativeDir = Path.Combine(runtimesDir, cRid, "native");
            if (Directory.Exists(nativeDir))
            {
                var nativeFiles = Directory.GetFiles(nativeDir, "*", SearchOption.AllDirectories);
                if (nativeFiles.Length > 0)
                {
                    files.AddRange(nativeFiles);
                    return files;
                }
            }

            var ridDir = Path.Combine(runtimesDir, cRid);
            if (Directory.Exists(ridDir))
            {
                var ridFiles = Directory.GetFiles(ridDir, "*", SearchOption.TopDirectoryOnly);
                if (ridFiles.Length > 0)
                {
                    files.AddRange(ridFiles);
                    return files;
                }
            }
        }

        return files;
    }

    /// <summary>
    /// Obtiene información de las versiones de paquetes NuGet presentes en un directorio específico.
    /// </summary>
    /// <param name="logger">El logger utilizado para registrar mensajes de error en caso de excepción.</param>
    /// <param name="packageDirectory">El directorio donde se buscarán los archivos .dll o activos de runtime de los paquetes NuGet.</param>
    /// <return>Una lista de objetos <see cref="NugetPackageVersionInformation"/> que contiene detalles sobre las versiones de los paquetes NuGet encontrados.</return>
    public static List<NugetPackageVersionInformation> GetNugetPackageVersionInformation(
        ILogger logger,
        string packageDirectory)
    {
        var result = new List<NugetPackageVersionInformation>();

        try
        {
            if (!Directory.Exists(packageDirectory))
                return result;

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
                    var frameworkDir = Directory.GetParent(dllPath);
                    var libDir = frameworkDir?.Parent;
                    var versionDir = libDir?.Parent?.FullName;
                    var hasNative = !string.IsNullOrEmpty(versionDir) && HasNativeRuntimeAssets(versionDir);

                    result.Add(new NugetPackageVersionInformation
                    {
                        PackageId = packageId,
                        VersionString = versionRaw,
                        Version = parsedVersion,
                        Framework = framework,
                        DllPath = dllPath,
                        IsRuntimePackage = false,
                        HasNativeAssets = hasNative,
                        PackageRootDirectory = versionDir
                    });
                }
            }

            // Descubrir paquetes puramente nativos (sin /lib/) presentes en el directorio
            var directRuntimes = Path.Combine(packageDirectory, "runtimes");
            if (Directory.Exists(directRuntimes) && HasNativeRuntimeAssets(packageDirectory))
            {
                if (!result.Any(r => string.Equals(r.PackageRootDirectory, packageDirectory, StringComparison.OrdinalIgnoreCase)))
                {
                    var pathSegments = packageDirectory.TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
                    if (pathSegments.Length >= 2)
                    {
                        var packageId = pathSegments[^2];
                        var versionRaw = pathSegments[^1];
                        string versionForParsing = versionRaw.Contains('-') ? versionRaw.Split('-')[0] : versionRaw;
                        if (Version.TryParse(versionForParsing, out var parsedVersion))
                        {
                            result.Add(new NugetPackageVersionInformation
                            {
                                PackageId = packageId,
                                VersionString = versionRaw,
                                Version = parsedVersion,
                                Framework = "native",
                                DllPath = string.Empty,
                                IsRuntimePackage = true,
                                HasNativeAssets = true,
                                PackageRootDirectory = packageDirectory
                            });
                        }
                    }
                }
            }

            foreach (var subDir in Directory.EnumerateDirectories(packageDirectory))
            {
                if (result.Any(r => string.Equals(r.PackageRootDirectory, subDir, StringComparison.OrdinalIgnoreCase)))
                    continue;

                if (HasNativeRuntimeAssets(subDir))
                {
                    var packageId = Path.GetFileName(packageDirectory);
                    var versionRaw = Path.GetFileName(subDir);
                    string versionForParsing = versionRaw.Contains('-') ? versionRaw.Split('-')[0] : versionRaw;
                    if (Version.TryParse(versionForParsing, out var parsedVersion))
                    {
                        result.Add(new NugetPackageVersionInformation
                        {
                            PackageId = packageId,
                            VersionString = versionRaw,
                            Version = parsedVersion,
                            Framework = "native",
                            DllPath = string.Empty,
                            IsRuntimePackage = true,
                            HasNativeAssets = true,
                            PackageRootDirectory = subDir
                        });
                    }
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error obteniendo información de versiones NuGet.");
        }

        return result;
    }
}