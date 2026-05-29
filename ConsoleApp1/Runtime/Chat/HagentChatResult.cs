using ConsoleApp1.Runtime.Tools;

namespace ConsoleApp1.Runtime.Chat;

internal sealed class HagentChatResult
{
    public string? Text { get; init; }
    public List<HagentToolCall> ToolCalls { get; init; } = [];
    public Dictionary<string, object?> Metadata { get; init; } = new();
}