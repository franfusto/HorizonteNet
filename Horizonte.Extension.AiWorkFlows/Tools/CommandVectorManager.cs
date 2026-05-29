using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OpenAI;

namespace Horizonte.Extension.AiWorkFlows.Tools;

public class CommandVectorManager
{
    private readonly IHGesCom _commandManager;
    private readonly ILogger _logService;
    private readonly OpenAIClient _openAiClient;

    // Constantes y campos privados
    private const string VectorCollectionName = "commands";
    private const string CacheFilePath = "cache_commands.csv";
    private List<CommandVectorRecord> _commandDefinitions = new();
    private List<ActiveCommandDefinition> _activeCommands = new();
    private Microsoft.Extensions.VectorData.VectorStoreCollection<int, CommandVectorRecord>? _commandVectorCollection;
    private readonly string _embedderModel;

    /// <summary>
    /// La clase CommandVectorManager es responsable de gestionar el almacenamiento y la búsqueda de comandos
    /// utilizando vectores de embeddings generados por un modelo de IA.
    /// Se comunica con los servicios OpenAI y utiliza un modelo especificado para generar los embeddings.
    /// </summary>
    public CommandVectorManager(IServiceProvider serviceProvider, OpenAIClient openAiClient, string embedderModel)
    {
        _embedderModel = embedderModel;
        _commandManager = serviceProvider.GetService<IHGesCom>()!;
        _logService = serviceProvider.GetService<ILoggerFactory>()!.CreateLogger<CommandVectorManager>();
        _openAiClient = openAiClient;
        //Task.Run(InitializeManagerAsync);
        //_logService.LogInformation("Inicializando CommandVectorManager...");
    }

    private void TestSerializer()
    {
        try
        {
            _commandDefinitions = new List<CommandVectorRecord>();
            _commandDefinitions.Add(new CommandVectorRecord()
            {
                Id = 1,
                Definition = "xx",
                Embedding = new ReadOnlyMemory<float>(),
                Name = "xxwww"
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    // Método público principal
    /// <summary>
    /// Inicializa el administrador de vectores de comandos realizando una serie de procesos asincrónicos,
    /// incluyendo la carga de la caché desde un archivo, la carga de definiciones de comandos activos,
    /// la validación de la consistencia de la caché, la serialización de la caché y la población del almacenamiento de vectores.
    /// Captura y registra errores críticos durante la inicialización.
    /// </summary>
    /// <returns>Un <c>Task</c> que representa la operación asincrónica de inicialización.</returns>
    public async Task InitializeManagerAsync()
    {
        try
        {
            _logService.LogInformation("Iniciando proceso de inicialización del administrador...");

            ////
            //TestSerializer();
            //SerializeCache();
            /////

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

    /// <summary>
    /// Método asincrónico que valida la consistencia del caché de comandos en el sistema.
    /// Revisa cada definición de comando activa y actualiza o añade las definiciones según sea necesario.
    /// Registra información sobre el proceso de validación y maneja cualquier excepción que pueda ocurrir,
    /// registrando un error en caso de fallos durante la validación.
    /// </summary>
    /// <returns>Una tarea que representa la operación asincrónica de validación del caché.</returns>
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
            _logService.LogInformation("Serializando el cache de comandos en formato CSV...");

            var csvBuilder = new System.Text.StringBuilder();
            // Encabezados
            csvBuilder.AppendLine("Id;Name;Definition;Embedding");

            foreach (var record in _commandDefinitions)
            {
                // Convertimos el ReadOnlyMemory<float> a formato [v1,v2,v3...]
                var embeddingArray = record.Embedding.ToArray();
                string embeddingString = $"[{string.Join(",", embeddingArray)}]";

                // Escapamos los campos de texto por si contienen puntos y coma o comillas
                string safeName = record.Name?.Replace("\"", "\"\"") ?? "";
                string safeDefinition =
                    record.Definition?.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "") ?? "";

                csvBuilder.AppendLine($"{record.Id};\"{safeName}\";\"{safeDefinition}\";\"{embeddingString}\"");
            }

            File.WriteAllText(CacheFilePath, csvBuilder.ToString());
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
            if (existingCommand != null || command.Definition != existingCommand!.Definition)
            {
                _commandDefinitions.Remove(existingCommand);
                _commandDefinitions.Add(new CommandVectorRecord
                {
                    Name = command.Name,
                    Definition = command.Definition,
                    Embedding = await GenerateEmbeddingsFromTextAsync($"{command.Name} => {command.Definition}",
                        CancellationToken.None)
                });
                _logService.LogInformation("Comando {CommandName} añadido o actualizado en el cache.", command.Name);
            }
        }
        catch (Exception e)
        {
            _logService.LogError(e, "Error al actualizar o agregar la definición del comando {CommandName}.",
                command.Name);
        }
    }


    /// <summary>
    /// Deserializa los registros de comando desde un archivo CSV de caché.
    /// Las líneas del archivo deben estar separadas por ';' y los campos pueden estar encapsulados en comillas.
    /// Asume que el archivo contiene un encabezado y realiza la limpieza de comillas en los campos.
    /// En caso de error durante la deserialización, se registra el error y se devuelve una lista vacía de registros.
    /// </summary>
    /// <returns>Una lista de objetos CommandVectorRecord que representan los comandos deserializados desde el archivo de caché.</returns>
    public List<CommandVectorRecord> DeserializeCacheFromFile()
    {
        try
        {
            string csvPath = CacheFilePath.Replace(".json", ".csv");
            if (!File.Exists(csvPath)) return new List<CommandVectorRecord>();

            _logService.LogInformation("Deserializando el cache de comandos desde CSV...");

            var records = new List<CommandVectorRecord>();
            var lines = File.ReadAllLines(csvPath);

            // Saltamos el encabezado (Id;Name;Definition;Embedding)
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Separamos por ';' teniendo en cuenta que los campos están entre comillas
                // Una forma simple si no hay ';' dentro de las comillas, sino usar un parser más robusto
                var parts = line.Split(';');
                if (parts.Length < 4) continue;

                var record = new CommandVectorRecord();

                // 1. Id
                if (int.TryParse(parts[0], out int id)) record.Id = id;

                // 2. Name (limpiamos comillas)
                record.Name = parts[1].Trim('"').Replace("\"\"", "\"");

                // 3. Definition (limpiamos comillas)
                record.Definition = parts[2].Trim('"').Replace("\"\"", "\"");

                // 4. Embedding (formato [1,2,3...])
                string embRaw = parts[3].Trim('"').Trim('[', ']');
                if (!string.IsNullOrEmpty(embRaw))
                {
                    var floatArray = embRaw.Split(',')
                        .Select(s => float.TryParse(s, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float f)
                            ? f
                            : 0f)
                        .ToArray();
                    record.Embedding = new ReadOnlyMemory<float>(floatArray);
                }

                records.Add(record);
            }

            return records;
        }
        catch (Exception e)
        {
            _logService.LogError(e, "Error al deserializar el archivo CSV de cache.");
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
                .EnsureCollectionExistsAsync();

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

    /// <summary>
    /// Busca un comando en el almacén de vectores utilizando el texto de consulta proporcionado.
    /// Genera embeddings a partir del texto y realiza una búsqueda en la colección de vectores
    /// para encontrar comandos relacionados. Devuelve los resultados de búsqueda en forma de cadena.
    /// </summary>
    /// <param name="queryText">Texto de consulta utilizado para buscar en el almacén de vectores.</param>
    /// <param name="cancellationToken">Token de cancelación para detener la operación de búsqueda si es necesario.</param>
    /// <return>Una cadena con los resultados de los comandos encontrados o null en caso de error durante la búsqueda.</return>
    public async Task<string?> SearchCommandInVectorStoreAsync(string queryText, CancellationToken cancellationToken)
    {
        _logService.LogInformation("Buscando comandos en la base de datos vectorial para la consulta: {QueryText}.",
            queryText);
        try
        {
            string textResult = string.Empty;
            var query = await GenerateEmbeddingsFromTextAsync(queryText, cancellationToken);
            var searchResults = _commandVectorCollection!.SearchAsync(query, 5);

            foreach (var item in searchResults.ToBlockingEnumerable())
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

    public async Task<string?> SearchCommandInVectorStoreAsync222(string catalog, string queryText,
        CancellationToken cancellationToken)
    {
        _logService.LogInformation("Buscando comandos en la base de datos vectorial para la consulta: {QueryText}.",
            queryText);
        try
        {
            string textResult = string.Empty;
            var query = await GenerateEmbeddingsFromTextAsync(queryText, cancellationToken);
            var searchResults =
                _commandVectorCollection!.SearchAsync(query, 5); //VectorizedSearchAsync(query, new() { Top = 5 });

            foreach (var item in searchResults.ToBlockingEnumerable())
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

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingsFromTextAsync(string text,
        CancellationToken cancellationToken)
    {
        try
        {
            OpenAI.Embeddings.OpenAIEmbedding embeddingResult = await _openAiClient
                //.GetEmbeddingClient("text-embedding-3-small")
                .GetEmbeddingClient(_embedderModel)
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
    [VectorStoreKey] public int Id { get; set; }

    [VectorStoreData] public string Name { get; set; } = string.Empty;

    [VectorStoreData] public string Definition { get; set; } = string.Empty;

    [VectorStoreVector(1536)] public ReadOnlyMemory<float> Embedding { get; set; }
}

public class ActiveCommandDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
}

public class CommandDefinition
{
    public string CommandName = string.Empty;
    public string CommandDescription = string.Empty;
    public string[] CommandRoles = Array.Empty<string>();
    public List<CommandParameterDefinition> Parameters = new List<CommandParameterDefinition>();
}

public class CommandParameterDefinition
{
    public string Name = string.Empty;
    public string Type = string.Empty;
}