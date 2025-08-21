using Microsoft.AspNetCore.Components;

namespace Horizonte.Ai.Agent;

/// <summary>
/// Representa un elemento de chat utilizado en el contexto de un agente.
/// Permite configurar el texto del mensaje, su orientación, ícono asociado y otros
/// argumentos relacionados con la ejecución o representación.
/// Incluye un RenderFragment para visualizar widgets que queremos mostrar al usuario.
/// </summary>
public class ChatElement
{
    public string Text = string.Empty;
    public string? IconName;
    public bool OrientationLeft = true;
    public Dictionary<string, object>? ExecuteArgs;
    public RenderFragment? RenderFragment;
}