using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte;

public class StartEndWorker : BackgroundService, IHservice
{
    private readonly IHorizonteEnv _env;
    private ILogger? _log;
    private IHGesCom? _gesCom;
    private IHContext? _context;
    private SECommandsSettings _seCommandsSettings;
    public bool IsRunning { get; set; } = false;
    public bool RunOnStart { get; set; }
    public string ServiceName { get; set; }

    public StartEndWorker(IHorizonteEnv env,string serviceName,bool runOnStart)
    {
        _env = env;
        ServiceName = serviceName;
        RunOnStart = runOnStart;
    }

    private void DoWork(CancellationToken stoppingToken)
    {
       
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
        _seCommandsSettings = _context?.Get<SECommandsSettings>() ?? new SECommandsSettings();
        _gesCom = _env.HHost.Services.GetService<IHGesCom>();
        _log?.LogInformation("Iniciando tart/End Worker");
        StartCommands();
        IsRunning = true;
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log?.LogInformation("Finalizando tart/End Worker");
        IsRunning = false;
        EndCommands();
        return base.StopAsync(cancellationToken);
    }


    public void StartCommands()
    {
        foreach (var item in _seCommandsSettings.StartCommands)
        {
            if(!item.Active) continue;
            _gesCom?.RunCommand(item.CommandName);
        }
        
    }

    public void EndCommands()
    {
        if(!IsRunning) return;
        foreach (var item in _seCommandsSettings.EndCommands)
        {
            if(!item.Active) continue;
            _gesCom?.RunCommand(item.CommandName);
        }
    }


}