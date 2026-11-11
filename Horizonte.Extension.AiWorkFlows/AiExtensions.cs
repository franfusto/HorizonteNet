using System;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;

namespace Horizonte.Extension.AiWorkFlows;

/// <summary>
/// Proporciona métodos de extensión para la construcción y configuración de agentes y clientes de IA en flujos de trabajo.
/// </summary>
public static class AiExtensions
{
    /// <summary>
    /// Construye una instancia de <see cref="AIAgent"/> a partir de su definición y un cliente de OpenAI.
    /// </summary>
    /// <param name="def">Definición de configuración del agente (<see cref="AiAgentDef"/>).</param>
    /// <param name="client">Cliente de OpenAI utilizado para instanciar el cliente de chat.</param>
    /// <returns>Una nueva instancia de <see cref="AIAgent"/> configurada con las instrucciones, modelo y herramientas especificadas.</returns>
    public static AIAgent Build(this AiAgentDef def, OpenAIClient client)
    {
        ArgumentNullException.ThrowIfNull(def);

        ChatClient chatClient = client.GetChatClient(def.Model);

        var agent = chatClient.AsAIAgent(
            instructions: def.Instructions,
            name: def.Name,
            description: null,
            tools: def.Tools,
            clientFactory: null,
            loggerFactory: null,
            services: null);


        return agent;
    }

    /// <summary>
    /// Asigna una lista de herramientas a la definición del agente de IA.
    /// </summary>
    /// <param name="def">Instancia de <see cref="AiAgentDef"/>.</param>
    /// <param name="tools">Lista de herramientas (<see cref="AITool"/>) a asignar.</param>
    /// <returns>La misma instancia de <see cref="AiAgentDef"/> para permitir encadenamiento fluido.</returns>
    public static AiAgentDef WhithTools(this AiAgentDef def, IList<AITool>? tools)
    {
        def.Tools = tools;
        return def;
    }

    /// <summary>
    /// Crea y configura una instancia de <see cref="OpenAIClient"/> a partir de la definición y el gestor de credenciales.
    /// </summary>
    /// <param name="def">Definición del cliente con nombres de credenciales de endpoint y API key.</param>
    /// <param name="credManager">Gestor de credenciales de Horizonte (<see cref="IHCredManager"/>) para resolver los valores secretos.</param>
    /// <returns>Una instancia configurada de <see cref="OpenAIClient"/>.</returns>
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

