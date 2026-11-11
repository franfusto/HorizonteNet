namespace Horizonte.Extension.Ai.Definitions;

/// <summary>
/// Define la configuración de una herramienta disponible para los agentes de IA.
/// </summary>
public class AgentToolDef
{
    /// <summary>
    /// Obtiene o establece el identificador único de la herramienta.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre descriptivo de la herramienta.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el tipo funcional de la herramienta (<see cref="AgentToolType"/>).
    /// </summary>
    public AgentToolType ToolType { get; set; } = AgentToolType.RagSearch;

    /// <summary>
    /// Obtiene o establece un valor que indica si la herramienta está habilitada.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Obtiene o establece el tiempo de espera máximo en segundos para la ejecución de la herramienta.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Obtiene o establece el identificador del cliente de IA requerido por la herramienta (si aplica).
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador del recurso RAG asociado a la herramienta (si aplica).
    /// </summary>
    public string? RagId { get; set; }

    /// <summary>
    /// Obtiene o establece parámetros de configuración adicionales para la herramienta.
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = new();
}