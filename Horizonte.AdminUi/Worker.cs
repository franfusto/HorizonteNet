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

namespace Horizonte.AdminUi;

public sealed class Worker : BackgroundService, IHorizonteBackgroundService
{
    private readonly IHorizonteEnv _env;
    private ILogger? _log;
    private IHGesCom? _gesCom;
    private WebApplication _app;
    private IHContext? _context;
    private IHCredManager? _credManager;
    private ISymLinkScafolder? _linkScafolder;
    //private IHModManager? _modManager;

    public Worker(IHorizonteEnv env, string serviceName, bool runOnStart)
    {
        _env = env;
        ServiceName = serviceName;
        RunOnStart = runOnStart;
    }



    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log?.LogInformation("Build and start AdminUi");
        string location = Assembly.GetExecutingAssembly().Location;
        string dir = Path.GetDirectoryName(location)!;
        string url =_context?.Get<HGuiSettings>()?.AppUrl ?? "http://0.0.0.0:5000";
        var builder = WebApplication.CreateSlimBuilder(
            new WebApplicationOptions
            {
                ApplicationName = "Horizonte.AdminUi",
                ContentRootPath = dir,
                WebRootPath = Path.Combine(dir, "wwwroot")
            });
        builder.Services.AddSingleton(_env);
        if (_gesCom != null) builder.Services.AddSingleton(_gesCom);
        if (_log != null)
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(new ExistingLoggerProvider(_log));
            builder.Logging.AddFilter("Microsoft", LogLevel.Error); 
            builder.Logging.AddFilter("Horizonte", LogLevel.Trace); 
            //builder.Logging.SetMinimumLevel(LogLevel.Error);
            builder.Services.AddSingleton(_log);
        }
        if (_context != null) builder.Services.AddSingleton(_context);
        if(_credManager != null) builder.Services.AddSingleton(_credManager);
        if (_linkScafolder != null) builder.Services.AddSingleton(_linkScafolder);
       // if (_modManager != null) builder.Services.AddSingleton(_modManager);
       
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddRadzenComponents();
        builder.Services.AddScoped<HGuiSession>();
        _app = builder.Build();
       _app.UseHorizonteStaticFiles();
        _app.UseAntiforgery();
        _app.UseStaticFiles();

        _app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return _app.RunAsync(url);
    }


    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _log = _env?.HHost.Services.GetService<ILogger<Worker>>();
        _gesCom = _env?.HHost.Services.GetService<IHGesCom>();
        _context = _env?.HHost.Services.GetService<IHContext>();
        _credManager = _env?.HHost.Services.GetService<IHCredManager>();
        _linkScafolder = _env?.HHost.Services.GetService<ISymLinkScafolder>();
        //_modManager = _env?.HHost.Services.GetService<IHModManager>();
        _log?.LogInformation("Starting Horizonte.AdminUi.Worker");
        IsRunning = true;
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log?.LogInformation("Ending Horizonte.AdminUi.Worker");
        _app.StopAsync(cancellationToken);
        IsRunning = false;
        return base.StopAsync(cancellationToken);
    }

    public bool IsRunning { get; set; } = false;
    public bool RunOnStart { get; set; }
    public string ServiceName { get; set; } = "Horizonte Net Gui";
}