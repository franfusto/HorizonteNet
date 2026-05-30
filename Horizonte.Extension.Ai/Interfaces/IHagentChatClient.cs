using Horizonte.Extension.Ai.Runtime.Chat;

namespace Horizonte.Extension.Ai.Interfaces;

internal interface IHagentChatClient
{
    Task<HagentChatResult> CompleteAsync(
        HagentChatRequest request,
        CancellationToken cancellationToken);
}
