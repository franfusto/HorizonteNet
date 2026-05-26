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
    /// Obtiene información de las versiones de paquetes NuGet presentes en un directorio específico.
    /// </summary>
    /// <param name="logger">El logger utilizado para registrar mensajes de error en caso de excepción.</param>
    /// <param name="packageDirectory">El directorio donde se buscarán los archivos .dll de los paquetes NuGet.</param>
    /// <return>Una lista de objetos <see cref="NugetPackageVersionInformation"/> que contiene detalles sobre las versiones de los paquetes NuGet encontrados.</return>
    public static List<NugetPackageVersionInformation> GetNugetPackageVersionInformation(
        ILogger logger,
        string packageDirectory)
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
            logger.LogError(e, "Error obteniendo información de versiones NuGet.");
        }

        return result;
    }
}