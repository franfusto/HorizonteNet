using ConsoleApp1.Definitions;
using ConsoleApp1.Runtime;
using ConsoleApp1.Runtime.Tools;

namespace ConsoleApp1.Interfaces;

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
