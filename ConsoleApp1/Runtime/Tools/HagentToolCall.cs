using ConsoleApp1.Definitions;

namespace ConsoleApp1.Runtime.Tools;

public sealed class HagentToolCall
{
    public string ToolId { get; init; } = string.Empty;
    public AgentToolType ToolType { get; init; }
    public string Name { get; init; } = string.Empty;
    public Dictionary<string, object?> Arguments { get; init; } = new();
}
