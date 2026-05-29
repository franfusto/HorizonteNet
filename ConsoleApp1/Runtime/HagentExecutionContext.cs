using ConsoleApp1.Contracts;
using ConsoleApp1.Definitions;
using ConsoleApp1.Runtime.Files;

namespace ConsoleApp1.Runtime;

internal sealed class HagentExecutionContext
{
    public required ChatQuery Query { get; init; }
    public required AgentDef Agent { get; init; }
    public required AgentFileOptions FileOptions { get; init; }
    public required AgentExecutionOptions ExecutionOptions { get; init; }

    public List<HagentFile> InputFiles { get; } = [];
    public List<HagentFile> OutputFiles { get; } = [];
    public List<ChatResponseItem> ResponseItems { get; } = [];

    public int TurnCount { get; set; }
    public int ToolCallCount { get; set; }

    public Dictionary<string, object?> Items { get; } = new();
}