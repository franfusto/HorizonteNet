using System.Text;
using System.Text.Json;
using ConsoleApp1.Contracts;
using ConsoleApp1.Definitions;
using ConsoleApp1.Interfaces;
using ConsoleApp1.Runtime.Files;

namespace ConsoleApp1.Runtime.Tools;

public sealed class FileWriterTool : IHagentTool
{
    private static readonly HashSet<string> SupportedMimeTypes = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "text/plain",
        "application/json"
    };

    private readonly AgentToolDef _toolDef;

    public FileWriterTool(AgentToolDef toolDef)
    {
        ArgumentNullException.ThrowIfNull(toolDef);

        _toolDef = toolDef;

        Id = string.IsNullOrWhiteSpace(toolDef.Id)
            ? "file-writer"
            : toolDef.Id;

        Name = string.IsNullOrWhiteSpace(toolDef.Name)
            ? "File Writer"
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
               call.ToolType == AgentToolType.FileWriter &&
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

        if (!context.FileOptions.AllowOutputFiles)
        {
            return ValueTask.FromResult(false);
        }

        if (!IsAutoUseEnabled())
        {
            return ValueTask.FromResult(false);
        }

        var mimeType = ResolveMimeType(null);

        return ValueTask.FromResult(IsMimeTypeAllowed(mimeType, context.FileOptions));
    }

    public Task<HagentToolResult> ExecuteAsync(
        HagentToolCall call,
        HagentToolContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(call);
        ArgumentNullException.ThrowIfNull(context);

        cancellationToken.ThrowIfCancellationRequested();

        if (!context.FileOptions.AllowOutputFiles)
        {
            return Task.FromResult(new HagentToolResult
            {
                Success = false,
                ErrorMessage = "La generación de archivos de salida no está permitida por AgentFileOptions."
            });
        }

        var mimeType = ResolveMimeType(call);

        if (!SupportedMimeTypes.Contains(mimeType))
        {
            return Task.FromResult(new HagentToolResult
            {
                Success = false,
                ErrorMessage = $"FileWriterTool no soporta el MIME type '{mimeType}'."
            });
        }

        if (!IsMimeTypeAllowed(mimeType, context.FileOptions))
        {
            return Task.FromResult(new HagentToolResult
            {
                Success = false,
                ErrorMessage = $"El MIME type de salida '{mimeType}' no está permitido por AgentFileOptions."
            });
        }

        var fileName = ResolveFileName(call, mimeType);
        var content = ResolveContent(call, context, mimeType);
        var bytes = Encoding.UTF8.GetBytes(content);

        var file = new HagentFile
        {
            FileName = fileName,
            MimeType = mimeType,
            Encoding = "utf-8",
            Content = bytes,
            Metadata =
            {
                ["toolId"] = Id,
                ["toolType"] = ToolType.ToString(),
                ["generatedBy"] = nameof(FileWriterTool)
            }
        };

        return Task.FromResult(new HagentToolResult
        {
            Success = true,
            Text = $"FileWriterTool generó el archivo '{fileName}' ({mimeType}).",
            Files =
            {
                file
            },
            ResponseItems =
            {
                new ChatResponseItem
                {
                    Type = ChatResponseItemType.Tool,
                    Content = $"FileWriterTool preparó el archivo de salida '{fileName}'."
                }
            },
            Metadata =
            {
                ["fileName"] = fileName,
                ["mimeType"] = mimeType,
                ["sizeBytes"] = bytes.Length
            }
        });
    }

    private bool IsAutoUseEnabled()
    {
        return _toolDef.Settings.TryGetValue("autoUse", out var value) &&
               bool.TryParse(value, out var enabled) &&
               enabled;
    }

    private string ResolveMimeType(HagentToolCall? call)
    {
        if (call?.Arguments.TryGetValue("mimeType", out var argumentMimeType) == true &&
            argumentMimeType is not null &&
            !string.IsNullOrWhiteSpace(argumentMimeType.ToString()))
        {
            return argumentMimeType.ToString()!;
        }

        if (_toolDef.Settings.TryGetValue("defaultMimeType", out var settingMimeType) &&
            !string.IsNullOrWhiteSpace(settingMimeType))
        {
            return settingMimeType;
        }

        return "text/plain";
    }

    private string ResolveFileName(HagentToolCall call, string mimeType)
    {
        if (call.Arguments.TryGetValue("fileName", out var argumentFileName) &&
            argumentFileName is not null &&
            !string.IsNullOrWhiteSpace(argumentFileName.ToString()))
        {
            return argumentFileName.ToString()!;
        }

        if (_toolDef.Settings.TryGetValue("fileName", out var settingFileName) &&
            !string.IsNullOrWhiteSpace(settingFileName))
        {
            return settingFileName;
        }

        return mimeType.Equals("application/json", StringComparison.OrdinalIgnoreCase)
            ? "output.json"
            : "output.txt";
    }

    private string ResolveContent(
        HagentToolCall call,
        HagentToolContext context,
        string mimeType)
    {
        if (call.Arguments.TryGetValue("content", out var argumentContent) &&
            argumentContent is not null)
        {
            return FormatContent(argumentContent, mimeType);
        }

        if (_toolDef.Settings.TryGetValue("content", out var settingContent))
        {
            return FormatContent(settingContent, mimeType);
        }

        if (_toolDef.Settings.TryGetValue("sourceItemKey", out var sourceItemKey) &&
            context.Items.TryGetValue(sourceItemKey, out var sourceValue) &&
            sourceValue is not null)
        {
            return FormatContent(sourceValue, mimeType);
        }

        return mimeType.Equals("application/json", StringComparison.OrdinalIgnoreCase)
            ? JsonSerializer.Serialize(new
            {
                query = context.UserQuery
            })
            : context.UserQuery;
    }

    private static string FormatContent(object value, string mimeType)
    {
        if (value is string text)
        {
            return text;
        }

        if (mimeType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Serialize(value);
        }

        return value.ToString() ?? string.Empty;
    }

    private static bool IsMimeTypeAllowed(
        string mimeType,
        AgentFileOptions fileOptions)
    {
        if (fileOptions.AllowedOutputMimeTypes.Length == 0)
        {
            return true;
        }

        return fileOptions.AllowedOutputMimeTypes.Contains(
            mimeType,
            StringComparer.OrdinalIgnoreCase);
    }
}