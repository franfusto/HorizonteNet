using Horizonte;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace Horizonte.Extension.WorkFlows;

public static class Extensions
{
    public static Workflow Build(this WorkFlowDef def, IHorizonteEnv env)
    {
        var gesCom = env.HHost.Services.GetService<IHGesCom>();
        if (gesCom == null)
        {
            throw new InvalidOperationException("IHGesCom service not found.");
        }

        // 1. Find the starting node (In type)
        var startNode = def.Nodes.FirstOrDefault(n => n.Type == WorkFlowNodeTypes.In);
        if (startNode == null)
        {
            throw new InvalidOperationException("Workflow definition must have an 'In' node.");
        }

        // 2. Create executors for each node
        var executors = def.Nodes.ToDictionary(
            node => node.Id,
            node => new HGesComExecutor(node.Id, node.CommandAction, gesCom)
        );

        // 3. Initialize WorkflowBuilder with the starting executor
        var builder = new WorkflowBuilder(executors[startNode.Id]);

        // 4. Add edges based on links
        foreach (var link in def.Links)
        {
            if (executors.TryGetValue(link.SourceNodeId, out var source) &&
                executors.TryGetValue(link.TargetNodeId, out var target))
            {
                builder.AddEdge(source, target);
            }
        }

        // 5. Build the workflow
        return builder.Build();
    }
}

/// <summary>
/// Custom executor that invokes HGesCom commands.
/// </summary>
public sealed partial class HGesComExecutor : Executor
{
    private readonly string _commandName;
    private readonly IHGesCom _gesCom;

    public HGesComExecutor(string id, string commandName, IHGesCom gesCom) : base(id)
    {
        _commandName = commandName;
        _gesCom = gesCom;
    }

    [MessageHandler]
    private async ValueTask<object?> HandleAsync(object? message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Execute the command via HGesCom
        // Each node acts as an executor, receiving a payload (message) and context.
        return await _gesCom.RunCommandAsync(_commandName, new object[] { message!, context }, cancellationToken);
    }

    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder builder)
    {
        return builder;
    }
}
