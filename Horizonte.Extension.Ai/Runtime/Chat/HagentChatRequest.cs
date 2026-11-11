namespace Horizonte.Extension.Ai.Runtime.Chat;

/// <summary>
/// Representa una solicitud de chat enviada al cliente de inferencia de un agente.
/// </summary>
public sealed class HagentChatRequest
{
    /// <summary>
    /// Obtiene la lista de mensajes cronológicos que conforman el historial y contexto de la conversación (<see cref="HagentMessage"/>).
    /// </summary>
    public List<HagentMessage> Messages { get; init; } = [];

    /// <summary>
    /// Obtiene la lista de descriptores de herramientas disponibles (<see cref="HagentToolDescriptor"/>) para function calling.
    /// </summary>
    public List<HagentToolDescriptor> Tools { get; init; } = [];

    /// <summary>
    /// Obtiene el diccionario de opciones adicionales pasadas al motor de inferencia.
    /// </summary>
    public Dictionary<string, object?> Options { get; init; } = new();
}
