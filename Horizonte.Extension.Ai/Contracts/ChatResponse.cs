namespace Horizonte.Extension.Ai.Contracts;

/// <summary>
/// Representa la respuesta producida por un agente de IA tras procesar una consulta.
/// </summary>
public class ChatResponse
{
    /// <summary>
    /// Obtiene o establece los elementos o trazas detalladas que componen la respuesta (<see cref="ChatResponseItem"/>).
    /// </summary>
    public ChatResponseItem[]? ResponseItems { get; set; }

    /// <summary>
    /// Obtiene o establece los archivos generados o devueltos por el agente en la respuesta.
    /// </summary>
    public string[]? ResponseFiles { get; set; }
}
