using System.Text;
using Horizonte.Extension.Ai.Contracts;
using Horizonte.Extension.Ai.Definitions;
using Horizonte.Extension.Ai.Interfaces;
using Horizonte.Extension.Ai.Runtime.Files;

namespace Horizonte.Extension.Ai.Runtime.Tools;

public sealed class FileReaderTool : IHagentTool
{
    private static readonly HashSet<string> SupportedMimeTypes = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "text/plain",
        "application/json",
        "text/markdown"
    };

    private readonly IReadOnlyDictionary<string, string> _settings;

    public FileReaderTool(AgentToolDef toolDef)
    {
        ArgumentNullException.ThrowIfNull(toolDef);

        Id = string.IsNullOrWhiteSpace(toolDef.Id)
            ? "file-reader"
            : toolDef.Id;

        Name = string.IsNullOrWhiteSpace(toolDef.Name)
            ? "File Reader"
            : toolDef.Name;

        ToolType = toolDef.ToolType;
        Enabled = toolDef.Enabled;
        _settings = toolDef.Settings;
    }

    public string Id { get; }

    public string Name { get; }

    public AgentToolType ToolType { get; }

    public bool Enabled { get; }

    public bool CanHandle(HagentToolCall call)
    {
        ArgumentNullException.ThrowIfNull(call);

        return Enabled &&
               call.ToolType == AgentToolType.FileReader &&
               string.Equals(call.ToolId, Id, StringComparison.OrdinalIgnoreCase);
    }

    public ValueTask<bool> ShouldAutoInvokeAsync(
        HagentToolContext context,
        CancellationToken cancellationToken = default)
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

        var hasSupportedFiles = context.InputFiles.Any(IsSupportedFile);

        return ValueTask.FromResult(hasSupportedFiles);
    }


    public Task<HagentToolResult> ExecuteAsync(
        HagentToolCall call,
        HagentToolContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(call);
        ArgumentNullException.ThrowIfNull(context);

        cancellationToken.ThrowIfCancellationRequested();

        var supportedFiles = context.InputFiles
            .Where(IsSupportedFile)
            .ToArray();

        if (supportedFiles.Length == 0)
        {
            return Task.FromResult(new HagentToolResult
            {
                Success = true,
                Text = "FileReaderTool no encontró archivos compatibles para extraer texto."
            });
        }

        var builder = new StringBuilder();

        foreach (var file in supportedFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var text = ExtractText(file);

            builder.AppendLine($"## Archivo: {file.FileName}");
            builder.AppendLine($"MIME: {file.MimeType}");
            builder.AppendLine();
            builder.AppendLine(text);
            builder.AppendLine();
        }

        var extractedText = builder.ToString();

        context.Items["file.extractedText"] = extractedText;

        return Task.FromResult(new HagentToolResult
        {
            Success = true,
            Text = extractedText,
            ResponseItems =
            {
                new ChatResponseItem
                {
                    Type = ChatResponseItemType.Tool,
                    Content = $"FileReaderTool extrajo texto de {supportedFiles.Length} archivo(s)."
                }
            }
        });
    }

    private bool IsAutoUseEnabled()
    {
        return _settings.TryGetValue("autoUse", out var value) &&
               bool.TryParse(value, out var enabled) &&
               enabled;
    }

    private static bool IsSupportedFile(HagentFile file)
    {
        return SupportedMimeTypes.Contains(file.MimeType);
    }

    private static string ExtractText(HagentFile file)
    {
        if (file.Content.Length == 0)
        {
            return string.Empty;
        }

        var encoding = ResolveEncoding(file);

        return encoding.GetString(file.Content);
    }

    private static Encoding ResolveEncoding(HagentFile file)
    {
        if (string.IsNullOrWhiteSpace(file.Encoding))
        {
            return Encoding.UTF8;
        }

        try
        {
            return Encoding.GetEncoding(file.Encoding);
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
    }
}