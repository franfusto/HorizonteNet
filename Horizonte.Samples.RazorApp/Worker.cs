using System.Reflection;
using Horizonte;
using Horizonte.Extension.AspNetCore;
using Horizonte.Samples.RazorApp.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Radzen;

namespace Horizonte.Samples.RazorApp;


public class BlazorWorker : BackgroundService, IHorizonteBackgroundService
{
    public bool IsRunning { get; set; } = false;
    public bool RunOnStart { get; set; }
    public string ServiceName { get; set; }
    
    private readonly IHorizonteEnv _env;
    private ILogger? _log;
    private WebApplication app;
    private RazorAppConfig _config = new();

    public BlazorWorker(IHorizonteEnv env,string serviceName,bool runOnStart)
    {
        _env = env;
        ServiceName = serviceName;
        RunOnStart = runOnStart;
    }

    private void GetConfiguration()
    {
        var context = _env.GetService<IHContext>();
        _config = context?.Get<RazorAppConfig>() ?? new RazorAppConfig();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enviroment = _config.DevelMode ? Environments.Development : Environments.Production;
        
        var builder = WebApplication.CreateSlimBuilder(
            new WebApplicationOptions { ApplicationName = "Horizonte.Samples.RazorApp",
             EnvironmentName = enviroment
        });
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        if (_log != null)
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(new ExistingLoggerProvider(_log));
            //builder.Logging.AddFilter("Microsoft", LogLevel.Error); 
            //builder.Logging.AddFilter("Horizonte", LogLevel.Trace); 
            builder.Logging.SetMinimumLevel(LogLevel.Information);
            builder.Services.AddSingleton(_log);
        }
        builder.Services.AddSingleton<WeatherForecastService>();
        builder.Services.AddHorizonteLegacyServices(_env); //-> Add Horizonte services
        builder.Services.AddRadzenComponents();
        app = builder.Build();
        app.UseHorizonteStaticFiles(); //-> Add Horizonte static files
        app.UseRouting();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        IsRunning = true;
        return app.RunAsync(_config.Url);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        GetConfiguration();
        _log = _env?.GetService<ILogger<BlazorWorker>>();
        _log?.LogInformation("Iniciando RazorApp");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log?.LogInformation("Finalizando RazorApp");
        IsRunning = false;
        app?.DisposeAsync();
        return base.StopAsync(cancellationToken);
    }

}