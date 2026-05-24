using System.Reflection;
using Horizonte.Extension.AspNetCore;
using Horizonte.Interfaces;
using Horizonte.Samples.WebApp.Components;
using Radzen;

namespace Horizonte.Samples.WebApp;

public class WebWorker :BackgroundService
{
    private readonly ILogger<WebWorker> _log;
    private readonly IHGesCom _gesCom;
    private readonly IHContext _context;
    private readonly IHCredManager _credManager;
    private readonly ISymLinkScafolder _linkScafolder;
    private readonly IhAssemblyManager _assemblyManager;
    private WebApplication? _app;

    public WebWorker(ILogger<WebWorker> log, 
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
        _log.LogInformation("Starting Horizonte.Samples.WebApp.WebWorker");
        string location = Assembly.GetExecutingAssembly().Location;
        string dir = Path.GetDirectoryName(location)!;
        string url = "http://0.0.0.0:5001";
        var builder = WebApplication.CreateSlimBuilder(
            new WebApplicationOptions
            {
                ApplicationName = "Horizonte.Samples.WebApp",
                ContentRootPath = dir,
                WebRootPath = Path.Combine(dir, "wwwroot")
            });
        builder.Services.AddSingleton(_context);
        builder.Services.AddSingleton(_assemblyManager);
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new ExistingLoggerProvider(_log));
        builder.Logging.AddFilter("Microsoft", LogLevel.Error); 
        builder.Logging.AddFilter("Horizonte", LogLevel.Trace); 
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddRadzenComponents();
        _app = builder.Build();
        _app.UseHorizonteStaticFiles(typeof(WebWorker).Assembly);
        _app.UseAntiforgery();
        _app.UseStaticFiles();

        _app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return _app.RunAsync(url);

    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Starting Horizonte.Samples.WebApp.WebWorker");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Ending Horizonte.Samples.WebApp.WebWorker");
        _app?.StopAsync();
        return base.StopAsync(cancellationToken);
    }
}