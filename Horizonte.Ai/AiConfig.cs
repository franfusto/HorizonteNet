using Horizonte.Extension.Ai.Entities;

namespace Horizonte.Ai;

public class AiConfig
{
    public AiConfig()
    {
        ClientList.Add(new AiClientDef()
        {
            ApiKey =  "AI_API_KEY",
            Endpoint =  "AI_ENDPOINT",
            Id =  Guid.NewGuid().ToString(),
            Name = "AI_API_Name"
        });

        AgentList.Add(new AiAgentDef()
        {
            Id =  Guid.NewGuid().ToString(),
            ClientId =   "AI_AI_CLIENT_ID",
            Name = "AI_API_Name"
        });
    }
    
    public List<AiClientDef> ClientList { get; set; } = new List<AiClientDef>();
    public List<AiAgentDef> AgentList { get; set; }= new List<AiAgentDef>();
}