namespace Horizonte.AdminUi;

/// <summary>
/// Representa la configuración de servicios personalizados registrados dinámicamente en el contenedor de dependencias de Horizonte AdminUI.
/// </summary>
public class CustomServicesConfig
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CustomServicesConfig"/>.
    /// </summary>
    public CustomServicesConfig()
    {
    }

    /// <summary>
    /// Obtiene o establece la lista de configuraciones de servicios individuales (<see cref="CustomServiceConfig"/>).
    /// </summary>
    public List<CustomServiceConfig> Services { get; set; } = new();
}

/// <summary>
/// Define la configuración de registro para un servicio personalizado individual.
/// </summary>
public class CustomServiceConfig
{
    /// <summary>
    /// Obtiene o establece el nombre calificado del tipo de servicio o interfaz.
    /// </summary>
    public string ServiceType { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre calificado del tipo de implementación del servicio.
    /// </summary>
    public string ImplementationType { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el tiempo de vida del servicio (<see cref="CustomServiceLifetime"/>) en el contenedor de inyección de dependencias.
    /// </summary>
    public CustomServiceLifetime Lifetime { get; set; } = CustomServiceLifetime.Scoped;
}

/// <summary>
/// Especifica el ciclo de vida de un servicio personalizado en el contenedor de inyección de dependencias.
/// </summary>
public enum CustomServiceLifetime
{
    /// <summary>
    /// Se crea una única instancia del servicio para toda la vida de la aplicación.
    /// </summary>
    Singleton,

    /// <summary>
    /// Se crea una instancia del servicio por cada ámbito (scope) o conexión de cliente.
    /// </summary>
    Scoped,

    /// <summary>
    /// Se crea una nueva instancia del servicio cada vez que es solicitada.
    /// </summary>
    Transient
}