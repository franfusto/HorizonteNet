using System.Text.Json;
using Microsoft.Agents.AI.Workflows;

namespace Horizonte.Extension.AiWorkFlows;

public enum WorkFlowStatus
{
    Idle,
    Running,
    Pausing,
    Paused,
    Resuming,
    Completed,
    Cancelled,
    Faulted
}

public class WorkFlowSnapshot
{
    public string WorkflowId { get; set; } = string.Empty;
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public WorkFlowStatus Status { get; set; } = WorkFlowStatus.Idle;
    public string? CurrentNodeId { get; set; }
    public object? LastPayload { get; set; }
    public CheckpointInfo? EngineCheckpoint { get; set; }
}

public class WorkFlowRunManager
{
    public const string DefaultCheckpointPath = "./checkpoint.json";

    private readonly object _lock = new();
    private CancellationTokenSource? _internalCts;
    private volatile bool _pauseRequested;
    private StreamingRun? _currentRun;
    private string _workflowId = string.Empty;
    private string? _currentNodeId;
    private object? _lastPayload;
    private CheckpointInfo? _lastCheckpoint;

    public WorkFlowStatus Status { get; private set; } = WorkFlowStatus.Idle;

    public bool IsPauseRequested => _pauseRequested;

    public CheckpointManager CheckpointManager { get; set; } = CheckpointManager.Default;

    public event Action<WorkFlowStatus>? StatusChanged;
    public event Action<WorkflowEvent>? EventReceived;

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
            }
            else if (evt is WorkflowOutputEvent outputEvt)
            {
                _lastPayload = outputEvt.Data;
            }
        }

        EventReceived?.Invoke(evt);
    }

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
