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

/// <summary>
/// Worker es una clase que se extiende de <see cref="BackgroundService"/> y administra el ciclo de vida de un
/// servicio web, el cual actúa como interfaz de usuario administrativa para la aplicación Horizonte.
/// Esta clase está diseñada para ejecutarse como un servicio en segundo plano.
/// </summary>
public sealed class Worker : BackgroundService
{

    private readonly ILogger<Worker> _log;
    private readonly IHGesCom _gesCom;
    private readonly IhContext _context;
    private readonly IHCredManager _credManager;
    private readonly ISymLinkScafolder _linkScafolder;
    private readonly IhAssemblyManager _assemblyManager;
    private WebApplication? _app;

    /// <summary>
    /// La clase <c>Worker</c> es un servicio en segundo plano implementado utilizando el patrón
    /// <see cref="BackgroundService"/>. Se utiliza para gestionar tareas de larga duración o periódicas
    /// dentro de la aplicación.
    /// </summary>
    /// <remarks>
    /// Este servicio se inicia y se detiene a través de los métodos <see cref="StartAsync"/> y
    /// <see cref="StopAsync"/>, respectivamente. Durante su ejecución, el método
    /// <see cref="ExecuteAsync"/> maneja la lógica principal que debe ser implementada.
    /// </remarks>
    /// <param name="log">Instancia de <c>ILogger</c> para el registro de eventos y errores.</param>
    /// <param name="gesCom">Objeto que implementa la interfaz <c>IHGesCom</c> para gestionar comandos basados en roles.</param>
    /// <param name="context">Objeto que implementa la interfaz <c>IhContext</c>.</param>
    /// <param name="credManager">Instancia de <c>IHCredManager</c> para la gestión de credenciales.</param>
    /// <param name="linkScafolder">Implementación de la interfaz <c>ISymLinkScafolder</c> para operaciones de enlaces simbólicos.</param>
    /// <param name="assemblyManager">Instancia de <c>IhAssemblyManager</c> para la gestión de ensamblados.</param>
    public Worker(ILogger<Worker> log, 
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
    /// Inicia y ejecuta de manera asincrónica el servicio de interfaz de usuario de AdminUi.
    /// Configura el entorno de ejecución para la aplicación web, inicializando servicios, middlewares
    /// y configuraciones necesarias para el funcionamiento del aplicativo.
    /// </summary>
    /// <param name="stoppingToken">
    /// Token de cancelación que se utiliza para notificar al servicio en ejecución que debe detenerse.
    /// Este token es transmitido para facilitar una parada controlada de la ejecución.
    /// </param>
    /// <returns>Una tarea que representa la operación asincrónica del método.</returns>
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
        builder.Services.AddScoped<HGuiSession>();
        
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new ExistingLoggerProvider(_log));
        builder.Logging.AddFilter("Microsoft", LogLevel.Error); 
        builder.Logging.AddFilter("Horizonte", LogLevel.Trace); 

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddRadzenComponents();
        
        _app = builder.Build();
        _app.UseMiddleware<HorizonteStaticFileMiddelware>(typeof(Worker).Assembly);
        _app.UseAntiforgery();
        _app.UseStaticFiles();

        _app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return _app.RunAsync(url);
    }

    /// <summary>
    /// Inicia el servicio de fondo de manera asincrónica.
    /// </summary>
    /// <param name="cancellationToken">
    /// Un token de cancelación que se puede usar para detener la operación de inicio antes de que se complete.
    /// </param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica de inicio del servicio.
    /// </returns>
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Starting Horizonte.AdminUi.Worker");
        IsRunning = true;
        return base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Detiene de manera asíncrona el servicio de fondo de la aplicación.
    /// </summary>
    /// <param name="cancellationToken">
    /// Un token de cancelación que puede usarse para comunicar una solicitud de cancelación del proceso de apagado.
    /// </param>
    /// <return>Una tarea que representa la operación asíncrona de detener el servicio.</return>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Ending Horizonte.AdminUi.Worker");
        _app?.StopAsync(cancellationToken);
        IsRunning = false;
        return base.StopAsync(cancellationToken);
    }

    /// Propiedad que indica si el servicio `Worker` está actualmente en ejecución.
    /// El valor es `true` cuando el servicio se ha iniciado y está operativo.
    /// Se establece como `true` en el método `StartAsync` y se cambia a `false`
    /// en el método `StopAsync`, representando así el estado de ejecución del servicio.
    /// /
    public bool IsRunning { get; set; } = false;

    /// /
    public bool RunOnStart { get; set; }

    /// Propiedad que representa el nombre del servicio en la aplicación.
    /// `ServiceName` se utiliza para identificar el servicio específico al que pertenece esta instancia
    /// de trabajo dentro de la aplicación. El valor predeterminado de esta propiedad es "Horizonte Net Gui".
    /// Es posible modificar este nombre según sea necesario para adaptarlo a distintas configuraciones
    /// o entornos de ejecución.
    /// /
    public string ServiceName { get; set; } = "Horizonte Net Gui";
}