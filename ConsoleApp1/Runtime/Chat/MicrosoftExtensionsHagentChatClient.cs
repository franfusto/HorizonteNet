using ConsoleApp1.Definitions;
using ConsoleApp1.Interfaces;
using Microsoft.Extensions.AI;

namespace ConsoleApp1.Runtime.Chat;

public class MicrosoftExtensionsHagentChatClient : IHagentChatClient
{
    private readonly IChatClient _chatClient;
    private readonly ClientDef _clientDef;
    private readonly ServerDef _serverDef;

    public MicrosoftExtensionsHagentChatClient(
        IChatClient chatClient,
        ClientDef clientDef,
        ServerDef serverDef)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentNullException.ThrowIfNull(clientDef);
        ArgumentNullException.ThrowIfNull(serverDef);

        _chatClient = chatClient;
        _clientDef = clientDef;
        _serverDef = serverDef;
    }

    public async Task<HagentChatResult> CompleteAsync(
        HagentChatRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var messages = request.Messages
            .Select(ToMicrosoftChatMessage)
            .ToList();

        var options = BuildChatOptions(request);

        var response = await _chatClient.GetResponseAsync(
            messages,
            options,
            cancellationToken);

        var result = new HagentChatResult
        {
            Text = ExtractText(response)
        };

        result.Metadata["clientId"] = _clientDef.Id;
        result.Metadata["serverId"] = _serverDef.Id;
        result.Metadata["provider"] = _serverDef.Provider;
        result.Metadata["model"] = _clientDef.Model;
        result.Metadata["adapter"] = nameof(MicrosoftExtensionsHagentChatClient);

        return result;
    }

    private static ChatMessage ToMicrosoftChatMessage(HagentMessage message)
    {
        var role = message.Role?.Trim().ToLowerInvariant() switch
        {
            "system" => ChatRole.System,
            "assistant" => ChatRole.Assistant,
            "user" => ChatRole.User,

            // Hagent usa "context" como mensaje propio.
            // Para Microsoft.Extensions.AI lo convertimos en system.
            "context" => ChatRole.System,

            _ => ChatRole.User
        };

        var content = message.Role?.Equals("context", StringComparison.OrdinalIgnoreCase) == true
            ? $"Contexto adicional:\n{message.Content}"
            : message.Content;

        return new ChatMessage(role, content);
    }

    private ChatOptions BuildChatOptions(HagentChatRequest request)
    {
        var options = new ChatOptions
        {
            ModelId = string.IsNullOrWhiteSpace(_clientDef.Model)
                ? null
                : _clientDef.Model,
            Temperature = _clientDef.Temperature,
            MaxOutputTokens = _clientDef.MaxOutputTokens > 0
                ? _clientDef.MaxOutputTokens
                : null
        };

        foreach (var item in request.Options)
        {
            options.AdditionalProperties ??= [];
            options.AdditionalProperties[item.Key] = item.Value;
        }

        foreach (var item in _clientDef.Settings)
        {
            options.AdditionalProperties ??= [];
            options.AdditionalProperties[$"client.{item.Key}"] = item.Value;
        }

        return options;
    }

    private static string ExtractText(ChatResponse response)
    {
        if (!string.IsNullOrWhiteSpace(response.Text))
        {
            return response.Text;
        }

        var parts = response.Messages
            .Select(message => message.Text)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToArray();

        return parts.Length == 0
            ? string.Empty
            : string.Join(Environment.NewLine, parts);
    }
}