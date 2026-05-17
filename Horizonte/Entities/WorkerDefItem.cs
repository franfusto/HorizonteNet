namespace Horizonte.Entities;


/// <summary>
/// Represents an individual worker configuration item within the system.
/// </summary>
public class WorkerDefItem
{
    /// <summary>
    /// Especifica el tipo de trabajador que será instanciado y ejecutado en el marco de trabajo Horizonte.
    /// Esta propiedad contiene un nombre de tipo completamente calificado en el formato "Namespace.Class,Assembly",
    /// el cual se utiliza para cargar dinámicamente y crear una instancia del trabajador en tiempo de ejecución.
    /// </summary>
    public string WorkerType { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el trabajador debería iniciarse automáticamente cuando se inicializa la aplicación.
    /// </summary>
    /// <remarks>
    /// Si se establece en true, la instancia del trabajador será registrada e iniciada automáticamente durante
    /// la inicialización de la aplicación. Si se establece en false, el trabajador no se iniciará automáticamente
    /// y deberá ser iniciado manualmente.
    /// </remarks>
    public bool RunOnStart { get; set; } = false;

    /// <summary>
    /// Obtiene o establece el nombre del servicio asociado al trabajador.
    /// Se utiliza para identificar únicamente la configuración del trabajador dentro de la aplicación.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Representa el orden de ejecución para los servicios del trabajador.
    /// Determina la secuencia en la cual se iniciaran y ejecutarán los trabajadores.
    /// </summary>
    public int Order { get; set; } = 99999;
}