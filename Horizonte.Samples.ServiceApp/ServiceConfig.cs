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
    /// <summary>
    /// Obtiene o establece la URL base utilizada por el servicio web de la aplicación.
    /// </summary>
    /// <remarks>
    /// Esta URL es utilizada para configurar el servidor web, permitiendo que la aplicación escuche
    /// en el host y puerto especificados (por defecto <c>"http://localhost:5003"</c>).
    /// </remarks>
    public string Url { get; set; } = "http://localhost:5003";

    /// <summary>
    /// Obtiene o establece un valor que indica si la generación y visualización de la interfaz Swagger/OpenAPI está habilitada.
    /// </summary>
    /// <remarks>
    /// Al establecer esta propiedad en <c>true</c>, el servicio configura automáticamente el middleware de Swagger,
    /// permitiendo la exploración y documentación de los endpoints de la API. Cuando se establece en <c>false</c>,
    /// la funcionalidad de Swagger permanece deshabilitada.
    /// </remarks>
    public bool EnableSwagger { get; set; } = true;
}