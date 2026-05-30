namespace Horizonte.Extension.Ai.Runtime.Chat;

public sealed class HagentMessage
{
    public string Role { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Dictionary<string, object?> Metadata { get; init; } = new();
}