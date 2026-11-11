using System.Text.Json;
using Microsoft.Agents.AI.Workflows;

namespace Horizonte.Extension.AiWorkFlows;

/// <summary>
/// Representa el estado de ejecución de un flujo de trabajo.
/// </summary>
public enum WorkFlowStatus
{
    /// <summary>
    /// El flujo de trabajo no ha comenzado o está inactivo.
    /// </summary>
    Idle,

    /// <summary>
    /// El flujo de trabajo está ejecutándose activamente.
    /// </summary>
    Running,

    /// <summary>
    /// Se ha solicitado pausar el flujo de trabajo y está completando la fase actual.
    /// </summary>
    Pausing,

    /// <summary>
    /// El flujo de trabajo está pausado en un punto de control (checkpoint).
    /// </summary>
    Paused,

    /// <summary>
    /// El flujo de trabajo se está reanudando desde un punto de control previo.
    /// </summary>
    Resuming,

    /// <summary>
    /// El flujo de trabajo ha finalizado exitosamente.
    /// </summary>
    Completed,

    /// <summary>
    /// La ejecución del flujo de trabajo ha sido cancelada.
    /// </summary>
    Cancelled,

    /// <summary>
    /// La ejecución del flujo de trabajo ha fallado debido a un error no recuperable.
    /// </summary>
    Faulted
}

/// <summary>
/// Representa una instantánea o snapshot del estado y punto de control de un flujo de trabajo.
/// </summary>
public class WorkFlowSnapshot
{
    /// <summary>
    /// Obtiene o establece el identificador del flujo de trabajo.
    /// </summary>
    public string WorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el identificador único de la instancia de ejecución.
    /// </summary>
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Obtiene o establece la marca de tiempo UTC en la que se tomó la instantánea.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Obtiene o establece el estado actual del flujo de trabajo en la instantánea.
    /// </summary>
    public WorkFlowStatus Status { get; set; } = WorkFlowStatus.Idle;

    /// <summary>
    /// Obtiene o establece el identificador del nodo actualmente en ejecución o último ejecutado.
    /// </summary>
    public string? CurrentNodeId { get; set; }

    /// <summary>
    /// Obtiene o establece los últimos datos o carga útil procesada en el flujo.
    /// </summary>
    public object? LastPayload { get; set; }

    /// <summary>
    /// Obtiene o establece la información del punto de control del motor de workflows (<see cref="CheckpointInfo"/>).
    /// </summary>
    public CheckpointInfo? EngineCheckpoint { get; set; }
}

/// <summary>
/// Administra el ciclo de vida, ejecución, pausa, reanudación, cancelación e instantáneas de un flujo de trabajo.
/// </summary>
public class WorkFlowRunManager
{
    /// <summary>
    /// Ruta de archivo predeterminada utilizada para almacenar los puntos de control de ejecución.
    /// </summary>
    public const string DefaultCheckpointPath = "./checkpoint.json";

    private readonly object _lock = new();
    private CancellationTokenSource? _internalCts;
    private volatile bool _pauseRequested;
    private StreamingRun? _currentRun;
    private string _workflowId = string.Empty;
    private string? _currentNodeId;
    private object? _lastPayload;
    private CheckpointInfo? _lastCheckpoint;

    /// <summary>
    /// Obtiene el estado actual de ejecución del flujo de trabajo (<see cref="WorkFlowStatus"/>).
    /// </summary>
    public WorkFlowStatus Status { get; private set; } = WorkFlowStatus.Idle;

    /// <summary>
    /// Obtiene un valor que indica si se ha solicitado una pausa en la ejecución actual.
    /// </summary>
    public bool IsPauseRequested => _pauseRequested;

    /// <summary>
    /// Obtiene o establece el administrador de puntos de control (<see cref="CheckpointManager"/>) del motor de workflows.
    /// </summary>
    public CheckpointManager CheckpointManager { get; set; } = CheckpointManager.Default;

    /// <summary>
    /// Evento que se desencadena cuando cambia el estado de ejecución del flujo de trabajo.
    /// </summary>
    public event Action<WorkFlowStatus>? StatusChanged;

    /// <summary>
    /// Evento que se desencadena cuando el motor de workflows emite un nuevo evento (<see cref="WorkflowEvent"/>).
    /// </summary>
    public event Action<WorkflowEvent>? EventReceived;

    /// <summary>
    /// Establece un nuevo estado de ejecución para el flujo de trabajo y notifica a los suscriptores.
    /// </summary>
    /// <param name="newStatus">El nuevo estado a asignar (<see cref="WorkFlowStatus"/>).</param>
    public void SetStatus(WorkFlowStatus newStatus)
    {
        lock (_lock)
        {
            if (Status == newStatus) return;
            Status = newStatus;
        }
        StatusChanged?.Invoke(newStatus);
    }

    internal CancellationTokenSource CreateLinkedCts(CancellationToken externalToken)
    {
        lock (_lock)
        {
            _internalCts?.Dispose();
            _internalCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            _pauseRequested = false;
            return _internalCts;
        }
    }

    internal void AttachRun(StreamingRun run, string workflowId)
    {
        lock (_lock)
        {
            _currentRun = run;
            _workflowId = workflowId;
        }
    }

    internal void NotifyEvent(WorkflowEvent evt)
    {
        lock (_lock)
        {
            if (evt is ExecutorInvokedEvent invokedEvt)
            {
                _currentNodeId = invokedEvt.ExecutorId;
                _lastPayload = invokedEvt.Data;
            }
            else if (evt is WorkflowOutputEvent outputEvt)
            {
                _lastPayload = outputEvt.Data;
            }
        }

        EventReceived?.Invoke(evt);
    }

    /// <summary>
    /// Solicita de forma asincrónica la pausa de la ejecución actual del flujo de trabajo.
    /// </summary>
    /// <returns>Una tarea completada que representa la solicitud de pausa.</returns>
    public Task PauseAsync()
    {
        lock (_lock)
        {
            if (Status == WorkFlowStatus.Running || Status == WorkFlowStatus.Resuming)
            {
                _pauseRequested = true;
                SetStatus(WorkFlowStatus.Pausing);
            }
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Reanuda la ejecución del flujo de trabajo a partir del punto de control previamente guardado.
    /// </summary>
    /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias y comandos.</param>
    /// <param name="def">Definición del flujo de trabajo a reanudar (<see cref="WorkFlowDef"/>).</param>
    /// <param name="procMessage">Acción opcional de devolución de llamada para procesar eventos generados durante la reanudación.</param>
    /// <param name="token">Token de cancelación opcional.</param>
    /// <returns>Una tarea que representa la operación asincrónica de reanudación.</returns>
    /// <exception cref="InvalidOperationException">Se lanza si no se encuentra ningún punto de control guardado.</exception>
    public async Task ResumeAsync(IServiceProvider serviceProvider, WorkFlowDef def, Action<WorkflowEvent>? procMessage = null, CancellationToken token = default)
    {
        var checkpoint = await LoadCheckpointAsync();
        if (checkpoint == null)
        {
            throw new InvalidOperationException("No se encontró ningún checkpoint guardado para reanudar.");
        }

        SetStatus(WorkFlowStatus.Resuming);
        var workflow = def.Build<object>(serviceProvider, token, this);
        await workflow.Resume(checkpoint, this, procMessage, token);
    }

    /// <summary>
    /// Cancela de forma asincrónica la ejecución en curso del flujo de trabajo.
    /// </summary>
    /// <returns>Una tarea completada que representa la cancelación.</returns>
    public Task CancelAsync()
    {
        lock (_lock)
        {
            if (_internalCts != null && !_internalCts.IsCancellationRequested)
            {
                _internalCts.Cancel();
            }
            SetStatus(WorkFlowStatus.Cancelled);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Obtiene una instantánea (<see cref="WorkFlowSnapshot"/>) con el estado, datos actuales y punto de control del flujo.
    /// </summary>
    /// <returns>Una nueva instancia de <see cref="WorkFlowSnapshot"/> con la información capturada.</returns>
    public WorkFlowSnapshot GetSnapshot()
    {
        lock (_lock)
        {
            return new WorkFlowSnapshot
            {
                WorkflowId = _workflowId,
                Status = Status,
                CurrentNodeId = _currentNodeId,
                LastPayload = _lastPayload,
                EngineCheckpoint = _lastCheckpoint ?? _currentRun?.LastCheckpoint,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    internal async Task SaveCheckpointAsync(CheckpointInfo checkpoint, object? lastPayload = null, string? currentNodeId = null)
    {
        WorkFlowSnapshot snapshot;
        lock (_lock)
        {
            _lastCheckpoint = checkpoint;
            if (lastPayload != null) _lastPayload = lastPayload;
            if (currentNodeId != null) _currentNodeId = currentNodeId;

            snapshot = new WorkFlowSnapshot
            {
                WorkflowId = _workflowId,
                Status = WorkFlowStatus.Paused,
                CurrentNodeId = _currentNodeId,
                LastPayload = _lastPayload,
                EngineCheckpoint = checkpoint,
                Timestamp = DateTime.UtcNow
            };
        }

        var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(DefaultCheckpointPath, json);
    }

    internal async Task<CheckpointInfo?> LoadCheckpointAsync()
    {
        if (!File.Exists(DefaultCheckpointPath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(DefaultCheckpointPath);
        var snapshot = JsonSerializer.Deserialize<WorkFlowSnapshot>(json);
        if (snapshot?.EngineCheckpoint != null)
        {
            lock (_lock)
            {
                _workflowId = snapshot.WorkflowId;
                _currentNodeId = snapshot.CurrentNodeId;
                _lastPayload = snapshot.LastPayload;
                _lastCheckpoint = snapshot.EngineCheckpoint;
            }
            return snapshot.EngineCheckpoint;
        }

        return null;
    }

    internal void CleanupCheckpoint()
    {
        if (File.Exists(DefaultCheckpointPath))
        {
            try
            {
                File.Delete(DefaultCheckpointPath);
            }
            catch
            {
                // Ignore cleanup error
            }
        }
    }
}
