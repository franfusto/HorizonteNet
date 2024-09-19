using System.Collections;
using System.Reflection;
using Horizonte;
using HorizonteGui.Components;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Radzen;
using Radzen.Blazor;
using Horizonte.Extensions;

namespace HorizonteGui;

public sealed class Worker : BackgroundService, IHservice
{
    private IHorizonteEnv _env;
    private ILogger? _log;
    private IHGesCom? _gesCom;
    private WebApplication _app;
    private IHContext? _context;
    private IHModManager? _modManager;

    public Worker(IHorizonteEnv env, string serviceName, bool runOnStart)
    {
        _env = env;
        ServiceName = serviceName;
        RunOnStart = runOnStart;
    }

    private void ConfigureModAssets(WebApplication builder, IHorizonteEnv env)
    {
        builder.UseStaticFiles();
        if (env.AssetsFolders.Count != 0)
        {
            foreach (var item in env.AssetsFolders)
            {
                builder.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(item),
                    RequestPath = ""
                });
            }
        }
    }


    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log?.LogInformation("Build and start Host");
//
        string location = Assembly.GetExecutingAssembly().Location;
        string dir = Path.GetDirectoryName(location)!;
//
        var builder = WebApplication.CreateSlimBuilder(
            new WebApplicationOptions
            {
                ApplicationName = "HorizonteGui",
                ContentRootPath = dir,
                WebRootPath = Path.Combine(dir, "Assets")
            });
        //builder.Logging.ClearProviders();
        builder.Services.AddSingleton(_env);
        if (_gesCom != null) builder.Services.AddSingleton(_gesCom);
        if (_log != null) builder.Services.AddSingleton(_log);
        if (_context != null) builder.Services.AddSingleton(_context);
        if (_modManager != null) builder.Services.AddSingleton(_modManager);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddRadzenComponents();
        builder.Services.AddScoped<HGuiSession>();
        _app = builder.Build();
        _app.UseAntiforgery();
        ConfigureModAssets(_app, _env);

        _app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return _app.RunAsync(GetUrl());
    }

    public string GetUrl()
    {
        string res =_context?.Get<HGuiSettings>()?.AppUrl ?? "http://0.0.0.0:5000";
        return res;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _log = _env?.HHost.Services.GetService<ILogger<Worker>>();
        _gesCom = _env?.HHost.Services.GetService<IHGesCom>();
        _context = _env?.HHost.Services.GetService<IHContext>();
        _modManager = _env?.HHost.Services.GetService<IHModManager>();
        _log?.LogInformation("Starting HorizonteGui.Worker");
        IsRunning = true;
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log?.LogInformation("Ending HorizonteGui.Worker");
        _app.StopAsync(new CancellationToken());
        IsRunning = false;
        return base.StopAsync(cancellationToken);
    }

    public bool IsRunning { get; set; } = false;
    public bool RunOnStart { get; set; }
    public string ServiceName { get; set; } = "Horizonte Net Gui";
}