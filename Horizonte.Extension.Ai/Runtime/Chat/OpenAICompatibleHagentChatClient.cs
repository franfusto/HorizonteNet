using System.ClientModel;
using Horizonte.Extension.Ai.Definitions;
using Microsoft.Extensions.AI;
using OpenAI;

namespace Horizonte.Extension.Ai.Runtime.Chat;

public sealed class OpenAICompatibleHagentChatClient : MicrosoftExtensionsHagentChatClient
{
    public OpenAICompatibleHagentChatClient(
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
        if (string.IsNullOrWhiteSpace(serverDef.Url))
        {
            throw new InvalidOperationException(
                $"El servidor compatible OpenAI '{serverDef.Id}' no define Url.");
        }

        var apiKey = ResolveApiKeyOrDefault(serverDef);

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(NormalizeOpenAICompatibleEndpoint(serverDef.Url))
        };

        var openAiClient = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            options);

        return openAiClient
            .GetChatClient(clientDef.Model)
            .AsIChatClient();
    }

    private static string NormalizeOpenAICompatibleEndpoint(string url)
    {
        var trimmed = url.TrimEnd('/');

        // Ollama suele exponer OpenAI-compatible en /v1.
        // Si el usuario ya puso /v1, lo respetamos.
        if (trimmed.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        return $"{trimmed}/v1";
    }

    private static string ResolveApiKeyOrDefault(ServerDef serverDef)
    {
        if (string.IsNullOrWhiteSpace(serverDef.ApiKey))
        {
            // Muchos servidores locales compatibles OpenAI ignoran el api key,
            // pero el SDK necesita un valor no vacío.
            return "local-development-key";
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