using Horizonte;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;
namespace Horizonte.Extension.AiWorkFlows;

public static class Extensions
{
    public static Workflow Build<T>(this WorkFlowDef def, IServiceProvider serviceProvider, CancellationToken token = default)
    {
        var gesCom = serviceProvider.GetService<IHGesCom>();
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
            node => new HGesComExecutor<T>(node.Id, node.CommandAction, gesCom, token)
        );

        // 3. Initialize WorkflowBuilder with the starting executor
        var builder = new WorkflowBuilder(executors[startNode.Id]);

        // 4. Add edges based on links
        foreach (var link in def.Links)
        {
            if (executors.TryGetValue(link.SourceNodeId, out var source) &&
                executors.TryGetValue(link.TargetNodeId, out var target))
            {
                var condition = GetConditionFunc<T>(link, gesCom);
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

        return (message) => { return 
            (gesCom.RunCommand<bool>(link.Condition.ConditionCommand, [message!]) == link.Condition.ExpectedValue); 
        };
    }

    public static async Task<WorkflowEvent?> Run<T>(this Workflow workflow, T message,
        Action<WorkflowEvent>? eventhandler = null, CancellationToken token = default) where T : notnull
    {
        var result = default(WorkflowEvent);
        await using var run =
            await InProcessExecution.RunStreamingAsync(workflow, input: message, cancellationToken: token);
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

internal sealed class HGesComExecutor<T>(string id, string command, IHGesCom gesCom, CancellationToken sharedToken)
    : Executor<T, T>(id)
{
    public override async ValueTask<T> HandleAsync(
        T message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        // Combinar señales (motor + compartida) 
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(sharedToken, cancellationToken);
        var ct = linkedCts.Token;
        try
        {
            // Comprobación temprana
            ct.ThrowIfCancellationRequested();
            return await gesCom.RunCommandAsync<T>(command, [message!, context, ct]);
        }
        catch (OperationCanceledException)
        {
            await context.YieldOutputAsync(message!, ct);
            await context.RequestHaltAsync();
            return message;
        }
    }
}