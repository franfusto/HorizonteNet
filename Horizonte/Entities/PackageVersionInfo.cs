using NuGet.Versioning;

namespace Horizonte.Entities;

/// <summary>
/// Información de una versión individual de un paquete en un origen específico.
/// </summary>
/// <param name="Version">Versión semántica normalizada.</param>
/// <param name="IsRequestedVersion">Indica si coincide con la versión solicitada.</param>
/// <param name="LocalPath">Ruta local al archivo .nupkg o paquete si existe en disco.</param>
public sealed record PackageVersionInfo(
    NuGetVersion Version,
    bool IsRequestedVersion,
    string? LocalPath);
