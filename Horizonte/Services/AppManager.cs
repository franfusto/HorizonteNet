using System.Text.RegularExpressions;
using Horizonte.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte.Services;

/// <summary>
/// Clase responsable de gestionar el ciclo de vida de una aplicación.
/// </summary>
/// <remarks>
/// La clase AppManager proporciona mecanismos esenciales para controlar el comportamiento de una aplicación,
/// incluyendo la capacidad de reiniciar y terminar la ejecución de la misma. Además, gestiona servicios en segundo plano
/// y el andamiaje de enlaces simbólicos necesarios para el funcionamiento de la aplicación.
/// </remarks>
public class AppManager : IAppManager
{
    private readonly IHost _host;
    private readonly ILogger<AppManager> _logger;
    private readonly ISymLinkScafolder _linkScafolder;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="AppManager"/>.
    /// </summary>
    /// <param name="host">Host de la aplicación para gestionar el ciclo de vida.</param>
    /// <param name="logger">Servicio de registro de eventos para <see cref="AppManager"/>.</param>
    /// <param name="linkScafolder">Gestor del andamiaje de enlaces simbólicos.</param>
    public AppManager(IHost host, ILogger<AppManager> logger, ISymLinkScafolder linkScafolder)
    {
        _host = host;
        _logger = logger;
        _linkScafolder = linkScafolder;
    }

    /// <inheritdoc/>
    public async Task Reboot()
    {
        _logger.LogInformation("Reiniciando la aplicación...");
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = false;
            var appexe = Environment.GetCommandLineArgs()[0];
            var extension = "";
            if (OperatingSystem.IsWindows()) extension = ".exe";
            Thread.Sleep(5000); 
            System.Diagnostics.Process.Start(Regex.Replace(appexe, ".dll", extension, RegexOptions.IgnoreCase));
        }).Start();
        await Quit();
    }

    /// <inheritdoc/>
    public async Task Quit()
    {
        _logger.LogInformation("Cerrando la aplicación...");
        var services = _host.Services.GetServices<BackgroundService>();
        foreach (var service in services)
        {
            await service.StopAsync(new CancellationToken());
        }

        // eliminamos andamio de enlaces simbólicos
        _linkScafolder.CleanScafolder();

        await _host.StopAsync(new CancellationToken());
        
    }
}