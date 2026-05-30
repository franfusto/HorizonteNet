using ConsoleApp1.Definitions;

namespace ConsoleApp1.Runtime.Chat;

internal sealed class StubHagentChatClient : Interfaces.IHagentChatClient
{
    private readonly ClientDef _clientDef;
    private readonly ServerDef _serverDef;

    public StubHagentChatClient(
        ClientDef clientDef,
        ServerDef serverDef)
    {
        _clientDef = clientDef;
        _serverDef = serverDef;
    }

    public Task<HagentChatResult> CompleteAsync(
        HagentChatRequest request,
        CancellationToken cancellationToken)
    {
        var userMessage = request.Messages
            .LastOrDefault(message => message.Role == "user")
            ?.Content;

        var responseText = string.IsNullOrWhiteSpace(userMessage)
            ? "Respuesta generada por StubHagentChatClient."
            : $"Respuesta generada por StubHagentChatClient para: {userMessage}";

        var result = new HagentChatResult
        {
            Text = responseText,
            Metadata =
            {
                ["clientId"] = _clientDef.Id,
                ["serverId"] = _serverDef.Id,
                ["provider"] = "stub"
            }
        };

        return Task.FromResult(result);
    }
}