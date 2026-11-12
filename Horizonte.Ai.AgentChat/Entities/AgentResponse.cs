using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Horizonte.Ai.AgentChat.Entities;

/// <summary>
/// Representa una respuesta formateada del agente.
/// Utilizada para estructurar y contener datos de salida del sistema.
/// </summary>
public class AgentChatResponse
{
    /// <summary>
    /// Obtiene o establece los datos estructurados generados en la respuesta del agente.
    /// </summary>
    [JsonPropertyName("Output")]
    public Output Output { get; set; } = new Output();
}

/// <summary>
/// Representa los datos de salida estructurados del agente.
/// Incluye información sobre la respuesta generada, el widget gráfico asociado y cualquier posible error.
/// </summary>
public class Output
{
    /// <summary>
    /// Obtiene o establece el texto de respuesta dirigido al usuario.
    /// </summary>
    [JsonPropertyName("Response")]
    [Description("Tu respuesta para el usuario.")]
    public string Response { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el comando generador del widget para interactuar en la interfaz gráfica.
    /// </summary>
    [JsonPropertyName("Widget")]
    [Description("El nombre del Comando que genera en la interface gráfica el widget solicitado para la interacción con el usuario.")]
    public string Widget { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el mensaje de error en caso de fallo durante el procesamiento del agente.
    /// </summary>
    [JsonPropertyName("Error")]
    [Description("Mensaje de error si ocurre alguno.")]
    public string Error { get; set; } = string.Empty;
}