namespace Horizonte.Extension.Ai.Definitions;

/// <summary>
/// Define las opciones y restricciones de ejecución para un agente de IA.
/// </summary>
public class AgentExecutionOptions
{
    /// <summary>
    /// Obtiene o establece el número máximo de turnos o iteraciones permitidas en una conversación.
    /// </summary>
    public int MaxTurns { get; set; } = 8;

    /// <summary>
    /// Obtiene o establece el número máximo de llamadas a herramientas permitidas en una ejecución.
    /// </summary>
    public int MaxToolCalls { get; set; } = 5;

    /// <summary>
    /// Obtiene o establece el tiempo límite de espera en segundos para la ejecución.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Obtiene o establece si las herramientas deben invocarse automáticamente tras ser requeridas por el modelo.
    /// </summary>
    public bool AutoInvokeTools { get; set; } = true;

    /// <summary>
    /// Obtiene o establece si los mensajes producidos por las herramientas deben incluirse en la respuesta.
    /// </summary>
    public bool ReturnToolMessages { get; set; } = true;

    /// <summary>
    /// Obtiene o establece si los pasos y trazas intermedias de ejecución deben devolverse en la respuesta.
    /// </summary>
    public bool ReturnTraceSteps { get; set; } = false;

    /// <summary>
    /// Obtiene o establece opciones de configuración adicionales.
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = new();
}