using Horizonte.Extension.Ai.Definitions;
using Horizonte.Extension.Ai.Runtime;
using Horizonte.Extension.Ai.Runtime.Tools;

namespace Horizonte.Extension.Ai.Interfaces;

internal interface IHagentTool
{
    string Id { get; }
    string Name { get; }
    AgentToolType ToolType { get; }
    bool Enabled { get; }

    bool CanHandle(HagentToolCall call);

    ValueTask<bool> ShouldAutoInvokeAsync(
        HagentToolContext context,
        CancellationToken cancellationToken);

    Task<HagentToolResult> ExecuteAsync(
        HagentToolCall call,
        HagentToolContext context,
        CancellationToken cancellationToken);
}
