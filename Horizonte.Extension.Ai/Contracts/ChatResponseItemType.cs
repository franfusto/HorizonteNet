namespace Horizonte.Extension.Ai.Contracts;

/// <summary>
/// Especifica la categoría o tipo de un elemento en la respuesta de un agente de IA.
/// </summary>
public enum ChatResponseItemType
{
    /// <summary>
    /// Información o paso de depuración y razonamiento interno del agente.
    /// </summary>
    Trace,

    /// <summary>
    /// Notificación de invocación o resultado de una herramienta ejecutada por el agente.
    /// </summary>
    Tool,

    /// <summary>
    /// Mensaje de respuesta final destinado al usuario.
    /// </summary>
    Response
}