using ConsoleApp1.Contracts;
using ConsoleApp1.Runtime.Files;

namespace ConsoleApp1.Runtime.Tools;

internal sealed class HagentToolResult
{
    public bool Success { get; init; } = true;
    public string? Text { get; init; }
    public string? ErrorMessage { get; init; }

    public List<HagentFile> Files { get; init; } = [];
    public List<ChatResponseItem> ResponseItems { get; init; } = [];

    public Dictionary<string, object?> Metadata { get; init; } = new();
}
