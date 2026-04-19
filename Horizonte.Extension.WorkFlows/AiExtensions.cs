using System;
using System.ClientModel;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;

namespace Horizonte.Extension.WorkFlows;

public static class AiExtensions
{
    public static AIAgent Build(this AiAgentDef def)
    {
        ArgumentNullException.ThrowIfNull(def);

        var endpoint = string.IsNullOrWhiteSpace(def.Endpoint)
            ? "https://api.openai.com/v1"
            : def.Endpoint;

        var apiKey = string.IsNullOrWhiteSpace(def.ApiKey)
            ? "not-provided"
            : def.ApiKey;

        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint, UriKind.Absolute)
        };

        var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey), clientOptions);
        var model = string.IsNullOrWhiteSpace(def.Model) ? "gpt-4o-mini" : def.Model;
        ChatClient chatClient = openAiClient.GetChatClient(model);

        var name = string.IsNullOrWhiteSpace(def.Name)
            ? (string.IsNullOrWhiteSpace(def.Id) ? Guid.NewGuid().ToString() : def.Id)
            : def.Name;

        return chatClient.AsAIAgent(
            instructions: def.Instructions,
            name: name,
            description: null,
            tools: null,
            clientFactory: null,
            loggerFactory: null,
            services: null);
    }
}