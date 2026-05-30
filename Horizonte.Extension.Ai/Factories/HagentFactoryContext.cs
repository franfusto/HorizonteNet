using Horizonte.Extension.Ai.Definitions;
using Horizonte.Extension.Ai.Interfaces;

namespace Horizonte.Extension.Ai.Factories;

internal sealed class HagentFactoryContext
{
    public HagentFactoryContext(
        AgentDef agentDef,
        ConfigSet configSet,
        ClientDef chatClientDef,
        ServerDef chatServerDef,
        AgentFileOptions fileOptions,
        AgentExecutionOptions executionOptions)
    {
        AgentDef = agentDef;
        ConfigSet = configSet;
        ChatClientDef = chatClientDef;
        ChatServerDef = chatServerDef;
        FileOptions = fileOptions;
        ExecutionOptions = executionOptions;
    }

    public AgentDef AgentDef { get; }

    public ConfigSet ConfigSet { get; }

    public ClientDef ChatClientDef { get; }

    public ServerDef ChatServerDef { get; }

    public IHagentChatClient? ChatClient { get; set; }

    public List<IHagentTool> Tools { get; } = [];

    public AgentFileOptions FileOptions { get; }

    public AgentExecutionOptions ExecutionOptions { get; }
}