using System.Text;
using System.Text.Json;
using ConsoleApp1.Contracts;
using ConsoleApp1.Definitions;
using ConsoleApp1.Interfaces;

namespace ConsoleApp1.Runtime.Tools;

public sealed class RagSearchTool : IHagentTool
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "al", "algo", "and", "ante", "as", "con", "de", "del", "dime",
        "el", "en", "es", "for", "la", "las", "lo", "los", "me", "of",
        "on", "or", "para", "por", "que", "sobre", "the", "to", "tu", "un",
        "una", "y"
    };

    private readonly AgentToolDef _toolDef;
    private readonly RaGdef _ragDef;

    public RagSearchTool(
        AgentToolDef toolDef,
        RaGdef ragDef)
    {
        ArgumentNullException.ThrowIfNull(toolDef);
        ArgumentNullException.ThrowIfNull(ragDef);

        _toolDef = toolDef;
        _ragDef = ragDef;

        Id = string.IsNullOrWhiteSpace(toolDef.Id)
            ? "rag-search"
            : toolDef.Id;

        Name = string.IsNullOrWhiteSpace(toolDef.Name)
            ? "RAG Search"
            : toolDef.Name;

        ToolType = toolDef.ToolType;
        Enabled = toolDef.Enabled;
    }

    public string Id { get; }

    public string Name { get; }

    public AgentToolType ToolType { get; }

    public bool Enabled { get; }

    public bool CanHandle(HagentToolCall call)
    {
        ArgumentNullException.ThrowIfNull(call);

        return Enabled &&
               call.ToolType == AgentToolType.RagSearch &&
               (string.Equals(call.ToolId, Id, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(call.ToolId));
    }

    public ValueTask<bool> ShouldAutoInvokeAsync(
        HagentToolContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        cancellationToken.ThrowIfCancellationRequested();

        if (!Enabled)
        {
            return ValueTask.FromResult(false);
        }

        if (!IsAutoUseEnabled())
        {
            return ValueTask.FromResult(false);
        }

        if (_ragDef.RagType != RagType.InMemory)
        {
            return ValueTask.FromResult(false);
        }

        if (string.IsNullOrWhiteSpace(context.UserQuery))
        {
            return ValueTask.FromResult(false);
        }

        var documents = LoadDocuments();

        return ValueTask.FromResult(documents.Count > 0);
    }

    public Task<HagentToolResult> ExecuteAsync(
        HagentToolCall call,
        HagentToolContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(call);
        ArgumentNullException.ThrowIfNull(context);

        cancellationToken.ThrowIfCancellationRequested();

        if (_ragDef.RagType != RagType.InMemory)
        {
            return Task.FromResult(new HagentToolResult
            {
                Success = false,
                ErrorMessage = $"RagSearchTool solo soporta '{RagType.InMemory}' en esta primera versión."
            });
        }

        var query = ResolveQuery(call, context);

        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult(new HagentToolResult
            {
                Success = false,
                ErrorMessage = "RagSearchTool no recibió una consulta válida."
            });
        }

        var documents = LoadDocuments();

        if (documents.Count == 0)
        {
            return Task.FromResult(new HagentToolResult
            {
                Success = true,
                Text = "RagSearchTool no encontró documentos configurados en memoria.",
                Metadata =
                {
                    ["ragId"] = _ragDef.Id,
                    ["ragType"] = _ragDef.RagType.ToString(),
                    ["hitCount"] = 0
                }
            });
        }

        var hits = Search(
            query,
            documents,
            ResolveTopK(call),
            ResolveMinScore(call));

        if (hits.Count == 0)
        {
            context.Items["rag.context"] = string.Empty;

            return Task.FromResult(new HagentToolResult
            {
                Success = true,
                Text = "RagSearchTool no encontró contexto relevante para la consulta.",
                ResponseItems =
                {
                    new ChatResponseItem
                    {
                        Type = ChatResponseItemType.Tool,
                        Content = "RagSearchTool no encontró documentos relevantes."
                    }
                },
                Metadata =
                {
                    ["ragId"] = _ragDef.Id,
                    ["ragType"] = _ragDef.RagType.ToString(),
                    ["hitCount"] = 0
                }
            });
        }

        var ragContext = RenderContext(query, hits);

        context.Items["rag.context"] = ragContext;
        context.Items["rag.hits"] = hits;

        return Task.FromResult(new HagentToolResult
        {
            Success = true,
            Text = ragContext,
            ResponseItems =
            {
                new ChatResponseItem
                {
                    Type = ChatResponseItemType.Tool,
                    Content = $"RagSearchTool recuperó {hits.Count} fragmento(s) de contexto."
                }
            },
            Metadata =
            {
                ["ragId"] = _ragDef.Id,
                ["ragType"] = _ragDef.RagType.ToString(),
                ["collectionName"] = _ragDef.CollectionName,
                ["hitCount"] = hits.Count
            }
        });
    }

    private bool IsAutoUseEnabled()
    {
        if (!_toolDef.Settings.TryGetValue("autoUse", out var value))
        {
            return false;
        }

        return value.Equals("always", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveQuery(
        HagentToolCall call,
        HagentToolContext context)
    {
        if (call.Arguments.TryGetValue("query", out var argumentQuery) &&
            argumentQuery is not null &&
            !string.IsNullOrWhiteSpace(argumentQuery.ToString()))
        {
            return argumentQuery.ToString()!;
        }

        return context.UserQuery;
    }

    private int ResolveTopK(HagentToolCall call)
    {
        if (call.Arguments.TryGetValue("topK", out var argumentTopK) &&
            int.TryParse(argumentTopK?.ToString(), out var topKFromCall) &&
            topKFromCall > 0)
        {
            return topKFromCall;
        }

        return _ragDef.TopK > 0
            ? _ragDef.TopK
            : 5;
    }

    private double ResolveMinScore(HagentToolCall call)
    {
        if (call.Arguments.TryGetValue("minScore", out var argumentMinScore) &&
            double.TryParse(argumentMinScore?.ToString(), out var minScoreFromCall))
        {
            return Math.Clamp(minScoreFromCall, 0d, 1d);
        }

        return Math.Clamp((double)_ragDef.MinScore, 0d, 1d);
    }

    private IReadOnlyList<RagMemoryDocument> LoadDocuments()
    {
        var documents = new List<RagMemoryDocument>();

        LoadDocumentsFromJsonSetting(documents);
        LoadDocumentsFromKeyValueSettings(documents);
        LoadSingleDocumentSetting(documents);

        return documents
            .Where(document => !string.IsNullOrWhiteSpace(document.Text))
            .GroupBy(document => document.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
    }

    private void LoadDocumentsFromJsonSetting(List<RagMemoryDocument> documents)
    {
        if (!_ragDef.Settings.TryGetValue("documents", out var json) ||
            string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(json);

            if (jsonDocument.RootElement.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var item in jsonDocument.RootElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var id = TryGetString(item, "id") ?? Guid.NewGuid().ToString("N");
                var title = TryGetString(item, "title") ?? id;
                var text = TryGetString(item, "text") ??
                           TryGetString(item, "content") ??
                           string.Empty;

                documents.Add(new RagMemoryDocument(
                    id,
                    title,
                    text));
            }
        }
        catch (JsonException)
        {
            // Primera versión: si el JSON de documentos es inválido, simplemente no carga esa fuente.
            // La validación estricta puede añadirse después en HagentFactory.
        }
    }

    private void LoadDocumentsFromKeyValueSettings(List<RagMemoryDocument> documents)
    {
        foreach (var setting in _ragDef.Settings)
        {
            if (!setting.Key.StartsWith("document.", StringComparison.OrdinalIgnoreCase) ||
                !setting.Key.EndsWith(".text", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(setting.Value))
            {
                continue;
            }

            var id = setting.Key["document.".Length..^".text".Length];
            var titleKey = $"document.{id}.title";

            var title = _ragDef.Settings.TryGetValue(titleKey, out var configuredTitle) &&
                        !string.IsNullOrWhiteSpace(configuredTitle)
                ? configuredTitle
                : id;

            documents.Add(new RagMemoryDocument(
                id,
                title,
                setting.Value));
        }
    }

    private void LoadSingleDocumentSetting(List<RagMemoryDocument> documents)
    {
        if (!_ragDef.Settings.TryGetValue("documentText", out var text) ||
            string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var title = _ragDef.Settings.TryGetValue("documentTitle", out var configuredTitle) &&
                    !string.IsNullOrWhiteSpace(configuredTitle)
            ? configuredTitle
            : _ragDef.Name;

        documents.Add(new RagMemoryDocument(
            "documentText",
            title,
            text));
    }

    private IReadOnlyList<RagSearchHit> Search(
        string query,
        IReadOnlyList<RagMemoryDocument> documents,
        int topK,
        double minScore)
    {
        var queryTerms = Tokenize(query);

        if (queryTerms.Count == 0)
        {
            return [];
        }

        var maxContextItems = _ragDef.MaxContextItems > 0
            ? _ragDef.MaxContextItems
            : topK;

        var take = Math.Max(1, Math.Min(topK, maxContextItems));

        return documents
            .Select(document => new RagSearchHit(
                document,
                ScoreDocument(queryTerms, document)))
            .Where(hit => hit.Score >= minScore && hit.Score > 0)
            .OrderByDescending(hit => hit.Score)
            .ThenBy(hit => hit.Document.Title, StringComparer.OrdinalIgnoreCase)
            .Take(take)
            .ToArray();
    }

    private static double ScoreDocument(
        HashSet<string> queryTerms,
        RagMemoryDocument document)
    {
        var titleTerms = Tokenize(document.Title);
        var bodyTerms = Tokenize(document.Text);

        var bodyMatches = queryTerms.Count(bodyTerms.Contains);
        var titleMatches = queryTerms.Count(titleTerms.Contains);

        if (bodyMatches == 0 && titleMatches == 0)
        {
            return 0d;
        }

        var bodyScore = (double)bodyMatches / queryTerms.Count;
        var titleBonus = ((double)titleMatches / queryTerms.Count) * 0.2d;

        return Math.Clamp(bodyScore + titleBonus, 0d, 1d);
    }

    private string RenderContext(
        string query,
        IReadOnlyList<RagSearchHit> hits)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"RAG: {_ragDef.Name}");
        builder.AppendLine($"Colección: {_ragDef.CollectionName}");
        builder.AppendLine($"Consulta: {query}");
        builder.AppendLine();

        for (var index = 0; index < hits.Count; index++)
        {
            var hit = hits[index];

            builder.AppendLine($"[{index + 1}] {hit.Document.Title}");
            builder.AppendLine($"Score: {hit.Score:0.###}");
            builder.AppendLine();
            builder.AppendLine(TrimToContextSize(hit.Document.Text));
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private string TrimToContextSize(string text)
    {
        var maxLength = _ragDef.ChunkSize > 0
            ? _ragDef.ChunkSize
            : 1200;

        if (text.Length <= maxLength)
        {
            return text;
        }

        return text[..maxLength] + "…";
    }

    private static HashSet<string> Tokenize(string text)
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var builder = new StringBuilder();

        foreach (var character in text)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
                continue;
            }

            FlushTerm(builder, terms);
        }

        FlushTerm(builder, terms);

        return terms;
    }

    private static void FlushTerm(
        StringBuilder builder,
        HashSet<string> terms)
    {
        if (builder.Length == 0)
        {
            return;
        }

        var term = builder.ToString();
        builder.Clear();

        if (term.Length < 3)
        {
            return;
        }

        if (StopWords.Contains(term))
        {
            return;
        }

        terms.Add(term);
    }

    private static string? TryGetString(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : property.ToString();
    }

    private sealed record RagMemoryDocument(
        string Id,
        string Title,
        string Text);

    private sealed record RagSearchHit(
        RagMemoryDocument Document,
        double Score);
}