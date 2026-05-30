using Horizonte.Extension.Ai.Definitions;
using Horizonte.Extension.Ai.Factories;

namespace Horizonte.Extension.Ai;

public static class HagentFactoryExtensions
{
    public static Hagent GetHagent(this AgentDef agentDef, Configuration configuration)
    {
        return HagentFactory.Create(agentDef, configuration);
    }
}