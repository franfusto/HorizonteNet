using Horizonte.Extension.Ai.Definitions;
using Horizonte.Extension.Ai.Interfaces;

namespace Horizonte.Extension.Ai.Runtime.Tools;

internal sealed class StubHagentImageToTextClient : IHagentImageToTextClient
{
    private readonly ClientDef _clientDef;
    private readonly ServerDef _serverDef;

    public StubHagentImageToTextClient(
        ClientDef clientDef,
        ServerDef serverDef)
    {
        ArgumentNullException.ThrowIfNull(clientDef);
        ArgumentNullException.ThrowIfNull(serverDef);

        _clientDef = clientDef;
        _serverDef = serverDef;
    }

    public Task<HagentImageToTextResult> DescribeAsync(
        HagentImageToTextRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var result = new HagentImageToTextResult
        {
            Text = request.Images.Count == 0
                ? "No se recibieron imágenes para analizar."
                : $"StubHagentImageToTextClient recibió {request.Images.Count} imagen(es)."
        };

        result.Metadata["clientId"] = _clientDef.Id;
        result.Metadata["model"] = _clientDef.Model;
        result.Metadata["serverId"] = _serverDef.Id;
        result.Metadata["provider"] = "stub";

        return Task.FromResult(result);
    }
}