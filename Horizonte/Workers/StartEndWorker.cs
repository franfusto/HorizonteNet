using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte;

public class StartEndWorker : BackgroundService, IHservice
{
    public bool IsRunning { get; set; } 
    public bool RunOnStart { get; set; } 
    public string ServiceName { get; set; } 
    
    private readonly IHorizonteEnv _env;
    private ILogger? _log;
    private IHGesCom? _gesCom;
    private IHContext? _context;
    private SeCommandsSettings _seCommandsSettings = new ();

    public StartEndWorker(IHorizonteEnv env,string serviceName,bool runOnStart)
    {
        _env = env;
        ServiceName = serviceName;
        RunOnStart = runOnStart;
        ServiceName = serviceName;
    }

    private void DoWork(CancellationToken stoppingToken)
    {
        _log?.LogInformation("Iniciando tart/End Worker");
        StartCommands();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log?.LogInformation("Ejecutando Start/End Worker");
        IsRunning = true;
        return Task.Run(() => DoWork(stoppingToken));
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _log = _env.HHost.Services.GetService<ILogger<FakeWorker>>();
        _context = _env.HHost.Services.GetService<IHContext>();
        _seCommandsSettings = _context?.Get<SeCommandsSettings>() ?? new SeCommandsSettings();
        _gesCom = _env.HHost.Services.GetService<IHGesCom>();
        IsRunning = true;
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log?.LogInformation("Finalizando Start/End Worker");
        IsRunning = false;
        EndCommands();
        return base.StopAsync(cancellationToken);
    }


    private void StartCommands()
    {
        foreach (var item in _seCommandsSettings.StartCommands.Where(item => item.Active))
        {
            _gesCom?.RunCommand(item.CommandName);
        }
    }

    private void EndCommands()
    {
        if(!IsRunning) return;
        foreach (var item in _seCommandsSettings.EndCommands.Where(item => item.Active))
        {
            _gesCom?.RunCommand(item.CommandName);
        }
    }


}