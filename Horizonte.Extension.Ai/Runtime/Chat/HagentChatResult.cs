using Horizonte.Extension.Ai.Runtime.Tools;

namespace Horizonte.Extension.Ai.Runtime.Chat;

/// <summary>
/// Representa el resultado obtenido de una llamada de inferencia de chat.
/// </summary>
public sealed class HagentChatResult
{
    /// <summary>
    /// Obtiene el texto generado en la respuesta del modelo de lenguaje (si existe).
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Obtiene la lista de llamadas a herramientas solicitadas por el modelo (<see cref="HagentToolCall"/>).
    /// </summary>
    public List<HagentToolCall> ToolCalls { get; init; } = [];

    /// <summary>
    /// Obtiene el diccionario de metadatos devueltos por el proveedor (tokens, modelo, tiempos, etc.).
    /// </summary>
    public Dictionary<string, object?> Metadata { get; init; } = new();
}