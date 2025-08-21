using System.ClientModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Horizonte;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace Horizonte.Ai.Agent;

public class Agent
{
    private readonly IHGesCom _gesCom;
    private readonly AgentConfig _config;
    private readonly ILogger _logger;
    private readonly OpenAIClient _client;
    private ChatHistory _chat = null!;
    private IChatCompletionService _chatService = null!;
    private OpenAIPromptExecutionSettings _settings = null!;
    private Kernel _kernel = null!;
    private bool _disposed;
    private AgentStatics _agentStatics = new();
    private CommandVectorManager _commandVectorManager;

    public string AgentName { get; }

    public Agent(string agentname, AgentConfig config, IHGesCom gesCom, ILogger logger)
    {
        _gesCom = gesCom;
        _logger = logger;
        _config = config;
        AgentName = agentname;
        _client = CreateOpenAiClient();
        _commandVectorManager = new CommandVectorManager(_gesCom, _logger, _client);
        InitializeAgent();
    }

    public async Task<IReadOnlyList<ChatMessageContent>?> Query(string userPrompt, CancellationToken cancellationToken)
    {
        try
        {
            EnsureChatInitialized();
            ResetChatIfNeeded();
            _chat.AddUserMessage(userPrompt);
            var response = await _chatService.GetChatMessageContentsAsync(_chat, _settings, _kernel, cancellationToken);
            _chat.Add(response[0]);
            _agentStatics.ComputeResponse(response);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return null;
        }
    }


    public ChatHistory GetHistory() => _chat;

    public Dictionary<string, string> GetStatics() => _agentStatics.GetStatics();

    public void InitializeAgent()
    {
        try
        {
            ConfigureKernel(_client);
            InitializeChat(_config.SystemPrompt);
            _logger.LogInformation("Horizonte.Ai.Agent Iniciado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar el agente.");
        }
    }

    private void EnsureChatInitialized()
    {
        if (_chat == null)
        {
            throw new InvalidOperationException("Chat no inicializado.");
        }
    }

    private void ResetChatIfNeeded()
    {
        if (!_config.EnableChatHistory)
        {
            InitializeChat(_config.SystemPrompt);
        }
    }

    private OpenAIClient CreateOpenAiClient()
    {
        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(_config.Url),
            UserAgentApplicationId = _config.UserAgentApplicationId,
            OrganizationId = _config.OrganizationId,
            ProjectId = _config.ProjectId
        };

        return new OpenAIClient(new ApiKeyCredential(_config.ApiKey), clientOptions);
    }

    private async void ConfigureKernel(OpenAIClient client)
    {
        var builder = Kernel.CreateBuilder();
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.SetMinimumLevel(LogLevel.Information);
        });
        _kernel = builder
            .AddOpenAIChatCompletion(_config.ModelId, client)
            .AddOpenAITextEmbeddingGeneration( _config.EmbeddingModelId ?? "text-embedding-3-small", client) 
            .Build();

        var agentTools = new AgentTools(_logger, _gesCom, _commandVectorManager);
        _kernel.ImportPluginFromObject(agentTools);

        _settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = _config.Temperature,
            Store = true,
            MaxTokens = _config.MaxTokens,
            TopP = _config.TopP,
        };

        if (!string.IsNullOrEmpty(_config.OutputType))
        {
            _settings.ResponseFormat = Type.GetType(_config.OutputType) ?? null;
        }

        _chatService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    private void InitializeChat(string systemPrompt)
    {
        _chat = new ChatHistory();
        // Locate and replace substrings enclosed in brackets with the result of their respective commands
        var processedPrompt = System.Text.RegularExpressions.Regex.Replace(systemPrompt, @"\[(.*?)\]", match =>
        {
            var commandName = match.Groups[1].Value;
            try
            {
                // Execute command and replace the placeholder with the result
                var result = _gesCom.RunCommand(commandName)?.ToString();
                return result?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing command '{commandName}': {ex.Message}");
                return string.Empty;
            }
        });

        _chat.AddSystemMessage(processedPrompt);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // Indica al GC que no se requiere la finalización.
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _logger?.LogInformation("Disposing Agent resources.");
                _chatService = null!;
                _chat = null!;
                _settings = null!;
            }

            _disposed = true;
        }
    }

    ~Agent()
    {
        Dispose(false);
    }
}