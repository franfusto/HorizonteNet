using Horizonte;
using Horizonte.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

//using Microsoft.OpenApi.Models;

namespace Horizonte.Samples.ServiceApp;

/// <summary>
/// Servicio en segundo plano que hospeda y ejecuta una aplicación web ASP.NET Core mínima dentro de Horizonte.
/// </summary>
/// <remarks>
/// Administra el ciclo de vida de la aplicación web del servicio de ejemplo, configurando endpoints para la
/// ejecución de comandos y diagnóstico, e integrando soporte para Swagger UI según lo definido en <see cref="ServiceConfig"/>.
/// </remarks>
public class WsWorker : BackgroundService
{
    private WebApplication? _app;
    private ILogger? _log;
    private IHGesCom? _gesCom;
    private IhContext? _context;
    private ServiceConfig _config = new();


    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="WsWorker"/>.
    /// </summary>
    /// <param name="serviceProvider">Proveedor de servicios utilizado para resolver dependencias como el logger, el gestor de comandos (<see cref="IHGesCom"/>) y el contexto de configuración (<see cref="IhContext"/>).</param>
    public WsWorker(IServiceProvider serviceProvider)
    {
        _log = serviceProvider?.GetService<ILogger<WsWorker>>();
        _gesCom = serviceProvider?.GetService<IHGesCom>();
        _context = serviceProvider?.GetService<IhContext>();
    }

    /// <summary>
    /// Ejecuta en segundo plano la aplicación web ASP.NET Core, configurando el enrutamiento de endpoints y la interfaz Swagger si está habilitada.
    /// </summary>
    /// <param name="stoppingToken">Token de cancelación que indica el momento en el que el servicio debe detenerse.</param>
    /// <returns>Una tarea que representa la ejecución en curso del servidor web.</returns>
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
            //builder.Logging.SetMinimumLevel(LogLevel.Information);
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

        return _app.RunAsync();
    }

    /// <summary>
    /// Inicia el servicio en segundo plano, cargando la configuración <see cref="ServiceConfig"/> desde el contexto de la aplicación.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación para abortar el inicio del servicio.</param>
    /// <returns>Una tarea que representa la operación asíncrona de inicio.</returns>
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _config = _context?.Get<ServiceConfig>() ?? new ServiceConfig();

        _log?.LogInformation("Starting Web Service Worker");
        return base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Detiene el servicio en segundo plano y la aplicación web hospedada si se encuentra en ejecución.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación para abortar la detención del servicio.</param>
    /// <returns>Una tarea que representa la operación asíncrona de detención.</returns>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log?.LogInformation("Ending Web Service Worker");
        if (_app != null)
        {
            _app?.StopAsync();
        }

        return base.StopAsync(cancellationToken);
    }
}