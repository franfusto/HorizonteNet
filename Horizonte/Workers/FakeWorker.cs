using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte;

public class FakeWorker : BackgroundService, IHservice
{
    private readonly IHorizonteEnv? _env;
    private ILogger? _log;
    private IHGesCom? _gesCom;
    public bool IsRunning { get; set; } = false;
    public bool RunOnStart { get; set; }
    public string ServiceName { get; set; }

    public FakeWorker(IHorizonteEnv env,string serviceName,bool runOnStart)
    {
        _env = env;
        ServiceName = serviceName;
        RunOnStart = runOnStart;
    }

    private void DoWork(CancellationToken stoppingToken)
    {
        int i = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            _log?.LogInformation("FakeWorker contador: " + i.ToString());
            Thread.Sleep(5000);
            i++;
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log?.LogInformation("Ejecutando FakeWorker");
        IsRunning = true;
        return Task.Run(() => DoWork(stoppingToken));
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _log = _env?.HHost.Services.GetService<ILogger<FakeWorker>>();
        _gesCom = _env?.HHost.Services.GetService<IHGesCom>();
        _log?.LogInformation("Iniciando FakeWorker");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log?.LogInformation("Finalizando FakeWorker");
        IsRunning = false;
        return base.StopAsync(cancellationToken);
    }


}