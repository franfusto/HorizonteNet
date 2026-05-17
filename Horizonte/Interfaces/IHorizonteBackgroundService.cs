namespace Horizonte;

/// <summary>
/// Representa una interfaz de servicio genérica que proporciona las propiedades esenciales 
/// para gestionar el estado y el comportamiento de un servicio.
/// </summary>
public interface IHorizonteBackgroundServiceXXX
{
    /// <summary>
    /// Representa el nombre del servicio. Esta propiedad se utiliza para identificar 
    /// de manera única a un servicio en segundo plano que implementa la interfaz <see cref="IHorizonteBackgroundService"/>.
    /// </summary>
    /// <remarks>
    /// El valor de esta propiedad puede ser establecido durante la inicialización del servicio 
    /// y puede contener un nombre contextualmente significativo para fines de monitoreo o configuración.
    /// Aplicable en múltiples implementaciones de <see cref="IHorizonteBackgroundService"/>.
    /// </remarks>
    public string ServiceName { get; set; }

    /// <summary>
    /// Indica si el servicio se encuentra actualmente en ejecución o no.
    /// </summary>
    /// <remarks>
    /// Esta propiedad refleja el estado de ejecución del servicio.  
    /// Puede ser utilizada para monitorear el estado operativo de los servicios que implementan la interfaz IHservice.
    /// </remarks>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Determina si el servicio debería iniciarse automáticamente cuando se inicializa la aplicación.
    /// Si se establece en true, el servicio será iniciado al arrancar la aplicación; de lo contrario, permanecerá inactivo hasta que se inicie explícitamente.
    /// </summary>
    public bool RunOnStart { get; set; } 
}