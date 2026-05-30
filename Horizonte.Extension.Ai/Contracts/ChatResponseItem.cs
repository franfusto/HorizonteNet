using Horizonte.Extension.Ai.Definitions;

namespace Horizonte.Extension.Ai.Contracts;

public class ChatResponseItem
{
    public ChatResponseItemType Type { get; set; } = ChatResponseItemType.Think;
    public string Content { get; set; } = string.Empty;
}
