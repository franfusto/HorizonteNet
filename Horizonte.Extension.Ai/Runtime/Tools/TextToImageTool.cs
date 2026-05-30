using Horizonte.Extension.Ai.Contracts;
using Horizonte.Extension.Ai.Definitions;
using Horizonte.Extension.Ai.Interfaces;
using Horizonte.Extension.Ai.Runtime.Files;

namespace Horizonte.Extension.Ai.Runtime.Tools;

public sealed class TextToImageTool : IHagentTool
{
    private static readonly HashSet<string> SupportedOutputMimeTypes = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg"
    };

    private readonly AgentToolDef _toolDef;
    private readonly IHagentTextToImageClient _client;

    public TextToImageTool(
        AgentToolDef toolDef,
        IHagentTextToImageClient client)
    {
        ArgumentNullException.ThrowIfNull(toolDef);
        ArgumentNullException.ThrowIfNull(client);

        _toolDef = toolDef;
        _client = client;

        Id = string.IsNullOrWhiteSpace(toolDef.Id)
            ? "text-to-image"
            : toolDef.Id;

        Name = string.IsNullOrWhiteSpace(toolDef.Name)
            ? "Text To Image"
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
               call.ToolType == AgentToolType.TextToImage &&
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

        if (!SupportedOutputMimeTypes.Contains(mimeType))
        {
            return ValueTask.FromResult(false);
        }

        return ValueTask.FromResult(IsOutputMimeTypeAllowed(
            mimeType,
            context.FileOptions));
    }

    public async Task<HagentToolResult> ExecuteAsync(
        HagentToolCall call,
        HagentToolContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(call);
        ArgumentNullException.ThrowIfNull(context);

        cancellationToken.ThrowIfCancellationRequested();

        if (!context.FileOptions.AllowOutputFiles)
        {
            return new HagentToolResult
            {
                Success = false,
                ErrorMessage = "La generación de archivos de salida no está permitida por AgentFileOptions."
            };
        }

        var mimeType = ResolveMimeType(call);

        if (!SupportedOutputMimeTypes.Contains(mimeType))
        {
            return new HagentToolResult
            {
                Success = false,
                ErrorMessage = $"TextToImageTool no soporta el MIME type de salida '{mimeType}'."
            };
        }

        if (!IsOutputMimeTypeAllowed(mimeType, context.FileOptions))
        {
            return new HagentToolResult
            {
                Success = false,
                ErrorMessage = $"El MIME type de salida '{mimeType}' no está permitido por AgentFileOptions."
            };
        }

        var prompt = ResolvePrompt(call, context);

        if (string.IsNullOrWhiteSpace(prompt))
        {
            return new HagentToolResult
            {
                Success = false,
                ErrorMessage = "TextToImageTool requiere un prompt no vacío."
            };
        }

        var request = new HagentTextToImageRequest
        {
            Prompt = prompt,
            MimeType = mimeType,
            Size = ResolveSize(call)
        };

        request.Options["toolId"] = Id;
        request.Options["toolType"] = ToolType.ToString();

        var generated = await _client.GenerateAsync(
            request,
            cancellationToken);

        if (generated.Images.Count == 0)
        {
            return new HagentToolResult
            {
                Success = false,
                ErrorMessage = "El cliente TextToImage no devolvió imágenes."
            };
        }

        var result = new HagentToolResult
        {
            Success = true,
            Text = $"TextToImageTool generó {generated.Images.Count} imagen(es).",
            ResponseItems =
            {
                new ChatResponseItem
                {
                    Type = ChatResponseItemType.Tool,
                    Content = $"TextToImageTool generó {generated.Images.Count} archivo(s) de imagen."
                }
            },
            Metadata =
            {
                ["imageCount"] = generated.Images.Count,
                ["mimeType"] = mimeType,
                ["prompt"] = prompt
            }
        };

        foreach (var image in generated.Images)
        {
            cancellationToken.ThrowIfCancellationRequested();

            result.Files.Add(new HagentFile
            {
                FileName = ResolveFileName(image.FileName, mimeType),
                MimeType = mimeType,
                Encoding = "binary",
                Content = image.Content,
                Metadata =
                {
                    ["toolId"] = Id,
                    ["toolType"] = ToolType.ToString(),
                    ["generatedBy"] = nameof(TextToImageTool),
                    ["prompt"] = prompt
                }
            });
        }

        context.Items[$"tool.{Id}.generatedImages"] = result.Files.ToArray();

        return result;
    }

    private bool IsAutoUseEnabled()
    {
        if (!_toolDef.Settings.TryGetValue("autoUse", out var value))
        {
            return false;
        }

        return value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("always", StringComparison.OrdinalIgnoreCase);
    }

    private string ResolvePrompt(
        HagentToolCall call,
        HagentToolContext context)
    {
        if (call.Arguments.TryGetValue("prompt", out var argumentPrompt) &&
            argumentPrompt is not null &&
            !string.IsNullOrWhiteSpace(argumentPrompt.ToString()))
        {
            return argumentPrompt.ToString()!;
        }

        if (_toolDef.Settings.TryGetValue("prompt", out var settingPrompt) &&
            !string.IsNullOrWhiteSpace(settingPrompt))
        {
            return settingPrompt;
        }

        return context.UserQuery;
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

        if (_toolDef.Settings.TryGetValue("outputFormat", out var outputFormat) &&
            !string.IsNullOrWhiteSpace(outputFormat))
        {
            return outputFormat.Equals("jpg", StringComparison.OrdinalIgnoreCase) ||
                   outputFormat.Equals("jpeg", StringComparison.OrdinalIgnoreCase)
                ? "image/jpeg"
                : "image/png";
        }

        return "image/png";
    }

    private string ResolveSize(HagentToolCall call)
    {
        if (call.Arguments.TryGetValue("size", out var argumentSize) &&
            argumentSize is not null &&
            !string.IsNullOrWhiteSpace(argumentSize.ToString()))
        {
            return argumentSize.ToString()!;
        }

        if (_toolDef.Settings.TryGetValue("size", out var settingSize) &&
            !string.IsNullOrWhiteSpace(settingSize))
        {
            return settingSize;
        }

        return "1024x1024";
    }

    private static string ResolveFileName(
        string fileName,
        string mimeType)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            return fileName;
        }

        return mimeType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase)
            ? "generated-image.jpg"
            : "generated-image.png";
    }

    private static bool IsOutputMimeTypeAllowed(
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