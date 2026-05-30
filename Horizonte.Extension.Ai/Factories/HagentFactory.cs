using Horizonte.Extension.Ai.Definitions;
using Horizonte.Extension.Ai.Interfaces;
using Horizonte.Extension.Ai.Runtime.Chat;
using Horizonte.Extension.Ai.Runtime.Tools;

namespace Horizonte.Extension.Ai.Factories;

public static class HagentFactory
{
    public static Hagent Create(AgentDef? agentDef, ConfigSet? configuration)
    {
        if (agentDef is null)
        {
            throw new HagentFactoryException("AgentDef no puede ser nulo.");
        }

        if (configuration is null)
        {
            throw new HagentFactoryException("Configuration no puede ser nula.");
        }

        var chatClientDef = ResolveRequiredChatClient(agentDef, configuration);
        var chatServerDef = ResolveRequiredServer(chatClientDef, configuration);
        var fileOptions = ResolveFileOptions(agentDef);
        var executionOptions = ResolveExecutionOptions(agentDef);

        var context = new HagentFactoryContext(
            agentDef,
            configuration,
            chatClientDef,
            chatServerDef,
            fileOptions,
            executionOptions);

        ValidateFileOptions(context);
        ValidateExecutionOptions(context);

        context.ChatClient = CreateChatClient(context);

        ResolveTools(context);

        return CreateHagent(context);
    }

    private static AgentFileOptions ResolveFileOptions(AgentDef agentDef)
    {
        return agentDef.FileOptions ?? new AgentFileOptions();
    }

    private static AgentExecutionOptions ResolveExecutionOptions(AgentDef agentDef)
    {
        return agentDef.ExecutionOptions ?? new AgentExecutionOptions();
    }

    private static ClientDef ResolveRequiredChatClient(
        AgentDef agentDef,
        ConfigSet configSet)
    {
        if (string.IsNullOrWhiteSpace(agentDef.ChatClientId))
        {
            throw new HagentFactoryException(
                $"El agente '{agentDef.Id}' no define '{nameof(agentDef.ChatClientId)}'.");
        }

        var clientDef = configSet.ClientDefs
            .FirstOrDefault(client => client.Id == agentDef.ChatClientId);

        if (clientDef is null)
        {
            throw new HagentFactoryException(
                $"El agente '{agentDef.Id}' referencia el cliente chat '{agentDef.ChatClientId}', pero no existe en la configuración.");
        }

        if (clientDef.ClientType != ClientType.Chat)
        {
            throw new HagentFactoryException(
                $"El cliente '{clientDef.Id}' usado como chat principal debe ser de tipo '{ClientType.Chat}', pero es '{clientDef.ClientType}'.");
        }

        return clientDef;
    }

    private static ServerDef ResolveRequiredServer(
        ClientDef clientDef,
        ConfigSet configSet)
    {
        if (string.IsNullOrWhiteSpace(clientDef.Server))
        {
            throw new HagentFactoryException(
                $"El cliente '{clientDef.Id}' no define servidor.");
        }

        var serverDef = configSet.ServerDefs
            .FirstOrDefault(server => server.Id == clientDef.Server);

        if (serverDef is null)
        {
            throw new HagentFactoryException(
                $"El cliente '{clientDef.Id}' referencia el servidor '{clientDef.Server}', pero no existe en la configuración.");
        }

        return serverDef;
    }

 private static IHagentChatClient CreateChatClient(HagentFactoryContext context)
    {
        var implementation = ResolveChatClientImplementation(
            context.ChatClientDef,
            context.ChatServerDef);

        return implementation switch
        {
            "stub" => new StubHagentChatClient(
                context.ChatClientDef,
                context.ChatServerDef),

            "openai" => new OpenAIHagentChatClient(
                context.ChatClientDef,
                context.ChatServerDef),

            "openai-compatible" => new OpenAICompatibleHagentChatClient(
                context.ChatClientDef,
                context.ChatServerDef),

            "microsoft.extensions.ai" => CreateMicrosoftExtensionsChatClient(context),

            _ => throw new HagentFactoryException(
                $"La implementación de chat '{implementation}' no está soportada para el cliente '{context.ChatClientDef.Id}'.")
        };
    }

    private static IHagentChatClient CreateMicrosoftExtensionsChatClient(
        HagentFactoryContext context)
    {
        if (IsOpenAIProvider(context.ChatServerDef))
        {
            return new OpenAIHagentChatClient(
                context.ChatClientDef,
                context.ChatServerDef);
        }

        if (IsOpenAICompatibleProvider(context.ChatServerDef))
        {
            return new OpenAICompatibleHagentChatClient(
                context.ChatClientDef,
                context.ChatServerDef);
        }

        throw new HagentFactoryException(
            $"No se puede crear un cliente '{nameof(MicrosoftExtensionsHagentChatClient)}' para el proveedor '{context.ChatServerDef.Provider}'.");
    }

    private static string ResolveChatClientImplementation(
        ClientDef clientDef,
        ServerDef serverDef)
    {
        if (clientDef.Settings.TryGetValue("implementation", out var clientImplementation) &&
            !string.IsNullOrWhiteSpace(clientImplementation))
        {
            return clientImplementation.Trim().ToLowerInvariant();
        }

        if (serverDef.Settings.TryGetValue("implementation", out var serverImplementation) &&
            !string.IsNullOrWhiteSpace(serverImplementation))
        {
            return serverImplementation.Trim().ToLowerInvariant();
        }

        if (serverDef.Settings.TryGetValue("chatClient", out var chatClient) &&
            !string.IsNullOrWhiteSpace(chatClient))
        {
            return chatClient.Trim().ToLowerInvariant();
        }

        if (IsOpenAIProvider(serverDef))
        {
            return "openai";
        }

        if (IsOpenAICompatibleProvider(serverDef))
        {
            return "openai-compatible";
        }

        return "stub";
    }

    private static bool IsOpenAIProvider(ServerDef serverDef)
    {
        return string.Equals(
            serverDef.Provider,
            "OpenAI",
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOpenAICompatibleProvider(ServerDef serverDef)
    {
        if (serverDef.Settings.TryGetValue("compatibility", out var compatibility) &&
            string.Equals(compatibility, "openai", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return serverDef.Provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase)
               || serverDef.Provider.Equals("LM Studio", StringComparison.OrdinalIgnoreCase)
               || serverDef.Provider.Equals("vLLM", StringComparison.OrdinalIgnoreCase)
               || serverDef.Provider.Equals("LocalAI", StringComparison.OrdinalIgnoreCase)
               || serverDef.Provider.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase);
    }

private static void ResolveTools(HagentFactoryContext context)
    {
        var toolDefs = ResolveAgentToolDefs(context);

        foreach (var toolDef in toolDefs.Where(tool => tool.Enabled))
        {
            ValidateTool(toolDef, context.ConfigSet);

            var tool = CreateTool(toolDef, context);

            if (tool is not null)
            {
                context.Tools.Add(tool);
            }
        }
    }

    private static AgentToolDef[] ResolveAgentToolDefs(HagentFactoryContext context)
    {
        if (context.AgentDef.Tools is null || context.AgentDef.Tools.Length == 0)
        {
            return [];
        }

        ValidateAgentToolReferences(context);

        return context.AgentDef.Tools
            .Select(toolId =>
                context.ConfigSet.AgentToolDefs.First(toolDef =>
                    string.Equals(
                        toolDef.Id,
                        toolId,
                        StringComparison.OrdinalIgnoreCase)))
            .ToArray();
    }

    private static void ValidateAgentToolReferences(HagentFactoryContext context)
    {
        if (context.ConfigSet.AgentToolDefs is null)
        {
            throw new HagentFactoryException(
                $"{nameof(context.ConfigSet.AgentToolDefs)} no puede ser null.");
        }

        var duplicatedToolDefinitions = context.ConfigSet.AgentToolDefs
            .Where(tool => !string.IsNullOrWhiteSpace(tool.Id))
            .GroupBy(tool => tool.Id, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicatedToolDefinitions.Length > 0)
        {
            throw new HagentFactoryException(
                $"Existen definiciones de tools duplicadas en ConfigSet.AgentToolDefs: {string.Join(", ", duplicatedToolDefinitions)}.");
        }

        var emptyReferences = context.AgentDef.Tools
            .Where(string.IsNullOrWhiteSpace)
            .ToArray();

        if (emptyReferences.Length > 0)
        {
            throw new HagentFactoryException(
                $"El agente '{context.AgentDef.Id}' contiene referencias a tools vacías.");
        }

        var missingToolIds = context.AgentDef.Tools
            .Where(toolId => !context.ConfigSet.AgentToolDefs.Any(toolDef =>
                string.Equals(
                    toolDef.Id,
                    toolId,
                    StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (missingToolIds.Length > 0)
        {
            throw new HagentFactoryException(
                $"El agente '{context.AgentDef.Id}' referencia tools que no existen en ConfigSet.AgentToolDefs: {string.Join(", ", missingToolIds)}.");
        }
    }

    private static void ValidateTool(
        AgentToolDef toolDef,
        ConfigSet configSet)
    {
        switch (toolDef.ToolType)
        {
            case AgentToolType.RagSearch:
                ValidateRagSearchTool(toolDef, configSet);
                break;

            case AgentToolType.ImageToText:
                ValidateClientTool(
                    toolDef,
                    configSet,
                    ClientType.ImageToText);
                break;

            case AgentToolType.TextToImage:
                ValidateClientTool(
                    toolDef,
                    configSet,
                    ClientType.TextToImage);
                break;

            case AgentToolType.FileReader:
            case AgentToolType.FileWriter:
            case AgentToolType.Custom:
                break;

            default:
                throw new HagentFactoryException(
                    $"La tool '{toolDef.Id}' usa un tipo no soportado: '{toolDef.ToolType}'.");
        }
    }

    private static void ValidateRagSearchTool(
        AgentToolDef toolDef,
        ConfigSet configSet)
    {
        if (string.IsNullOrWhiteSpace(toolDef.RagId))
        {
            throw new HagentFactoryException(
                $"La tool RAG '{toolDef.Id}' requiere '{nameof(toolDef.RagId)}'.");
        }

        var ragDef = configSet.RaGdefs
            .FirstOrDefault(rag => rag.Id == toolDef.RagId);

        if (ragDef is null)
        {
            throw new HagentFactoryException(
                $"La tool RAG '{toolDef.Id}' referencia el RAG '{toolDef.RagId}', pero no existe.");
        }

        if (string.IsNullOrWhiteSpace(ragDef.EmbedderClient))
        {
            throw new HagentFactoryException(
                $"El RAG '{ragDef.Id}' no define '{nameof(ragDef.EmbedderClient)}'.");
        }

        var embedderClient = configSet.ClientDefs
            .FirstOrDefault(client => client.Id == ragDef.EmbedderClient);

        if (embedderClient is null)
        {
            throw new HagentFactoryException(
                $"El RAG '{ragDef.Id}' referencia el embedder '{ragDef.EmbedderClient}', pero no existe.");
        }

        if (embedderClient.ClientType != ClientType.Embedder)
        {
            throw new HagentFactoryException(
                $"El cliente '{embedderClient.Id}' usado por el RAG '{ragDef.Id}' debe ser '{ClientType.Embedder}', pero es '{embedderClient.ClientType}'.");
        }

        _ = ResolveRequiredServer(embedderClient, configSet);
    }

    private static void ValidateClientTool(
        AgentToolDef toolDef,
        ConfigSet configSet,
        ClientType expectedClientType)
    {
        if (string.IsNullOrWhiteSpace(toolDef.ClientId))
        {
            throw new HagentFactoryException(
                $"La tool '{toolDef.Id}' de tipo '{toolDef.ToolType}' requiere '{nameof(toolDef.ClientId)}'.");
        }

        var clientDef = configSet.ClientDefs
            .FirstOrDefault(client => client.Id == toolDef.ClientId);

        if (clientDef is null)
        {
            throw new HagentFactoryException(
                $"La tool '{toolDef.Id}' referencia el cliente '{toolDef.ClientId}', pero no existe.");
        }

        if (clientDef.ClientType != expectedClientType)
        {
            throw new HagentFactoryException(
                $"La tool '{toolDef.Id}' requiere un cliente de tipo '{expectedClientType}', pero '{clientDef.Id}' es '{clientDef.ClientType}'.");
        }

        _ = ResolveRequiredServer(clientDef, configSet);
    }

    private static IHagentTool? CreateTool(
        AgentToolDef toolDef,
        HagentFactoryContext context)
    {
        return toolDef.ToolType switch
        {
            AgentToolType.RagSearch => CreateRagSearchTool(toolDef, context),
            AgentToolType.ImageToText => CreateImageToTextTool(toolDef, context),
            AgentToolType.TextToImage => CreateTextToImageTool(toolDef, context),
            AgentToolType.FileReader => new FileReaderTool(toolDef),
            AgentToolType.FileWriter => new FileWriterTool(toolDef),
            AgentToolType.Custom => new CustomHagentTool(toolDef),
            _ => throw new HagentFactoryException(
                $"No existe constructor para la tool '{toolDef.Id}' de tipo '{toolDef.ToolType}'.")
        };
    }
    private static IHagentTool CreateTextToImageTool(
        AgentToolDef toolDef,
        HagentFactoryContext context)
    {
        var clientDef = context.ConfigSet.ClientDefs
            .FirstOrDefault(client => client.Id == toolDef.ClientId);

        if (clientDef is null)
        {
            throw new HagentFactoryException(
                $"La tool TextToImage '{toolDef.Id}' referencia el cliente '{toolDef.ClientId}', pero no existe.");
        }

        if (clientDef.ClientType != ClientType.TextToImage)
        {
            throw new HagentFactoryException(
                $"La tool TextToImage '{toolDef.Id}' requiere un cliente '{ClientType.TextToImage}', pero '{clientDef.Id}' es '{clientDef.ClientType}'.");
        }

        var serverDef = ResolveRequiredServer(clientDef, context.ConfigSet);

        var textToImageClient = CreateTextToImageClient(
            clientDef,
            serverDef);

        return new TextToImageTool(
            toolDef,
            textToImageClient);
    }

    private static IHagentTextToImageClient CreateTextToImageClient(
        ClientDef clientDef,
        ServerDef serverDef)
    {
        return new StubHagentTextToImageClient(
            clientDef,
            serverDef);
    }

    private static IHagentTool CreateImageToTextTool(
        AgentToolDef toolDef,
        HagentFactoryContext context)
    {
        var clientDef = context.ConfigSet.ClientDefs
            .FirstOrDefault(client => client.Id == toolDef.ClientId);

        if (clientDef is null)
        {
            throw new HagentFactoryException(
                $"La tool ImageToText '{toolDef.Id}' referencia el cliente '{toolDef.ClientId}', pero no existe.");
        }

        if (clientDef.ClientType != ClientType.ImageToText)
        {
            throw new HagentFactoryException(
                $"La tool ImageToText '{toolDef.Id}' requiere un cliente '{ClientType.ImageToText}', pero '{clientDef.Id}' es '{clientDef.ClientType}'.");
        }

        var serverDef = ResolveRequiredServer(clientDef, context.ConfigSet);

        var imageClient = CreateImageToTextClient(
            clientDef,
            serverDef);

        return new ImageToTextTool(
            toolDef,
            imageClient);
    }
    private static IHagentImageToTextClient CreateImageToTextClient(
        ClientDef clientDef,
        ServerDef serverDef)
    {
        return new StubHagentImageToTextClient(
            clientDef,
            serverDef);
    }

    private static IHagentTool CreateRagSearchTool(
        AgentToolDef toolDef,
        HagentFactoryContext context)
    {
        var ragDef = context.ConfigSet.RaGdefs
            .FirstOrDefault(rag => rag.Id == toolDef.RagId);

        if (ragDef is null)
        {
            throw new HagentFactoryException(
                $"La tool RAG '{toolDef.Id}' referencia el RAG '{toolDef.RagId}', pero no existe.");
        }

        if (ragDef.RagType != RagType.InMemory)
        {
            throw new HagentFactoryException(
                $"La primera versión de RagSearchTool solo soporta '{RagType.InMemory}', pero el RAG '{ragDef.Id}' es '{ragDef.RagType}'.");
        }

        return new RagSearchTool(toolDef, ragDef);
    }


    private static void ValidateFileOptions(HagentFactoryContext context)
    {
        ValidateFileOptionsStrict(context.FileOptions);

        if (ShouldValidateFunctionalFileOptions(context.AgentDef))
        {
            ValidateFileOptionsFunctional(context);
        }
    }
private static void ValidateFileOptionsStrict(AgentFileOptions fileOptions)
    {
        if (fileOptions.MaxInputFileSizeBytes < 0)
        {
            throw new HagentFactoryException(
                $"{nameof(fileOptions.MaxInputFileSizeBytes)} no puede ser negativo.");
        }

        if (fileOptions.MaxOutputFileSizeBytes < 0)
        {
            throw new HagentFactoryException(
                $"{nameof(fileOptions.MaxOutputFileSizeBytes)} no puede ser negativo.");
        }

        if (fileOptions.AllowedInputMimeTypes is null)
        {
            throw new HagentFactoryException(
                $"{nameof(fileOptions.AllowedInputMimeTypes)} no puede ser null.");
        }

        if (fileOptions.AllowedOutputMimeTypes is null)
        {
            throw new HagentFactoryException(
                $"{nameof(fileOptions.AllowedOutputMimeTypes)} no puede ser null.");
        }

        if (fileOptions.Settings is null)
        {
            throw new HagentFactoryException(
                $"{nameof(fileOptions.Settings)} no puede ser null.");
        }

        if (fileOptions.AllowedInputMimeTypes.Any(string.IsNullOrWhiteSpace))
        {
            throw new HagentFactoryException(
                $"{nameof(fileOptions.AllowedInputMimeTypes)} no puede contener valores vacíos.");
        }

        if (fileOptions.AllowedOutputMimeTypes.Any(string.IsNullOrWhiteSpace))
        {
            throw new HagentFactoryException(
                $"{nameof(fileOptions.AllowedOutputMimeTypes)} no puede contener valores vacíos.");
        }

        if (string.IsNullOrWhiteSpace(fileOptions.TransportEncoding))
        {
            throw new HagentFactoryException(
                $"{nameof(fileOptions.TransportEncoding)} no puede estar vacío.");
        }
    }

    private static bool ShouldValidateFunctionalFileOptions(AgentDef agentDef)
    {
        return agentDef.Settings.TryGetValue("validateFileOptionDependencies", out var value)
               && bool.TryParse(value, out var enabled)
               && enabled;
    }

    private static void ValidateFileOptionsFunctional(HagentFactoryContext context)
    {
        var fileOptions = context.FileOptions;
        var toolDefs = ResolveAgentToolDefs(context);

        if (fileOptions.AutoProcessImages)
        {
            var hasImageTool = toolDefs
                .Any(tool => tool.Enabled && tool.ToolType == AgentToolType.ImageToText);

            if (!hasImageTool)
            {
                throw new HagentFactoryException(
                    $"{nameof(fileOptions.AutoProcessImages)} está activado, pero el agente no tiene una tool '{AgentToolType.ImageToText}' habilitada.");
            }
        }

        if (fileOptions.AutoProcessDocuments)
        {
            var hasFileReader = toolDefs
                .Any(tool => tool.Enabled && tool.ToolType == AgentToolType.FileReader);

            if (!hasFileReader)
            {
                throw new HagentFactoryException(
                    $"{nameof(fileOptions.AutoProcessDocuments)} está activado, pero el agente no tiene una tool '{AgentToolType.FileReader}' habilitada.");
            }
        }

        if (fileOptions.AllowOutputFiles)
        {
            var hasFileWriter = toolDefs
                .Any(tool => tool.Enabled && tool.ToolType == AgentToolType.FileWriter);

            if (!hasFileWriter)
            {
                throw new HagentFactoryException(
                    $"{nameof(fileOptions.AllowOutputFiles)} está activado, pero el agente no tiene una tool '{AgentToolType.FileWriter}' habilitada.");
            }
        }
    }

    
    private static void ValidateExecutionOptions(HagentFactoryContext context)
    {
        var executionOptions = context.ExecutionOptions;

        if (executionOptions.MaxTurns < 1)
        {
            throw new HagentFactoryException(
                $"{nameof(executionOptions.MaxTurns)} debe ser mayor o igual a 1.");
        }

        if (executionOptions.TimeoutSeconds <= 0)
        {
            throw new HagentFactoryException(
                $"{nameof(executionOptions.TimeoutSeconds)} debe ser mayor que 0.");
        }

        if (executionOptions.MaxToolCalls < 0)
        {
            throw new HagentFactoryException(
                $"{nameof(executionOptions.MaxToolCalls)} no puede ser negativo.");
        }
    }

    private static Hagent CreateHagent(HagentFactoryContext context)
    {
        if (context.ChatClient is null)
        {
            throw new HagentFactoryException(
                "No se ha podido resolver el cliente chat principal del agente.");
        }

        return new Hagent(
            context.AgentDef,
            context.ChatClient,
            context.Tools,
            context.FileOptions,
            context.ExecutionOptions);
    }




}