using ConsoleApp1.Definitions;
using ConsoleApp1.Factories;

namespace ConsoleApp1;

public static class HagentFactoryExtensions
{
    public static Hagent GetHagent(this AgentDef agentDef, Configuration configuration)
    {
        return HagentFactory.Create(agentDef, configuration);
    }
}