

using Horizonte.Extension.WorkFlows;

namespace Horizonte.Ai.AgentChat;

public class AgentChatConfig
{
    public AiClientDef ClientDef { get;set; } = new AiClientDef();
    public AiAgentDef AgentDef { get;set; } = new AiAgentDef();
}