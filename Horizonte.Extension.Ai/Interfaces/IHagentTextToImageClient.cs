using Horizonte.Extension.Ai.Runtime.Files;

namespace Horizonte.Extension.Ai.Interfaces;

public interface IHagentTextToImageClient
{
    Task<HagentTextToImageResult> GenerateAsync(
        HagentTextToImageRequest request,
        CancellationToken cancellationToken);
}

public sealed class HagentTextToImageRequest
{
    public string Prompt { get; set; } = string.Empty;

    public string MimeType { get; set; } = "image/png";

    public string Size { get; set; } = "1024x1024";

    public Dictionary<string, object?> Options { get; } = [];
}

public sealed class HagentTextToImageResult
{
    public List<HagentGeneratedImage> Images { get; } = [];

    public Dictionary<string, object?> Metadata { get; } = [];
}

public sealed class HagentGeneratedImage
{
    public string FileName { get; set; } = "generated-image.png";

    public string MimeType { get; set; } = "image/png";

    public byte[] Content { get; set; } = [];

    public Dictionary<string, object?> Metadata { get; } = [];
}