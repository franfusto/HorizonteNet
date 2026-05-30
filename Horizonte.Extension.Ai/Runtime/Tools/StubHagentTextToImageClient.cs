using Horizonte.Extension.Ai.Definitions;
using Horizonte.Extension.Ai.Interfaces;

namespace Horizonte.Extension.Ai.Runtime.Tools;

internal sealed class StubHagentTextToImageClient : IHagentTextToImageClient
{
    private readonly ClientDef _clientDef;
    private readonly ServerDef _serverDef;

    public StubHagentTextToImageClient(
        ClientDef clientDef,
        ServerDef serverDef)
    {
        ArgumentNullException.ThrowIfNull(clientDef);
        ArgumentNullException.ThrowIfNull(serverDef);

        _clientDef = clientDef;
        _serverDef = serverDef;
    }

    public Task<HagentTextToImageResult> GenerateAsync(
        HagentTextToImageRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var result = new HagentTextToImageResult();

        result.Images.Add(new HagentGeneratedImage
        {
            FileName = ResolveFileName(request.MimeType),
            MimeType = request.MimeType,
            Content = CreatePlaceholderImageBytes(),
            Metadata =
            {
                ["prompt"] = request.Prompt,
                ["provider"] = "stub",
                ["model"] = _clientDef.Model,
                ["size"] = request.Size
            }
        });

        result.Metadata["clientId"] = _clientDef.Id;
        result.Metadata["model"] = _clientDef.Model;
        result.Metadata["serverId"] = _serverDef.Id;
        result.Metadata["provider"] = "stub";

        return Task.FromResult(result);
    }

    private static string ResolveFileName(string mimeType)
    {
        return mimeType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase)
            ? "generated-image.jpg"
            : "generated-image.png";
    }

    private static byte[] CreatePlaceholderImageBytes()
    {
        const string png1x1Transparent =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII=";

        return Convert.FromBase64String(png1x1Transparent);
    }
}