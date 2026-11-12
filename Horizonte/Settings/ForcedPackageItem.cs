namespace Horizonte.Settings;

/// <summary>
/// Representa un paquete NuGet cuya inclusión y carga es obligatoria en el entorno de ensamblados de Horizonte.
/// </summary>
/// <remarks>
/// Permite especificar paquetes y versiones críticas que deben cargarse en un dominio determinado.
/// </remarks>
public class ForcedPackageItem
{
    /// <summary>
    /// Obtiene o establece si el paquete forzado está activo y debe ser procesado por el sistema.
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>
    /// Propiedad que representa el identificador único de un paquete forzado.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se utiliza para gestionar paquetes en el sistema, permitiendo
    /// identificar específicamente qué paquete debe ser cargado o manipulado.
    /// Se espera que cada paquete tenga un identificador único que distinga
    /// un paquete de otro dentro de las aplicaciones que utilizan esta funcionalidad.
    /// </remarks>
    public string PackageId { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la versión específica del paquete NuGet requerida.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el framework de destino compatible (ej. "net10.0").
    /// </summary>
    public string Framework { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre del dominio de ensamblados en el cual cargar este paquete (por defecto "Default").
    /// </summary>
    public string Domain { get; set; } = "Default";
}