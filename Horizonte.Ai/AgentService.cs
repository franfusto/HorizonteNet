using Horizonte.Extension.Ai.Entities;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;

namespace Horizonte.Ai;

public class AgentService
{
    public AgentService(IHorizonteEnv env)
    {
        _env = env;
        
    }


    public AIAgent BuildAgent()
    {
        AIAgent _agent;
        var _credManager = _env.GetService<IHCredManager>();    
        var apiKey = _credManager?.GetCredential("openai.key") ?? throw new InvalidOperationException("OPENAI_API_KEY is not set.");
        var model = "gpt-4o-mini";
        _agent = new OpenAIClient(apiKey)
            .GetChatClient(model)
            .CreateAIAgent(instructions: "Eres un amable asistente", name: "Hache");
        return _agent;
    }

    public AIAgent BuildAgent(string agentId)
    {
        return null!;
    }
    public AIAgent BuildAgent(AiAgentDef agentSettings)
    {
        return null!;
    }
    
    
    
    private readonly IHorizonteEnv _env;
}