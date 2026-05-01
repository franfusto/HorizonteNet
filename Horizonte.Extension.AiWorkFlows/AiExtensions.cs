using System;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;

namespace Horizonte.Extension.AiWorkFlows;

public static class AiExtensions
{
    public static AIAgent Build(this AiAgentDef def, OpenAIClient client)
    {
        ArgumentNullException.ThrowIfNull(def);

        ChatClient chatClient = client.GetChatClient(def.Model);

        var agent = chatClient. AsAIAgent(
            instructions: def.Instructions,
            name: def.Name,
            description: null,
            tools: def.Tools,
            clientFactory: null,
            loggerFactory: null,
            services: null);


        return agent;
    }

    public static AiAgentDef WhithTools(this AiAgentDef def, IList<AITool>? tools)
    {
        def.Tools = tools;
        return def;
    }

    public static OpenAIClient GetClient(this AiClientDef def, IHCredManager credManager)
    {
        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(credManager.GetCredential(def.Endpoint), UriKind.Absolute)
        };
        var openAiClient = new OpenAIClient(new ApiKeyCredential(credManager.GetCredential(def.ApiKey)), clientOptions);
        return openAiClient;
    }
}

