namespace ConsoleApp1.Runtime.Chat;

internal sealed class HagentChatRequest
{
    public List<HagentMessage> Messages { get; init; } = [];
    public List<HagentToolDescriptor> Tools { get; init; } = [];
    public Dictionary<string, object?> Options { get; init; } = new();
}
