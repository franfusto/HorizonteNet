using ConsoleApp1.Definitions;

namespace ConsoleApp1.Contracts;

public class ChatResponseItem
{
    public ChatResponseItemType Type { get; set; } = ChatResponseItemType.Think;
    public string Content { get; set; } = string.Empty;
}
