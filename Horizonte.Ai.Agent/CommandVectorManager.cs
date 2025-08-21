using System.Text.Json;
using Horizonte;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OpenAI;

namespace Horizonte.Ai.Agent;

public class CommandVectorManager
{
    private readonly IHGesCom _commandManager;
    private readonly ILogger _logService;
    private readonly OpenAIClient _openAiClient;

    // Constantes y campos privados
    private const string VectorCollectionName = "commands";
    private const string CacheFilePath = "cache_commands.json";
    private List<CommandVectorRecord> _commandDefinitions = new();
    private List<ActiveCommandDefinition> _activeCommands = new();
    private IVectorStoreRecordCollection<int, CommandVectorRecord>? _commandVectorCollection;

    public CommandVectorManager(IHGesCom commandManager, ILogger logService, OpenAIClient openAiClient)
    {
        _commandManager = commandManager;
        _logService = logService;
        _openAiClient = openAiClient;
        Task.Run(InitializeManagerAsync);
        _logService.LogInformation("Inicializando CommandVectorManager...");

    }

    // Método público principal
    private async Task InitializeManagerAsync()
    {
        try
        {
            _logService.LogInformation("Iniciando proceso de inicialización del administrador...");
            await LoadCacheFromFile();
            await LoadActiveCommandDefinitions();
            await ValidateCacheConsistencyAsync();
            await SerializeCache();
            await PopulateVectorStorageAsync();
            _logService.LogInformation("Inicialización completada.");

        }
        catch (Exception e)
        {
            _logService.LogError(e, "Error crítico durante la inicialización del administrador.");

        }

    }

    private Task LoadCacheFromFile()
    {
        try
        {
            _logService.LogInformation("Cargando el cache de comandos desde el archivo...");
            _commandDefinitions = File.Exists(CacheFilePath)
                ? DeserializeCacheFromFile()
                : new List<CommandVectorRecord>();
        }
        catch (Exception e)
        {
            _logService.LogError(e, "Error al cargar el cache de comandos");
        }
        return Task.CompletedTask;
    }

    private Task LoadActiveCommandDefinitions()
    {
        try
        {
            _logService.LogInformation("Cargando definiciones activas de comandos...");
            _activeCommands = (_commandManager?.GetCommandList() as List<HCommand>)?
                .Select(hCommand => new ActiveCommandDefinition
                {
                    Name = hCommand.CommandKey,
                    Definition = GenerateCommandJson(hCommand)
                })
                .ToList() ?? new List<ActiveCommandDefinition>();

        }
        catch (Exception e)
        {
            _logService.LogError(e, "Error al cargar las definiciones activas de comandos.");

        }
        return Task.CompletedTask;
    }

    public async Task ValidateCacheConsistencyAsync()
    {
        try
        {
            _logService.LogInformation("Validando la consistencia del cache de comandos...");
            foreach (var command in _activeCommands)
            {
                await UpdateOrAddCommandDefinition(command);
            }

        }
        catch (Exception e)
        {
            _logService.LogError(e, "Error al validar la consistencia del cache de comandos.");
        }

    }
    private Task SerializeCache()
    {
        try
        {
            _logService.LogInformation("Serializando el cache de comandos en disco...");
            string fileContent = JsonSerializer.Serialize(_commandDefinitions, JsonSerializerOptions);
            File.WriteAllText(CacheFilePath, fileContent);
        }
        catch (Exception e)
        {
            _logService.LogError(e, "Error durante la serialización del cache de comandos.");
        }
        return Task.CompletedTask;
    }
    
    private async Task UpdateOrAddCommandDefinition(ActiveCommandDefinition command)
    {
        try
        {
            var existingCommand = _commandDefinitions.FirstOrDefault(def => def.Name == command.Name);
            if (existingCommand == null || command.Definition != existingCommand.Definition)
            {
                _commandDefinitions.Remove(existingCommand);
                _commandDefinitions.Add(new CommandVectorRecord
                {
                    Name = command.Name,
                    Definition = command.Definition,
                    Embedding = await GenerateEmbeddingsFromTextAsync(command.Definition, CancellationToken.None)
                });
                _logService.LogInformation("Comando {CommandName} añadido o actualizado en el cache.", command.Name);

            }
        }
        catch (Exception e)
        {
            _logService.LogError(e, "Error al actualizar o agregar la definición del comando {CommandName}.", command.Name);

        }
    }


    
    public List<CommandVectorRecord> DeserializeCacheFromFile()
    {
        try
        {
            _logService.LogInformation("Deserializando el cache de comandos desde disco...");
            string fileContent = File.ReadAllText(CacheFilePath);
            return JsonSerializer.Deserialize<List<CommandVectorRecord>>(fileContent) ?? new List<CommandVectorRecord>();
        }
        catch (JsonException e)
        {
            _logService.LogError(e, "Formato de JSON no válido en el archivo de cache.");
            return new List<CommandVectorRecord>();
        }
        catch (Exception e)
        {
            _logService.LogError(e, "Error general al deserializar el archivo de cache.");
            return new List<CommandVectorRecord>();
        }

    }

    public async Task PopulateVectorStorageAsync()
    {
        try
        {
            _logService.LogInformation("Cargando los datos en la base de datos vectorial...");

            var vectorStore = new InMemoryVectorStore();
            await vectorStore.GetCollection<int, CommandVectorRecord>(VectorCollectionName)
                .CreateCollectionIfNotExistsAsync();

            _commandVectorCollection = vectorStore.GetCollection<int, CommandVectorRecord>(VectorCollectionName);
            for (int i = 0; i < _commandDefinitions.Count; i++)
            {
                _commandDefinitions[i].Id = i;
                await _commandVectorCollection.UpsertAsync(_commandDefinitions[i]);
            }
        }
        catch (Exception e)
        {
            _logService.LogError(e, "Error al cargar los datos en la base de datos vectorial.");

        }
    }

    public async Task<string?> SearchCommandInVectorStoreAsync(string catalog, string queryText, CancellationToken cancellationToken)
    {
        _logService.LogInformation("Buscando comandos en la base de datos vectorial para la consulta: {QueryText}.", queryText);
        try
        {
            string textResult = string.Empty;
            var query = await GenerateEmbeddingsFromTextAsync(queryText, cancellationToken);
            var searchResults = await _commandVectorCollection!.VectorizedSearchAsync(query, new() { Top = 5 });
            
        foreach (var item in searchResults.Results.ToBlockingEnumerable())
            {
                _logService.LogInformation("Comando encontrado: {CommandName}", item.Record.Name);
                textResult += item.Record.Definition + Environment.NewLine;
            }
            _logService.LogInformation("Búsqueda completada, resultados encontrados.");

            return textResult;

        }
        catch (Exception e)
        {
            _logService.LogError(e, "Error al realizar la búsqueda en la base de datos vectorial.");
            return null;

        }
    }

    private string GenerateCommandJson(HCommand command)
    {
        var definition = GetCommandDefinition(command);
        return JsonSerializer.Serialize(definition, JsonSerializerOptions);
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false,
        IncludeFields = true
    };

    private CommandDefinition GetCommandDefinition(HCommand command)
    {
        var definition = new CommandDefinition
        {
            CommandName = command.CommandKey,
            CommandDescription = command.Description,
            CommandRoles = command.Roles.ToArray(),
            Parameters = command.InNames?.Zip(command.InTypes ?? new List<Type>(), (name, type) =>
                new CommandParameterDefinition
                {
                    Name = name,
                    Type = type?.FullName ?? "Tipo desconocido"
                }).ToList() ?? new List<CommandParameterDefinition>()
        };
        return definition;
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingsFromTextAsync(string text, CancellationToken cancellationToken)
    {
        try
        {
            OpenAI.Embeddings.OpenAIEmbedding embeddingResult = await _openAiClient
                .GetEmbeddingClient("text-embedding-3-small")
                .GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
            return embeddingResult.ToFloats();

        }
        catch (Exception e)
        {
            _logService.LogError(e, "Error al generar embeddings para el texto.");
            return new ReadOnlyMemory<float>();
        }
    }
}


public class CommandVectorRecord
{
    [VectorStoreRecordKey]
    public int Id { get; set; }
    [VectorStoreRecordData]
    public string Name { get; set; } = null!;
    [VectorStoreRecordData]
    public string Definition { get; set; } = null!;
    [VectorStoreRecordData]
    [VectorStoreRecordVector(1536)]
    public ReadOnlyMemory<float> Embedding { get; set; }
}

public class ActiveCommandDefinition
{
    public string Name { get; set; } = null!;
    public string Definition { get; set; } = null!;
}