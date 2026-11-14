using System.Reflection;

namespace Horizonte.Entities;

/// <summary>
/// Representa un elemento persistido o consultado en la caché de resolución de ensamblados.
/// </summary>
public class AssemblyCacheItem
{
    /// <summary>
    /// Nombre simple del ensamblado.
    /// </summary>
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Versión del ensamblado solicitada o resuelta.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del dominio o ALC de destino.
    /// </summary>
    public string DomainName { get; set; } = "Default";

    /// <summary>
    /// Identificador del paquete NuGet asociado.
    /// </summary>
    public string? PackageId { get; set; }

    /// <summary>
    /// Versión del paquete NuGet resuelta.
    /// </summary>
    public string? PackageVersion { get; set; }

    /// <summary>
    /// Ruta absoluta a la librería DLL en disco.
    /// </summary>
    public string? DllPath { get; set; }

    /// <summary>
    /// Instancia del ensamblado cargado en memoria, si aplica.
    /// </summary>
    public Assembly? ResolvedAssembly { get; set; }

    /// <summary>
    /// Indica si el proceso de resolución fue completado (éxito o fallo).
    /// </summary>
    public bool IsResolved { get; set; }

    /// <summary>
    /// Indica si el ensamblado fue encontrado exitosamente (false indica caché negativa).
    /// </summary>
    public bool Found { get; set; }

    /// <summary>
    /// Recursos de versión y orígenes descubiertos para el paquete correspondiente.
    /// </summary>
    public VersionResources? VersionResources { get; set; }

    /// <summary>
    /// Marca de tiempo UTC en la que se resolvió o registró la entrada.
    /// </summary>
    public DateTime ResolvedAtUtc { get; set; } = DateTime.UtcNow;
}
