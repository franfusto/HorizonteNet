using Horizonte.Extension.Ai.Definitions;
using Horizonte.Extension.Ai.Interfaces;

namespace Horizonte.Extension.Ai.Factories;

internal sealed class HagentFactoryContext
{
    public HagentFactoryContext(
        AgentDef agentDef,
        Configuration configuration,
        ClientDef chatClientDef,
        ServerDef chatServerDef,
        AgentFileOptions fileOptions,
        AgentExecutionOptions executionOptions)
    {
        AgentDef = agentDef;
        Configuration = configuration;
        ChatClientDef = chatClientDef;
        ChatServerDef = chatServerDef;
        FileOptions = fileOptions;
        ExecutionOptions = executionOptions;
    }

    public AgentDef AgentDef { get; }

    public Configuration Configuration { get; }

    public ClientDef ChatClientDef { get; }

    public ServerDef ChatServerDef { get; }

    public IHagentChatClient? ChatClient { get; set; }

    public List<IHagentTool> Tools { get; } = [];

    public AgentFileOptions FileOptions { get; }

    public AgentExecutionOptions ExecutionOptions { get; }
}