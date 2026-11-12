namespace Horizonte;

/// <summary>
/// Representa la configuración completa de un nuevo módulo en el sistema, incluyendo sus rutas binarias y carpetas raíz.
/// </summary>
public class NewModuleConfig
{
    /// <summary>
    /// Propiedad que representa el nombre del módulo en la configuración.
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// Propiedad que obtiene o establece la versión del módulo.
    /// </summary>
    public string ModuleVersion { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la ruta al directorio de binarios ejecutables del módulo.
    /// </summary>
    public string BinPath { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene una colección de las carpetas raíz asociadas con una entidad o proceso específico.
    /// </summary>
    /// <remarks>
    /// Las carpetas raíz son los directorios principales desde los cuales se inician operaciones como
    /// la exploración, búsqueda o gestión de archivos y carpetas.
    /// Esta propiedad es útil para aplicaciones que requieren gestionar múltiples sistemas de archivos
    /// o contextos de almacenamiento.
    /// </remarks>
    /// <value>
    /// Una colección que contiene las carpetas raíz.
    /// </value>
    public List<string> RootFolders { get; set; } = [];
}