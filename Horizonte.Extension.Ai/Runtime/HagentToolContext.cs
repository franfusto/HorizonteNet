using Horizonte.Extension.Ai.Definitions;
using Horizonte.Extension.Ai.Runtime.Files;

namespace Horizonte.Extension.Ai.Runtime;

public sealed class HagentToolContext
{
    public required string AgentId { get; init; }
    public required string UserQuery { get; init; }
    public required IReadOnlyList<HagentFile> InputFiles { get; init; }
    public required IReadOnlyList<HagentFile> OutputFiles { get; init; }
    public required AgentFileOptions FileOptions { get; init; }
    public required AgentExecutionOptions ExecutionOptions { get; init; }

    public Dictionary<string, object?> Items { get; init; } = new();
}
