using Horizonte.Extension.Ai.Contracts;
using Horizonte.Extension.Ai.Runtime.Files;

namespace Horizonte.Extension.Ai.Runtime.Tools;

public sealed class HagentToolResult
{
    public bool Success { get; init; } = true;
    public string? Text { get; init; }
    public string? ErrorMessage { get; init; }

    public List<HagentFile> Files { get; init; } = [];
    public List<ChatResponseItem> ResponseItems { get; init; } = [];

    public Dictionary<string, object?> Metadata { get; init; } = new();
}
