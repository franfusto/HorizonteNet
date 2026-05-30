using System.ClientModel;
using Horizonte.Extension.Ai.Definitions;
using Microsoft.Extensions.AI;
using OpenAI;

namespace Horizonte.Extension.Ai.Runtime.Chat;

public sealed class OpenAIHagentChatClient : MicrosoftExtensionsHagentChatClient
{
    public OpenAIHagentChatClient(
        ClientDef clientDef,
        ServerDef serverDef)
        : base(
            CreateChatClient(clientDef, serverDef),
            clientDef,
            serverDef)
    {
    }

    private static IChatClient CreateChatClient(
        ClientDef clientDef,
        ServerDef serverDef)
    {
        var apiKey = ResolveApiKey(serverDef);

        var openAiClient = new OpenAIClient(
            new ApiKeyCredential(apiKey));

        return openAiClient
            .GetChatClient(clientDef.Model)
            .AsIChatClient();
    }

    private static string ResolveApiKey(ServerDef serverDef)
    {
        if (string.IsNullOrWhiteSpace(serverDef.ApiKey))
        {
            throw new InvalidOperationException(
                $"El servidor '{serverDef.Id}' no define ApiKey.");
        }

        if (serverDef.ApiKey.StartsWith("env:", StringComparison.OrdinalIgnoreCase))
        {
            var environmentVariableName = serverDef.ApiKey["env:".Length..];
            var value = Environment.GetEnvironmentVariable(environmentVariableName);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"La variable de entorno '{environmentVariableName}' no está definida o está vacía.");
            }

            return value;
        }

        return serverDef.ApiKey;
    }
}