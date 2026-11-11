namespace Horizonte.Extension.Ai.Definitions;

/// <summary>
/// Define la configuración completa de un agente inteligente en el ecosistema Horizonte.
/// </summary>
public class AgentDef
{
    /// <summary>
    /// Obtiene o establece el identificador único del agente.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre descriptivo del agente.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la descripción funcional del agente.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el prompt del sistema que instruye el comportamiento del agente.
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el identificador del cliente de chat asociado para inferencia.
    /// </summary>
    public string ChatClientId { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece los identificadores de las herramientas habilitadas para este agente.
    /// </summary>
    public string[] Tools { get; set; } = [];

    /// <summary>
    /// Obtiene o establece las opciones para el manejo y transporte de archivos (<see cref="AgentFileOptions"/>).
    /// </summary>
    public AgentFileOptions FileOptions { get; set; } = new();

    /// <summary>
    /// Obtiene o establece las opciones de ejecución y límites del agente (<see cref="AgentExecutionOptions"/>).
    /// </summary>
    public AgentExecutionOptions ExecutionOptions { get; set; } = new();

    /// <summary>
    /// Obtiene o establece parámetros de configuración adicionales personalizados.
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = new();
}