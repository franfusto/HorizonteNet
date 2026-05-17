namespace Horizonte.Entities;

/// <summary>
/// Representa la definición de configuración de trabajadores del sistema.
/// </summary>
/// <remarks>
/// Esta clase actúa como contenedor de una colección de elementos <see cref="WorkerDefItem"/>,
/// donde cada elemento describe un trabajador individual junto con su información de inicialización,
/// como el tipo, el nombre del servicio, el orden de ejecución y si debe iniciarse automáticamente.
/// </remarks>
public class WorkerDef
{
    /// <summary>
    /// Obtiene o establece la colección de definiciones de trabajadores.
    /// </summary>
    /// <remarks>
    /// Cada elemento de esta lista representa la configuración de un trabajador que puede ser
    /// registrado y procesado por la infraestructura de ejecución de la aplicación.
    /// </remarks>
    public List<WorkerDefItem> List { get; set; } = [];
}
