namespace Horizonte.Extension.Ai.Definitions;

/// <summary>
/// Define la configuración de conexión y autenticación a un servidor o proveedor de servicios de IA.
/// </summary>
public class ServerDef
{
    /// <summary>
    /// Obtiene o establece el identificador único del servidor.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre descriptivo del servidor.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la URL base del endpoint de la API.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la clave de API o token de autenticación.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el proveedor de IA (ej. "openai", "azure", "anthropic", "ollama").
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la versión de la API utilizada por el proveedor.
    /// </summary>
    public string ApiVersion { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el tiempo límite de espera en segundos para las peticiones HTTP.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Obtiene o establece encabezados HTTP personalizados adicionales a incluir en las solicitudes.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// Obtiene o establece opciones de configuración adicionales para el servidor.
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = new();
}











