using Microsoft.Extensions.Hosting;

namespace Horizonte.Entities;

public sealed class BackgroundServiceState
{
    public string WorkerType { get; init; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public BackgroundService? Instance { get; set; }
    public CancellationTokenSource? RunCancellationTokenSource { get; set; }
    public CancellationTokenSource? StopCancellationTokenSource { get; set; }
    public bool IsRunning { get; set; }
    public bool RestartOnDomainLoad { get; set; }
}