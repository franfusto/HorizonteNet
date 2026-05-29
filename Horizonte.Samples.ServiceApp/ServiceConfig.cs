namespace Horizonte.Samples.ServiceApp;

/// <summary>
/// Configuración para un servicio específico dentro de la aplicación.
/// </summary>
/// <remarks>
/// Esta clase se encarga de proporcionar las propiedades de configuración
/// necesarias para el funcionamiento de un servicio, incluyendo la URL
/// base del servicio y la activación de la documentación Swagger.
/// </remarks>
public class ServiceConfig
{
    /// Propiedad que representa la URL base del servicio utilizado por la aplicación.
    /// Esta URL es utilizada para configurar y ejecutar la aplicación web,
    /// permitiendo que la aplicación escuche en el host y puerto específicos definidos.
    /// El valor por defecto asignado a esta propiedad es "http://localhost:5003",
    /// pero puede ser configurado a otro valor según las necesidades del entorno de despliegue.
    /// /
    public string Url { get; set; } = "http://localhost:5003";

    /// Propiedad que indica si la generación de documentación Swagger está habilitada o no en la aplicación.
    /// Al establecer esta propiedad en verdadero (`true`), el servicio configura automáticamente middleware
    /// de Swagger, permitiendo la exploración y documentación de los endpoints de la API a través de la interfaz Swagger.
    /// Esto es útil para desarrolladores que necesitan una forma fácil de entender y consumir los servicios expuestos.
    /// Cuando se establece en falso (`false`), la funcionalidad de Swagger estará deshabilitada, evitando así la carga
    /// de las herramientas relacionadas con Swagger durante la ejecución del servicio.
    /// /
    public bool EnableSwagger { get; set; } = true;
}