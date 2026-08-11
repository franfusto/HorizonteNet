using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Horizonte;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Horizonte.Ai.MCPServer;

public class MCPWorker : BackgroundService
{
    public bool IsRunning { get; set; } = false;

    private readonly ILogger<MCPWorker> _log;
    private WebApplication? _app;

    public MCPWorker(ILogger<MCPWorker> logger)
    {
        _log = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("Iniciando MCPWorker");
        var builder = WebApplication.CreateBuilder();

        builder.Services
            .AddMcpServer()
            .WithHttpTransport()
            .WithTools<MCPTools>();
        _app = builder.Build();
        _app.MapMcp("/mcp");

        await _app.RunAsync("http://localhost:5005");
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Starting MCPWorker");
        return base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Finalizando MCPWorker");
        if (_app != null)
        {
            await _app.StopAsync(cancellationToken);
        }
        await base.StopAsync(cancellationToken);
    }
}