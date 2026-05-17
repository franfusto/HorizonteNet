using System.Reflection;
using Horizonte;
using Horizonte.Extension.AspNetCore;
using Horizonte.Samples.RazorApp;
using Horizonte.Samples.RazorApp.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Radzen;

namespace Horizonte.Samples;


public class BlazorWorker : BackgroundService
{
    
    private ILogger? _log;
    private WebApplication app;
    private RazorAppConfig _config = new();
    private IServiceProvider _services;

    public BlazorWorker(IServiceProvider services)
    {
        _log = services?.GetService<ILogger<BlazorWorker>>();
        _services = services;
    }

    private void GetConfiguration()
    {
        var context = _services.GetService<IHContext>();
        _config = context?.Get<RazorAppConfig>() ?? new RazorAppConfig();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enviroment = _config.DevelMode ? Environments.Development : Environments.Production;
        string location = Assembly.GetExecutingAssembly().Location;
        string dir = Path.GetDirectoryName(location)!;
        var builder = WebApplication.CreateSlimBuilder(
            new WebApplicationOptions
            {
                ApplicationName = "Horizonte.Samples.RazorApp",
                ContentRootPath = dir,
                WebRootPath = Path.Combine(dir, "wwwroot")
            });
        //builder.Services.AddRazorPages();
        //builder.Services.AddServerSideBlazor();
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
        builder.Services.AddHorizonteLegacyServices(_services); //-> Add Horizonte services
        builder.Services.AddRadzenComponents();
        app = builder.Build();
        app.UseHorizonteStaticFiles(); //-> Add Horizonte static files
        app.UseRouting();
        //app.MapBlazorHub();
        //app.MapFallbackToPage("/_Host");

        return app.RunAsync(_config.Url);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        GetConfiguration();
        
        _log?.LogInformation("Iniciando RazorApp");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log?.LogInformation("Finalizando RazorApp");
        app?.DisposeAsync();
        return base.StopAsync(cancellationToken);
    }

}