namespace Horizonte.Entities;

/// <summary>
/// Origen de paquetes (directorio local o servidor NuGet remoto) y sus versiones disponibles.
/// </summary>
/// <param name="SourceName">Nombre o identificador del origen.</param>
/// <param name="IsLocal">Indica si el origen es una carpeta local o un servidor remoto.</param>
/// <param name="Versions">Lista de versiones descubiertas en dicho origen.</param>
public sealed record PackageSourceResource(
    string SourceName,
    bool IsLocal,
    IReadOnlyList<PackageVersionInfo> Versions);
