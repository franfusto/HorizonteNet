using Horizonte.Extension.Ai.Definitions;

namespace Horizonte.Extension.Ai.Contracts;

/// <summary>
/// Representa un elemento individual dentro de la secuencia de respuesta de un agente de IA.
/// </summary>
public class ChatResponseItem
{
    /// <summary>
    /// Obtiene o establece el tipo de elemento de respuesta (<see cref="ChatResponseItemType"/>), como traza, herramienta o texto final.
    /// </summary>
    public ChatResponseItemType Type { get; set; } = ChatResponseItemType.Trace;

    /// <summary>
    /// Obtiene o establece el contenido textual o mensaje del elemento de respuesta.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
