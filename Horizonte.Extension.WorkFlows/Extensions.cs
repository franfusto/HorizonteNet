using Horizonte;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace Horizonte.Extension.WorkFlows;

public static class Extensions
{
    public static Workflow Build<T>(this WorkFlowDef def, IHorizonteEnv env)
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
            node => new HGesComExecutor<T>(node.Id, node.CommandAction, gesCom)
        );

        // 3. Initialize WorkflowBuilder with the starting executor
        var builder = new WorkflowBuilder(executors[startNode.Id]);

        // 4. Add edges based on links
        foreach (var link in def.Links)
        {
            if (executors.TryGetValue(link.SourceNodeId, out var source) &&
                executors.TryGetValue(link.TargetNodeId, out var target))
            {
                Func<T, bool>? condition = GetConditionFunc<T>(link, gesCom);
                var edge = builder.AddEdge(source, target, condition!);

                // If the target node is an 'Out' node, mark it as a workflow output
                if (def.Nodes.Any(n => n.Id == link.TargetNodeId && n.Type == WorkFlowNodeTypes.Out))
                {
                    edge.WithOutputFrom(target);
                }

                // If the source node is also an 'Out' node, mark it as a workflow output
                if (def.Nodes.Any(n => n.Id == link.SourceNodeId && n.Type == WorkFlowNodeTypes.Out))
                {
                    edge.WithOutputFrom(source);
                }
            }
        }

        // 5. Special case: If the start node is also an 'Out' node but has no edges,
        // it's not possible to mark it as output via builder.AddEdge(...).WithOutputFrom(...).
        // However, if there are edges starting from it, we've handled it above.
        // If it's a single-node workflow (only 'In'), it should probably be connected to itself or similar,
        // but 'WorkflowBuilder' expects at least one edge for outputs usually.

        // 6. Build the workflow
        return builder.Build();
    }

    private static Func<T, bool>? GetConditionFunc<T>(WorkFlowLink link, IHGesCom gesCom)
    {
        if (link.Condition == null || string.IsNullOrEmpty(link.Condition.ConditionCommand))
        {
            return null;
        }

        return (message) => { return gesCom.RunCommand<bool>(link.Condition.ConditionCommand, [message!]); };
    }

    public static async Task<WorkflowEvent?> Run<T>(this Workflow workflow, T message, Action<WorkflowEvent>? eventhandler = null, CancellationToken token = default) where T : notnull
    {
        Console.WriteLine($"Extensions.Run Token Hash: {token.GetHashCode()}");
        var result = default(WorkflowEvent);
        await using var run = await InProcessExecution.RunStreamingAsync(workflow, input: message,cancellationToken: token);
        await foreach (var evt in run.WatchStreamAsync())
        {
            Console.WriteLine(evt);
            result = evt;
            eventhandler?.Invoke(evt);
            if (evt is WorkflowOutputEvent)
            {
                return evt;
            }
        }
        return result;
    }
}

/// <summary>
/// Custom executor that invokes HGesCom commands.
/// </summary>
///
internal sealed class HGesComExecutor<T>(string id, string command, IHGesCom gesCom) : Executor<T, T>(id)
{
    public override async ValueTask<T> HandleAsync(T message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"HGesComExecutor.HandleAsync Token Hash: {cancellationToken.GetHashCode()}");
        Console.WriteLine($"HGesComExecutor.HandleAsync Context Hash: {context.GetHashCode()}");
        cancellationToken.ThrowIfCancellationRequested();
        return await gesCom.RunCommandAsync<T>(command, cancellationToken, [message!, context, cancellationToken]);
    }
}

/*
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
        return await _gesCom.RunCommandAsync(_commandName, new object[] { message!, context });
    }

    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder builder)
    {
        return builder;
    }
}
*/