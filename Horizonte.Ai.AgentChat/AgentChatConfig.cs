

using Horizonte.Extension.AiWorkFlows;

namespace Horizonte.Ai.AgentChat;

/// <summary>
/// Representa la configuración requerida para el módulo de chat con agentes Horizonte.Ai.AgentChat.
/// </summary>
public class AgentChatConfig
{
    /// <summary>
    /// Obtiene o establece la definición de conexión del cliente de IA (endpoints, credenciales).
    /// </summary>
    public AiClientDef ClientDef { get; set; } = new AiClientDef();

    /// <summary>
    /// Obtiene o establece la definición del agente de IA (nombre, modelos, instrucciones).
    /// </summary>
    public AiAgentDef AgentDef { get; set; } = new AiAgentDef();
}