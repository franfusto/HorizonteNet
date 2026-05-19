using System.Collections;
using System.Reflection;
using Horizonte;
using Horizonte.AdminUi.Components;
using Horizonte.Extension.AspNetCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Radzen;
using Radzen.Blazor;
using Horizonte.Extensions;
using Horizonte.Interfaces;

namespace Horizonte.AdminUi;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _log;
    private readonly IHGesCom _gesCom;
    private readonly IHContext _context;
    private readonly IHCredManager _credManager;
    private readonly ISymLinkScafolder _linkScafolder;
    private readonly IhAssemblyManager _assemblyManager;
    private WebApplication? _app;

    public Worker(ILogger<Worker> log, 
                  IHGesCom gesCom, 
                  IHContext context, 
                  IHCredManager credManager, 
                  ISymLinkScafolder linkScafolder, 
                  IhAssemblyManager assemblyManager)
    {
        _log = log;
        _gesCom = gesCom;
        _context = context;
        _credManager = credManager;
        _linkScafolder = linkScafolder;
        _assemblyManager = assemblyManager;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("Build and start AdminUi");
        string location = Assembly.GetExecutingAssembly().Location;
        string dir = Path.GetDirectoryName(location)!;
        string url = _context.Get<HGuiSettings>()?.AppUrl ?? "http://0.0.0.0:5000";
        var builder = WebApplication.CreateSlimBuilder(
            new WebApplicationOptions
            {
                ApplicationName = "Horizonte.AdminUi",
                ContentRootPath = dir,
                WebRootPath = Path.Combine(dir, "wwwroot")
            });
        
        builder.Services.AddSingleton(_gesCom);
        builder.Services.AddSingleton((ILogger)_log);
        builder.Services.AddSingleton(_context);
        builder.Services.AddSingleton(_credManager);
        builder.Services.AddSingleton(_linkScafolder);
        builder.Services.AddSingleton(_assemblyManager);
        
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new ExistingLoggerProvider(_log));
        builder.Logging.AddFilter("Microsoft", LogLevel.Error); 
        builder.Logging.AddFilter("Horizonte", LogLevel.Trace); 

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddRadzenComponents();
        builder.Services.AddScoped<HGuiSession>();
        
        _app = builder.Build();
        _app.UseHorizonteStaticFiles(typeof(Worker).Assembly);
        _app.UseAntiforgery();
        _app.UseStaticFiles();

        _app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return _app.RunAsync(url);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Starting Horizonte.AdminUi.Worker");
        IsRunning = true;
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Ending Horizonte.AdminUi.Worker");
        _app?.StopAsync(cancellationToken);
        IsRunning = false;
        return base.StopAsync(cancellationToken);
    }

    public bool IsRunning { get; set; } = false;
    public bool RunOnStart { get; set; }
    public string ServiceName { get; set; } = "Horizonte Net Gui";
}