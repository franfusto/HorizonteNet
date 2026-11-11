namespace Horizonte.Extension.Ai.Runtime.Chat;

/// <summary>
/// Representa un mensaje individual dentro de la conversación de chat de un agente.
/// </summary>
public sealed class HagentMessage
{
    /// <summary>
    /// Obtiene el rol del emisor del mensaje (ej. "system", "user", "assistant", "context", "tool").
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene el contenido textual del mensaje.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene los metadatos asociados al mensaje.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; init; } = new();
}