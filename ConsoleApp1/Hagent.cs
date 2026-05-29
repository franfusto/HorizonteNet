using ConsoleApp1.Contracts;
using ConsoleApp1.Definitions;
using ConsoleApp1.Interfaces;
using ConsoleApp1.Runtime;
using ConsoleApp1.Runtime.Files;

namespace ConsoleApp1;

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
        _agentDef = agentDef;
        _chatClient = chatClient;
        _tools = tools;
        _fileOptions = fileOptions;
        _executionOptions = executionOptions;
        _fileCodec = new HagentFileCodec();
    }

    public ChatResponse Query(ChatQuery query)
    {
        return QueryAsync(query).GetAwaiter().GetResult();
    }

    public async Task<ChatResponse> QueryAsync(
        ChatQuery query,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_executionOptions.TimeoutSeconds));

        var context = new HagentExecutionContext
        {
            Query = query,
            Agent = _agentDef,
            FileOptions = _fileOptions,
            ExecutionOptions = _executionOptions
        };

        await ProcessInputFilesAsync(context, timeoutCts.Token);
        await ExecuteAutoToolsAsync(context, timeoutCts.Token);
        await ExecuteToolLoopAsync(context, timeoutCts.Token);

        return BuildResponse(context);
    }

    private Task ProcessInputFilesAsync(
        HagentExecutionContext context,
        CancellationToken cancellationToken)
    {
        // Diseño:
        // 1. Validar AllowInputFiles.
        // 2. Decodificar ChatQuery.Files.
        // 3. Validar tamaño y MIME.
        // 4. Añadir HagentFile a context.InputFiles.
        return Task.CompletedTask;
    }

    private Task ExecuteAutoToolsAsync(
        HagentExecutionContext context,
        CancellationToken cancellationToken)
    {
        // Diseño:
        // 1. Ejecutar FileReader si procede.
        // 2. Ejecutar ImageToText si AutoProcessImages está activo.
        // 3. Ejecutar RAG si alguna RagSearchTool tiene autoUse.
        return Task.CompletedTask;
    }

    private Task ExecuteToolLoopAsync(
        HagentExecutionContext context,
        CancellationToken cancellationToken)
    {
        // Diseño:
        // 1. Construir mensajes para el chat.
        // 2. Llamar al cliente conversacional.
        // 3. Si el modelo solicita tools, ejecutarlas.
        // 4. Repetir hasta MaxTurns / MaxToolCalls.
        // 5. Añadir respuesta final a context.ResponseItems.
        return Task.CompletedTask;
    }

    private ChatResponse BuildResponse(HagentExecutionContext context)
    {
        return new ChatResponse
        {
            ResponseItems = context.ResponseItems.ToArray(),
            ResponseFiles = _fileCodec.EncodeOutputFiles(context.OutputFiles, _fileOptions)
        };
    }
}
