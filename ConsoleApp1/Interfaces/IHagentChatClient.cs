using ConsoleApp1.Runtime.Chat;

namespace ConsoleApp1.Interfaces;

internal interface IHagentChatClient
{
    Task<HagentChatResult> CompleteAsync(
        HagentChatRequest request,
        CancellationToken cancellationToken);
}
