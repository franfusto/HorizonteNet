using System.Text;
using Horizonte.Extension.Ai.Contracts;
using Horizonte.Extension.Ai.Definitions;
using Horizonte.Extension.Ai.Interfaces;
using Horizonte.Extension.Ai.Runtime;
using Horizonte.Extension.Ai.Runtime.Chat;
using Horizonte.Extension.Ai.Runtime.Files;
using Horizonte.Extension.Ai.Runtime.Tools;

namespace Horizonte.Extension.Ai;

public class Hagent
{
    private readonly AgentDef _agentDef;
    private readonly IHagentChatClient _chatClient;
    private readonly IReadOnlyList<IHagentTool> _tools;
    private readonly AgentFileOptions _fileOptions;
    private readonly AgentExecutionOptions _executionOptions;
    private readonly HagentFileCodec _fileCodec;

    internal Hagent(
        AgentDef agentDef,
        IHagentChatClient chatClient,
        IReadOnlyList<IHagentTool> tools,
        AgentFileOptions fileOptions,
        AgentExecutionOptions executionOptions)
    {
        ArgumentNullException.ThrowIfNull(agentDef);
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentNullException.ThrowIfNull(tools);
        ArgumentNullException.ThrowIfNull(fileOptions);
        ArgumentNullException.ThrowIfNull(executionOptions);

        _agentDef = agentDef;
        _chatClient = chatClient;
        _tools = tools.ToArray();
        _fileOptions = fileOptions;
        _executionOptions = executionOptions;
        _fileCodec = new HagentFileCodec();
    }

    /// <summary>
    /// Ejecuta una consulta de forma síncrona.
    /// La API principal recomendada es <see cref="QueryAsync(ChatQuery, CancellationToken)"/>.
    /// </summary>
    public ChatResponse Query(ChatQuery query)
    {
        return QueryAsync(query).GetAwaiter().GetResult();
    }

    public async Task<ChatResponse> QueryAsync(
        ChatQuery query,
        CancellationToken cancellationToken = default)
    {
        ValidateQuery(query);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_executionOptions.TimeoutSeconds));

        var context = new HagentExecutionContext
        {
            OriginalQuery = query,
            AgentDef = _agentDef,
            FileOptions = _fileOptions,
            ExecutionOptions = _executionOptions,
            CancellationToken = timeoutCts.Token
        };

        await ProcessInputFilesAsync(context, timeoutCts.Token);
        await ExecuteAutoToolsAsync(context, timeoutCts.Token);
        await ExecuteToolLoopAsync(context, timeoutCts.Token);

        return BuildResponse(context);
    }

    private static void ValidateQuery(ChatQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.Query))
        {
            throw new ArgumentException(
                "La consulta no puede estar vacía.",
                nameof(query));
        }
    }

    private Task ProcessInputFilesAsync(
        HagentExecutionContext context,
        CancellationToken cancellationToken)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var inputFiles = _fileCodec.DecodeInputFiles(
            context.OriginalQuery.Files,
            context.FileOptions);

        context.InputFiles.AddRange(inputFiles);

        return Task.CompletedTask;
    }

    private async Task ExecuteAutoToolsAsync(
        HagentExecutionContext context,
        CancellationToken cancellationToken)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        if (!context.ExecutionOptions.AutoInvokeTools)
        {
            context.Items["autoTools.skipped"] = true;
            return;
        }

        if (context.ExecutionOptions.MaxToolCalls == 0)
        {
            context.Items["autoTools.skipped"] = true;
            context.Items["autoTools.skipReason"] = "MaxToolCalls es 0.";
            return;
        }

        var toolContext = BuildToolContext(context);

        foreach (var tool in _tools.Where(tool => tool.Enabled))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (context.ToolCallCount >= context.ExecutionOptions.MaxToolCalls)
            {
                context.Items["autoTools.limitReached"] = true;
                break;
            }

            var shouldAutoInvoke = await tool.ShouldAutoInvokeAsync(
                toolContext,
                context.CancellationToken);

            if (!shouldAutoInvoke)
            {
                continue;
            }

            await ExecuteAutoToolAsync(
                tool,
                toolContext,
                context,
                context.CancellationToken);
        }
    }

    private async Task ExecuteAutoToolAsync(
        IHagentTool tool,
        HagentToolContext toolContext,
        HagentExecutionContext context,
        CancellationToken cancellationToken)
    {
        var call = new HagentToolCall
        {
            ToolId = tool.Id,
            Name = tool.Name,
            ToolType = tool.ToolType,
            Arguments =
            {
                ["mode"] = "auto"
            }
        };

        if (context.ExecutionOptions.ReturnToolMessages)
        {
            context.ResponseItems.Add(new ChatResponseItem
            {
                Type = ChatResponseItemType.Tool,
                Content = $"Ejecutando tool automática '{tool.Name}'."
            });
        }

        var result = await tool.ExecuteAsync(
            call,
            toolContext,
            cancellationToken);

        context.ToolCallCount++;

        ApplyToolResult(
            tool,
            result,
            context);
    }

    private async Task ExecuteToolLoopAsync(
        HagentExecutionContext context,
        CancellationToken cancellationToken)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        if (context.TurnCount >= context.ExecutionOptions.MaxTurns)
        {
            context.Items["chat.skipped"] = true;
            context.Items["chat.skipReason"] = "MaxTurns alcanzado antes de llamar al chat.";
            return;
        }

        var request = BuildChatRequest(context);

        var result = await _chatClient.CompleteAsync(
            request,
            cancellationToken);

        context.TurnCount++;
        context.Items["chat.lastResult"] = result;

        if (!string.IsNullOrWhiteSpace(result.Text))
        {
            context.ResponseItems.Add(new ChatResponseItem
            {
                Type = ChatResponseItemType.Response,
                Content = result.Text
            });
        }

        if (result.ToolCalls.Count > 0)
        {
            context.Items["chat.toolCalls"] = result.ToolCalls;
            await HandleModelToolCallsAsync(
                result.ToolCalls,
                context,
                cancellationToken);
        }
    }
    private Task HandleModelToolCallsAsync(
        IReadOnlyList<HagentToolCall> toolCalls,
        HagentExecutionContext context,
        CancellationToken cancellationToken)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        context.Items["chat.toolCalls.ignored"] = true;
        context.Items["chat.toolCalls.ignoreReason"] =
            "Tool-calling iterativo todavía no está implementado. Fase 8 Nivel 2 queda preparado para una fase posterior.";

        if (context.ExecutionOptions.ReturnToolMessages)
        {
            foreach (var toolCall in toolCalls)
            {
                context.ResponseItems.Add(new ChatResponseItem
                {
                    Type = ChatResponseItemType.Tool,
                    Content = $"El modelo solicitó la tool '{toolCall.Name}' ({toolCall.ToolType}), pero el tool-calling iterativo aún no está habilitado."
                });
            }
        }

        return Task.CompletedTask;
    }
    private HagentToolContext BuildToolContext(HagentExecutionContext context)
    {
        return new HagentToolContext
        {
            AgentId = _agentDef.Id,
            UserQuery = context.OriginalQuery.Query,
            InputFiles = context.InputFiles,
            OutputFiles = context.OutputFiles,
            FileOptions = context.FileOptions,
            ExecutionOptions = context.ExecutionOptions,
            Items = context.Items
        };
    }

    private static void ApplyToolResult(
        IHagentTool tool,
        HagentToolResult result,
        HagentExecutionContext context)
    {
        var resultKey = $"tool.{tool.Id}.result";
        context.Items[resultKey] = result;

        if (result.Files.Count > 0)
        {
            context.OutputFiles.AddRange(result.Files);
        }

        if (context.ExecutionOptions.ReturnToolMessages)
        {
            if (result.ResponseItems.Count > 0)
            {
                context.ResponseItems.AddRange(result.ResponseItems);
            }
            else
            {
                context.ResponseItems.Add(new ChatResponseItem
                {
                    Type = ChatResponseItemType.Tool,
                    Content = result.Success
                        ? $"Tool automática '{tool.Name}' ejecutada correctamente."
                        : $"Tool automática '{tool.Name}' falló: {result.ErrorMessage}"
                });
            }
        }
    }

    private HagentChatRequest BuildChatRequest(HagentExecutionContext context)
    {
        var request = new HagentChatRequest
        {
            Messages =
            {
                new HagentMessage
                {
                    Role = "system",
                    Content = _agentDef.SystemPrompt
                }
            },
            Tools = _tools
                .Where(tool => tool.Enabled)
                .Select(tool => new HagentToolDescriptor
                {
                    Id = tool.Id,
                    Name = tool.Name,
                    ToolType = tool.ToolType
                })
                .ToList(),
            Options =
            {
                ["agentId"] = _agentDef.Id,
                ["agentName"] = _agentDef.Name,
                ["maxTurns"] = _executionOptions.MaxTurns,
                ["maxToolCalls"] = _executionOptions.MaxToolCalls,
                ["autoInvokeTools"] = _executionOptions.AutoInvokeTools,
                ["turnCount"] = context.TurnCount,
                ["toolCallCount"] = context.ToolCallCount
            }
        };

        var contextMessage = BuildTextualContext(context);

        if (!string.IsNullOrWhiteSpace(contextMessage))
        {
            request.Messages.Add(new HagentMessage
            {
                Role = "context",
                Content = contextMessage
            });
        }

        request.Messages.Add(new HagentMessage
        {
            Role = "user",
            Content = context.OriginalQuery.Query
        });

        return request;
    }
    private static string? BuildTextualContext(HagentExecutionContext context)
    {
        var builder = new StringBuilder();

        AppendInputFilesContext(builder, context);
        AppendKnownContextItem(builder, context, "rag.context", "Contexto RAG");
        AppendKnownContextItem(builder, context, "image.descriptions", "Descripciones de imágenes");
        AppendKnownContextItem(builder, context, "file.extractedText", "Texto extraído de archivos");
        AppendToolResultsContext(builder, context);

        return builder.Length == 0
            ? null
            : builder.ToString();
    }
    private static void AppendInputFilesContext(
        StringBuilder builder,
        HagentExecutionContext context)
    {
        if (context.InputFiles.Count == 0)
        {
            return;
        }

        builder.AppendLine("Archivos de entrada disponibles:");
        builder.AppendLine();

        foreach (var file in context.InputFiles)
        {
            builder.Append("- ");
            builder.Append(file.FileName);
            builder.Append(" (");
            builder.Append(file.MimeType);
            builder.Append(", ");
            builder.Append(file.Content.Length);
            builder.AppendLine(" bytes)");
        }

        builder.AppendLine();
    }

    private static void AppendKnownContextItem(
        StringBuilder builder,
        HagentExecutionContext context,
        string key,
        string title)
    {
        if (!context.Items.TryGetValue(key, out var value) || value is null)
        {
            return;
        }

        var text = value.ToString();

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        builder.AppendLine(title + ":");
        builder.AppendLine(text);
        builder.AppendLine();
    }

    private static void AppendToolResultsContext(
        StringBuilder builder,
        HagentExecutionContext context)
    {
        var toolResults = context.Items
            .Where(item =>
                item.Key.StartsWith("tool.", StringComparison.OrdinalIgnoreCase) &&
                item.Key.EndsWith(".result", StringComparison.OrdinalIgnoreCase) &&
                item.Value is HagentToolResult)
            .Select(item => (Key: item.Key, Result: (HagentToolResult)item.Value!))
            .ToArray();

        if (toolResults.Length == 0)
        {
            return;
        }

        builder.AppendLine("Resultados de tools automáticas:");
        builder.AppendLine();

        foreach (var toolResult in toolResults)
        {
            builder.AppendLine(toolResult.Key + ":");

            if (!toolResult.Result.Success)
            {
                builder.AppendLine("Error: " + toolResult.Result.ErrorMessage);
            }
            else if (!string.IsNullOrWhiteSpace(toolResult.Result.Text))
            {
                builder.AppendLine(toolResult.Result.Text);
            }
            else
            {
                builder.AppendLine("Tool ejecutada correctamente sin texto adicional.");
            }

            builder.AppendLine();
        }
    }

    private ChatResponse BuildResponse(HagentExecutionContext context)
    {
        if (context.ResponseItems.Count == 0)
        {
            context.ResponseItems.Add(new ChatResponseItem
            {
                Type = ChatResponseItemType.Response,
                Content = string.Empty
            });
        }

        return new ChatResponse
        {
            ResponseItems = context.ResponseItems.ToArray(),
            ResponseFiles = _fileCodec.EncodeOutputFiles(context.OutputFiles, _fileOptions)
        };
    }
}