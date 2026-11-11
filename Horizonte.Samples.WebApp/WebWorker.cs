using System.Reflection;
using Horizonte.Extension.AspNetCore;
using Horizonte.Interfaces;
using Horizonte.Samples.WebApp.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Radzen;

namespace Horizonte.Samples.WebApp;

/// <summary>
/// Servicio en segundo plano que hospeda y ejecuta una aplicación web Blazor (Razor Components con Radzen) dentro de Horizonte.
/// </summary>
/// <remarks>
/// Administra el ciclo de vida del servidor web ASP.NET Core mínimo, configurando los servicios interactivos de Razor Components,
/// la biblioteca de componentes Radzen y el middleware para servir recursos estáticos y páginas web.
/// </remarks>
public class WebWorker : BackgroundService
{
    private readonly ILogger<WebWorker> _log;
    private readonly IHGesCom _gesCom;
    private readonly IhContext _context;
    private readonly IHCredManager _credManager;
    private readonly ISymLinkScafolder _linkScafolder;
    private readonly IhAssemblyManager _assemblyManager;
    private WebApplication? _app;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="WebWorker"/>.
    /// </summary>
    /// <param name="log">Instancia de <see cref="ILogger{TCategoryName}"/> para el registro de eventos y diagnósticos del servicio.</param>
    /// <param name="gesCom">Gestor de comandos de Horizonte (<see cref="IHGesCom"/>) para interactuar con otros módulos.</param>
    /// <param name="context">Contexto global de ejecución y configuración de Horizonte (<see cref="IhContext"/>).</param>
    /// <param name="credManager">Gestor de credenciales del marco Horizonte (<see cref="IHCredManager"/>).</param>
    /// <param name="linkScafolder">Herramienta de andamiaje y gestión de enlaces simbólicos (<see cref="ISymLinkScafolder"/>).</param>
    /// <param name="assemblyManager">Gestor de ensamblados y tipos cargados en Horizonte (<see cref="IhAssemblyManager"/>).</param>
    public WebWorker(ILogger<WebWorker> log, 
        IHGesCom gesCom, 
        IhContext context, 
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

    /// <summary>
    /// Ejecuta en segundo plano la aplicación web ASP.NET Core, configurando los componentes de Razor, Radzen y el enrutamiento interactivo.
    /// </summary>
    /// <param name="stoppingToken">Token de cancelación que señala la solicitud de finalización del servicio.</param>
    /// <returns>Una tarea que representa la ejecución asíncrona del servidor web hospedado.</returns>
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
        _app.UseMiddleware<HorizonteStaticFileMiddelware>(typeof(WebWorker).Assembly);
        _app.UseAntiforgery();
        _app.UseStaticFiles();

        _app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return _app.RunAsync(url);

    }

    /// <summary>
    /// Inicia el servicio en segundo plano de la aplicación web.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación para abortar la operación de inicio.</param>
    /// <returns>Una tarea que representa la operación asíncrona de inicio.</returns>
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Starting Horizonte.Samples.WebApp.WebWorker");
        return base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Detiene el servicio en segundo plano y el servidor web ASP.NET Core hospedado.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación para abortar la operación de detención.</param>
    /// <returns>Una tarea que representa la operación asíncrona de detención.</returns>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Ending Horizonte.Samples.WebApp.WebWorker");
        _app?.StopAsync();
        return base.StopAsync(cancellationToken);
    }
}