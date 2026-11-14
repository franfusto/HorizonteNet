namespace Horizonte.Entities;

/// <summary>
/// Catálogo unificado de orígenes y versiones descubiertas para un paquete NuGet.
/// </summary>
public class VersionResources
{
    /// <summary>
    /// Identificador del paquete NuGet.
    /// </summary>
    public string PackageId { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora UTC en la que se recuperó la información.
    /// </summary>
    public DateTime RetrievedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Lista de orígenes de paquetes consultados con sus respectivas versiones.
    /// </summary>
    public List<PackageSourceResource> Sources { get; set; } = new();
}
