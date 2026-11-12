using Microsoft.AspNetCore.Components;

namespace Horizonte.Ai.AgentChat.Entities;

/// <summary>
/// Representa un elemento de chat utilizado en el contexto de interacción con un agente.
/// Permite configurar el texto del mensaje, su orientación, ícono asociado y otros
/// argumentos relacionados con la ejecución o representación.
/// Incluye un <see cref="RenderFragment"/> para visualizar widgets interactivos en la interfaz.
/// </summary>
public class ChatElement
{
    /// <summary>
    /// Obtiene o establece el contenido textual del mensaje de chat.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre del icono representativo del mensaje o emisor.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Obtiene o establece un valor que indica si el mensaje se alinea a la izquierda (usuario) o a la derecha (agente).
    /// </summary>
    public bool OrientationLeft { get; set; } = true;

    /// <summary>
    /// Obtiene o establece los argumentos o parámetros para la ejecución de acciones asociadas al mensaje.
    /// </summary>
    public Dictionary<string, object>? ExecuteArgs { get; set; }

    /// <summary>
    /// Obtiene o establece el fragmento de renderizado Razor opcional para incrustar widgets interactivos en el mensaje.
    /// </summary>
    public RenderFragment? RenderFragment { get; set; }
}