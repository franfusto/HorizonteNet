using Horizonte;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Modulo3;

public class WsWorker : BackgroundService, IHservice
{
    public string ServiceName { get; set; } = "Web Service Worker";
    public bool IsRunning { get; set; }
    public bool RunOnStart { get; set; }
    private WebApplication? _app;

    public WsWorker(IHorizonteEnv env, string serviceName, bool runOnStart)
    {
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://0.0.0.0:5003");
        _app = builder.Build();
        _app.MapGet("/", () => "Hello World!");
        _app.MapGet("/time", () => DateTime.Now);
        IsRunning = true;
        return _app.RunAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        IsRunning = false;
        return _app?.StopAsync(cancellationToken) ?? Task.CompletedTask;
    }
}