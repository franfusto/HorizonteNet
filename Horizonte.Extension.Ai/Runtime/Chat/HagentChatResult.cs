using Horizonte.Extension.Ai.Runtime.Tools;

namespace Horizonte.Extension.Ai.Runtime.Chat;

public sealed class HagentChatResult
{
    public string? Text { get; init; }
    public List<HagentToolCall> ToolCalls { get; init; } = [];
    public Dictionary<string, object?> Metadata { get; init; } = new();
}