using System.Text;
using ConsoleApp1.Contracts;
using ConsoleApp1.Definitions;
using ConsoleApp1.Interfaces;
using ConsoleApp1.Runtime.Files;

namespace ConsoleApp1.Runtime.Tools;

public sealed class ImageToTextTool : IHagentTool
{
    private static readonly HashSet<string> DefaultSupportedMimeTypes = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/webp"
    };

    private readonly AgentToolDef _toolDef;
    private readonly IHagentImageToTextClient _client;

    public ImageToTextTool(
        AgentToolDef toolDef,
        IHagentImageToTextClient client)
    {
        ArgumentNullException.ThrowIfNull(toolDef);
        ArgumentNullException.ThrowIfNull(client);

        _toolDef = toolDef;
        _client = client;

        Id = string.IsNullOrWhiteSpace(toolDef.Id)
            ? "image-to-text"
            : toolDef.Id;

        Name = string.IsNullOrWhiteSpace(toolDef.Name)
            ? "Image To Text"
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
               call.ToolType == AgentToolType.ImageToText &&
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

        if (!context.FileOptions.AutoProcessImages && !HasExplicitAutoUseMimeTypes())
        {
            return ValueTask.FromResult(false);
        }

        var images = GetSupportedImages(context.InputFiles);

        return ValueTask.FromResult(images.Count > 0);
    }

    public async Task<HagentToolResult> ExecuteAsync(
        HagentToolCall call,
        HagentToolContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(call);
        ArgumentNullException.ThrowIfNull(context);

        cancellationToken.ThrowIfCancellationRequested();

        var images = GetSupportedImages(context.InputFiles);

        if (images.Count == 0)
        {
            return new HagentToolResult
            {
                Success = true,
                Text = "ImageToTextTool no encontró imágenes compatibles para analizar.",
                Metadata =
                {
                    ["imageCount"] = 0
                }
            };
        }

        var prompt = ResolvePrompt(call);

        var request = new HagentImageToTextRequest
        {
            Prompt = prompt
        };

        request.Images.AddRange(images);
        request.Options["toolId"] = Id;
        request.Options["toolType"] = ToolType.ToString();

        var clientResult = await _client.DescribeAsync(
            request,
            cancellationToken);

        var description = BuildDescription(images, clientResult.Text);

        context.Items["image.descriptions"] = description;

        return new HagentToolResult
        {
            Success = true,
            Text = description,
            ResponseItems =
            {
                new ChatResponseItem
                {
                    Type = ChatResponseItemType.Tool,
                    Content = $"ImageToTextTool analizó {images.Count} imagen(es)."
                }
            },
            Metadata =
            {
                ["imageCount"] = images.Count,
                ["provider"] = clientResult.Metadata.TryGetValue("provider", out var provider)
                    ? provider
                    : null
            }
        };
    }

    private string ResolvePrompt(HagentToolCall call)
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

        return "Describe la imagen de forma clara, útil y estructurada.";
    }

    private bool HasExplicitAutoUseMimeTypes()
    {
        return _toolDef.Settings.TryGetValue("autoUseForMimeTypes", out var value) &&
               !string.IsNullOrWhiteSpace(value);
    }

    private IReadOnlyList<HagentFile> GetSupportedImages(
        IReadOnlyList<HagentFile> files)
    {
        var supportedMimeTypes = ResolveSupportedMimeTypes();

        return files
            .Where(file => supportedMimeTypes.Contains(file.MimeType))
            .ToArray();
    }

    private HashSet<string> ResolveSupportedMimeTypes()
    {
        if (!_toolDef.Settings.TryGetValue("autoUseForMimeTypes", out var value) ||
            string.IsNullOrWhiteSpace(value))
        {
            return new HashSet<string>(
                DefaultSupportedMimeTypes,
                StringComparer.OrdinalIgnoreCase);
        }

        var configuredMimeTypes = value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(mimeType => !string.IsNullOrWhiteSpace(mimeType));

        return new HashSet<string>(
            configuredMimeTypes,
            StringComparer.OrdinalIgnoreCase);
    }

    private static string BuildDescription(
        IReadOnlyList<HagentFile> images,
        string text)
    {
        var builder = new StringBuilder();

        builder.AppendLine("Análisis de imágenes:");
        builder.AppendLine();

        foreach (var image in images)
        {
            builder.Append("- ");
            builder.Append(image.FileName);
            builder.Append(" (");
            builder.Append(image.MimeType);
            builder.Append(", ");
            builder.Append(image.Content.Length);
            builder.AppendLine(" bytes)");
        }

        builder.AppendLine();

        if (string.IsNullOrWhiteSpace(text))
        {
            builder.AppendLine("El cliente de visión no devolvió descripción.");
        }
        else
        {
            builder.AppendLine(text);
        }

        return builder.ToString();
    }
}