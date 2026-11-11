namespace Horizonte.Extension.AiWorkFlows;

/// <summary>
/// Define los parámetros de conexión y autenticación para un cliente de servicio de IA.
/// </summary>
public class AiClientDef
{
    /// <summary>
    /// Obtiene o establece el nombre de clave de credencial o URI del endpoint del servicio de IA.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre de clave de credencial para la API Key del servicio de IA.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}