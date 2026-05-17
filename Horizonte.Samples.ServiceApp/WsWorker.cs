using Horizonte;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
//using Microsoft.OpenApi.Models;

namespace Horizonte.Samples.ServiceApp;

public class WsWorker : BackgroundService
{

   
    private WebApplication? _app;
    private ILogger? _log;
    private IHGesCom? _gesCom;
    private IHContext? _context;
    private ServiceConfig _config = new();
    
    public WsWorker(IServiceProvider serviceProvider)
    {
        _log = serviceProvider?.GetService<ILogger<WsWorker>>();
        _gesCom = serviceProvider?.GetService<IHGesCom>();
        _context = serviceProvider?.GetService<IHContext>();
        //Horizonte.Samples.ServiceApp.WsWorker

    }

    //https://medium.com/@mayoorakasri20/building-a-minimal-web-api-with-asp-net-core-and-net-8-c2df508b0c8a
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(_config.Url);
        if (_log != null)
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(new ExistingLoggerProvider(_log));
           // builder.Logging.AddFilter("Microsoft", LogLevel.Error); 
           // builder.Logging.AddFilter("Horizonte", LogLevel.Trace); 
            builder.Logging.SetMinimumLevel(LogLevel.Information);
            builder.Services.AddSingleton(_log);
        }
        if (_config.EnableSwagger)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                //c.SwaggerDoc("v1", new OpenApiInfo
                //{
                   // Title = "Web Service", Description = "Sample web service worker", Version = "v1"
                //});
            });
        }
        _app = builder.Build();
        if (_config.EnableSwagger)
        {
            _app.UseSwagger();
            _app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample web service worker API V1");
            });
        }
        //todo: cancelation token
        _app.MapGet("/", () => "Hello World!");
        _app.MapGet("/time", () => DateTime.Now);
        _app.MapGet("/workers", (CancellationToken ct) => _gesCom?.RunCommandAsync("Workers_GetServicesRunning"));
        _app.MapGet("/assemblies", () => _gesCom?.RunCommand("Horizonte_ListLoadedAssemblies"));
  
        return _app.RunAsync(stoppingToken);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {

        _config = _context?.Get<ServiceConfig>() ?? new ServiceConfig();
        
        _log?.LogInformation("Starting Web Service Worker");
        return base.StartAsync(cancellationToken);  
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log?.LogInformation("Ending Web Service Worker");

        return _app?.StopAsync(cancellationToken) ?? Task.CompletedTask;
    }
}