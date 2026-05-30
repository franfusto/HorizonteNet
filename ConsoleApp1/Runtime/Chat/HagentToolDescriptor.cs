using ConsoleApp1.Definitions;

namespace ConsoleApp1.Runtime.Chat;

public sealed class HagentToolDescriptor
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public AgentToolType ToolType { get; init; }
    public string Description { get; init; } = string.Empty;
    public Dictionary<string, object?> Schema { get; init; } = new();
}