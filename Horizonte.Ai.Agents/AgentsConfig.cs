using Horizonte.Extension.Ai.Definitions;

namespace Horizonte.Ai.Agents;

/// <summary>
/// Representa la configuración principal del módulo Horizonte.Ai.Agents, incluyendo conjuntos de configuraciones y ajustes de chat.
/// </summary>
public class AgentsConfig
{
    /// <summary>
    /// Obtiene o establece el conjunto de definiciones y configuraciones de agentes, herramientas, clientes, servidores y RAG.
    /// </summary>
    public ConfigSet ConfigSet { get; set; } = new ConfigSet();

    /// <summary>
    /// Obtiene o establece la configuración activa para la sesión de chat con el agente seleccionado.
    /// </summary>
    public ChatConfig ChatConfig { get; set; } = new ChatConfig();
}

/// <summary>
/// Define los parámetros de selección y nombre para la interacción de chat con agentes.
/// </summary>
public class ChatConfig
{
    /// <summary>
    /// Obtiene o establece el nombre descriptivo de la sesión o agente en el chat.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el identificador del agente seleccionado para la conversación.
    /// </summary>
    public string SelectedId { get; set; } = string.Empty;
}