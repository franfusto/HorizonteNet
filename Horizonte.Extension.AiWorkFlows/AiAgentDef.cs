using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Horizonte.Extension.AiWorkFlows;

/// <summary>
/// Define la configuración de un agente de inteligencia artificial dentro de los flujos de trabajo de Horizonte.
/// </summary>
public class AiAgentDef
{
    /// <summary>
    /// Obtiene o establece el nombre identificador del agente de IA.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre del modelo de lenguaje utilizado para el chat del agente.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre del modelo utilizado para la generación de embeddings.
    /// </summary>
    public string EmbedderModel { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece las instrucciones de sistema o directivas base del agente.
    /// </summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la lista de herramientas (<see cref="AITool"/>) asignadas al agente.
    /// </summary>
    [JsonIgnore] public IList<AITool>? Tools { get; set; }
}