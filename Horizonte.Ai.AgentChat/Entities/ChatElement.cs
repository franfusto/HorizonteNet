using Microsoft.AspNetCore.Components;

namespace Horizonte.Ai.AgentChat.Entities;

/// <summary>
/// Representa un elemento de chat utilizado en el contexto de un agente.
/// Permite configurar el texto del mensaje, su orientación, ícono asociado y otros
/// argumentos relacionados con la ejecución o representación.
/// Incluye un RenderFragment para visualizar widgets que queremos mostrar al usuario.
/// </summary>
public class ChatElement
{
    public string Text { get; set; } = string.Empty;
    public string? IconName  { get; set; }
    public bool OrientationLeft  { get; set; } = true;
    public Dictionary<string, object>? ExecuteArgs  { get; set; }
    public RenderFragment? RenderFragment  { get; set; }
}