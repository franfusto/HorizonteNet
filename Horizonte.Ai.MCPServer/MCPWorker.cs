using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Horizonte;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Horizonte.Ai.MCPServer;

public class MCPWorker : BackgroundService
{
    public bool IsRunning { get; set; } = false;


    private ILogger? _log;
    private IHost _app;

    public MCPWorker(ILogger<MCPWorker> logger)
    {
        _log = logger;
    }


    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log?.LogInformation("Iniciando MCPWorker");
        _log.LogInformation("Starting Horizonte.Samples.WebApp.WebWorker");
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddMcpServer();

        _app = builder.Build();
        return _app.RunAsync();
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Starting MCPWorker");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log?.LogInformation("Finalizando MCPWorker");
        _app?.StopAsync();
        return base.StopAsync(cancellationToken);
    }
}

//////