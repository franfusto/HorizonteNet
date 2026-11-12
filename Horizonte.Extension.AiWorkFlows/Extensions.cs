using Horizonte;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace Horizonte.Extension.AiWorkFlows;

/// <summary>
/// Proporciona métodos de extensión para la construcción, ejecución y reanudación de flujos de trabajo basados en <see cref="WorkFlowDef"/>.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Ejecuta un flujo de trabajo a partir de su definición, procesando un mensaje de entrada de tipo <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Tipo del mensaje o datos de entrada y salida del flujo (debe ser una clase de referencia).</typeparam>
    /// <param name="def">Definición del flujo de trabajo (<see cref="WorkFlowDef"/>).</param>
    /// <param name="serviceProvider">Proveedor de servicios para la resolución de dependencias y comandos.</param>
    /// <param name="message">Mensaje o carga útil de entrada para el flujo.</param>
    /// <param name="token">Token de cancelación para abortar la ejecución.</param>
    /// <param name="procMessage">Acción opcional para recibir y procesar eventos generados durante la ejecución.</param>
    /// <param name="manager">Instancia opcional de <see cref="WorkFlowRunManager"/> para supervisar o pausar la ejecución.</param>
    /// <returns>El resultado producido al finalizar el flujo de trabajo.</returns>
    public static async Task<T> RunWorkFlow<T>(this WorkFlowDef def, IServiceProvider serviceProvider, T message,
        CancellationToken token = default, Action<WorkflowEvent>? procMessage = null,
        WorkFlowRunManager? manager = null) where T : class
    {
        var workflow = def.Build<T>(serviceProvider, token, manager);
        var result = await workflow.Run(message, procMessage, token, manager, def.Id);
        return result?.Data as T ?? null!;
    }

    /// <summary>
    /// Construye una instancia ejecutable de <see cref="Workflow"/> a partir de su definición <see cref="WorkFlowDef"/>.
    /// </summary>
    /// <typeparam name="T">Tipo del mensaje o datos que fluirán por los nodos del flujo.</typeparam>
    /// <param name="def">Definición del flujo con sus nodos, enlaces y condiciones.</param>
    /// <param name="serviceProvider">Proveedor de servicios para resolver el gestor de comandos <see cref="IHGesCom"/>.</param>
    /// <param name="token">Token de cancelación.</param>
    /// <param name="manager">Gestor de ejecución opcional.</param>
    /// <returns>Una instancia de <see cref="Workflow"/> compilada y lista para ser ejecutada.</returns>
    /// <exception cref="InvalidOperationException">Se lanza si no se encuentra el servicio <see cref="IHGesCom"/> o si falta el nodo de entrada 'In'.</exception>
    public static Workflow Build<T>(this WorkFlowDef def, IServiceProvider serviceProvider,
        CancellationToken token = default, WorkFlowRunManager? manager = null)
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
            node => new HGesComExecutor<T>(node.Id, node.CommandAction, gesCom, token, def.FlowErrorProcessor, def.FlowCancelProcessor, manager)
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

        return (message) =>
        {
            return
                (gesCom.RunCommand<bool>(link.Condition.ConditionCommand, [message!]) == link.Condition.ExpectedValue);
        };
    }

    /// <summary>
    /// Ejecuta el flujo de trabajo en modo streaming, procesando el mensaje de entrada y emitiendo eventos.
    /// </summary>
    /// <typeparam name="T">Tipo del mensaje de entrada (clase de referencia).</typeparam>
    /// <param name="workflow">Instancia de <see cref="Workflow"/> a ejecutar.</param>
    /// <param name="message">Mensaje o datos de entrada para el flujo.</param>
    /// <param name="eventhandler">Acción opcional para recibir eventos de ejecución.</param>
    /// <param name="token">Token de cancelación.</param>
    /// <param name="manager">Gestor de ejecución opcional para control de estados y checkpoints.</param>
    /// <param name="workflowId">Identificador opcional del flujo de trabajo.</param>
    /// <returns>El evento final producido por el flujo de trabajo (<see cref="WorkflowEvent"/>), o null.</returns>
    public static async Task<WorkflowEvent?> Run<T>(this Workflow workflow, T message,
        Action<WorkflowEvent>? eventhandler = null, CancellationToken token = default,
        WorkFlowRunManager? manager = null, string workflowId = "") where T : class
    {
        using var linkedCts = manager != null ? manager.CreateLinkedCts(token) : null;
        var effectiveToken = linkedCts?.Token ?? token;

        manager?.SetStatus(WorkFlowStatus.Running);

        var result = default(WorkflowEvent);
        try
        {
            var checkpointManager = manager?.CheckpointManager ?? CheckpointManager.Default;
            await using var run =
                await InProcessExecution.RunStreamingAsync(workflow, input: message, checkpointManager: checkpointManager, cancellationToken: effectiveToken);

            if (manager != null)
            {
                manager.AttachRun(run, workflowId);
            }

            await foreach (var evt in run.WatchStreamAsync(effectiveToken))
            {
                Console.WriteLine(evt);
                result = evt;
                manager?.NotifyEvent(evt);
                eventhandler?.Invoke(evt);
                if (evt is WorkflowOutputEvent)
                {
                    manager?.SetStatus(WorkFlowStatus.Completed);
                    manager?.CleanupCheckpoint();
                    return evt;
                }
            }

            if (manager != null && manager.IsPauseRequested && run.LastCheckpoint != null)
            {
                await manager.SaveCheckpointAsync(run.LastCheckpoint);
                manager.SetStatus(WorkFlowStatus.Paused);
            }
            else if (manager != null && manager.Status != WorkFlowStatus.Paused && manager.Status != WorkFlowStatus.Cancelled)
            {
                manager.SetStatus(WorkFlowStatus.Completed);
                manager.CleanupCheckpoint();
            }
        }
        catch (OperationCanceledException)
        {
            manager?.SetStatus(WorkFlowStatus.Cancelled);
            throw;
        }
        catch (Exception)
        {
            manager?.SetStatus(WorkFlowStatus.Faulted);
            throw;
        }

        return result;
    }

    /// <summary>
    /// Reanuda la ejecución en streaming de un flujo de trabajo a partir de un punto de control específico.
    /// </summary>
    /// <param name="workflow">Instancia de <see cref="Workflow"/> a reanudar.</param>
    /// <param name="checkpoint">Información del punto de control previo (<see cref="CheckpointInfo"/>).</param>
    /// <param name="manager">Gestor de ejecución opcional.</param>
    /// <param name="eventhandler">Acción opcional para recibir eventos de ejecución durante la reanudación.</param>
    /// <param name="token">Token de cancelación.</param>
    /// <returns>El evento final producido tras la reanudación (<see cref="WorkflowEvent"/>), o null.</returns>
    public static async Task<WorkflowEvent?> Resume(this Workflow workflow, CheckpointInfo checkpoint,
        WorkFlowRunManager? manager = null, Action<WorkflowEvent>? eventhandler = null,
        CancellationToken token = default)
    {
        using var linkedCts = manager != null ? manager.CreateLinkedCts(token) : null;
        var effectiveToken = linkedCts?.Token ?? token;

        manager?.SetStatus(WorkFlowStatus.Running);

        var result = default(WorkflowEvent);
        try
        {
            var checkpointManager = manager?.CheckpointManager ?? CheckpointManager.Default;
            await using var run =
                await InProcessExecution.ResumeStreamingAsync(workflow, fromCheckpoint: checkpoint, checkpointManager: checkpointManager, cancellationToken: effectiveToken);

            if (manager != null)
            {
                manager.AttachRun(run, "");
            }

            await foreach (var evt in run.WatchStreamAsync(effectiveToken))
            {
                Console.WriteLine(evt);
                result = evt;
                manager?.NotifyEvent(evt);
                eventhandler?.Invoke(evt);
                if (evt is WorkflowOutputEvent)
                {
                    manager?.SetStatus(WorkFlowStatus.Completed);
                    manager?.CleanupCheckpoint();
                    return evt;
                }
            }

            if (manager != null && manager.IsPauseRequested && run.LastCheckpoint != null)
            {
                await manager.SaveCheckpointAsync(run.LastCheckpoint);
                manager.SetStatus(WorkFlowStatus.Paused);
            }
            else if (manager != null && manager.Status != WorkFlowStatus.Paused && manager.Status != WorkFlowStatus.Cancelled)
            {
                manager.SetStatus(WorkFlowStatus.Completed);
                manager.CleanupCheckpoint();
            }
        }
        catch (OperationCanceledException)
        {
            manager?.SetStatus(WorkFlowStatus.Cancelled);
            throw;
        }
        catch (Exception)
        {
            manager?.SetStatus(WorkFlowStatus.Faulted);
            throw;
        }

        return result;
    }
}

internal sealed class HGesComExecutor<T>(string id, string command, IHGesCom gesCom, CancellationToken sharedToken,
    string? flowErrorProcessor = null, string? flowCancelProcessor = null, WorkFlowRunManager? manager = null)
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

        // Comprobación de pausa antes de ejecutar la acción
        if (manager != null && manager.IsPauseRequested)
        {
            await context.RequestHaltAsync();
            return message;
        }

        try
        {
            // Comprobación temprana
            ct.ThrowIfCancellationRequested();
            var result = await gesCom.RunCommandAsync<T>(command, [message!, context, ct]);

            // Comprobación de pausa tras ejecutar la acción
            if (manager != null && manager.IsPauseRequested)
            {
                await context.RequestHaltAsync();
            }
            if (manager != null && manager.Status == WorkFlowStatus.Cancelled)
            {
                throw new OperationCanceledException();
            }
            return result;
        }
        //aqui definimos la lógica que queremos para manejar errores y cancelaciones
        catch (OperationCanceledException)
        {
            if (string.IsNullOrEmpty(flowCancelProcessor)) throw;
            message = await HandleCancelAsync(message);
            await context.YieldOutputAsync(message!, ct);
            await context.RequestHaltAsync();
            return message;
        }
        catch (Exception ex)
        {
            if (string.IsNullOrEmpty(flowErrorProcessor)) throw;
            message = await HandleErrorAsync(message, ex);
            await context.YieldOutputAsync(message!, ct);
            await context.RequestHaltAsync();
            return message;
        }
    }
    
    private async Task<T> HandleErrorAsync(T message, Exception ex)
    {
        if (flowErrorProcessor == null) return message;
        return await gesCom.RunCommandAsync<T>(flowErrorProcessor, [message!, ex]);
    }    
    private async Task<T> HandleCancelAsync(T message)
    {
        if (flowCancelProcessor == null) return message;
        return await gesCom.RunCommandAsync<T>(flowCancelProcessor, [message!]);
    }
    
}