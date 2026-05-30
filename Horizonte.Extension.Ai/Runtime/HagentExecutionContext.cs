using Horizonte.Extension.Ai.Contracts;
using Horizonte.Extension.Ai.Definitions;
using Horizonte.Extension.Ai.Runtime.Files;

namespace Horizonte.Extension.Ai.Runtime;

internal sealed class HagentExecutionContext
{
    public required ChatQuery OriginalQuery { get; init; }

    public required AgentDef AgentDef { get; init; }

    public required AgentFileOptions FileOptions { get; init; }

    public required AgentExecutionOptions ExecutionOptions { get; init; }

    public required CancellationToken CancellationToken { get; init; }

    public List<HagentFile> InputFiles { get; } = [];

    public List<HagentFile> OutputFiles { get; } = [];

    public List<ChatResponseItem> ResponseItems { get; } = [];

    public int TurnCount { get; set; }

    public int ToolCallCount { get; set; }

    public Dictionary<string, object?> Items { get; } = new();
}