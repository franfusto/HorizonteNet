using System.Text.RegularExpressions;
using Horizonte.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte.Services;

public class AppManager : IAppManager
{
    private readonly IHost _host;
    private readonly ILogger<AppManager> _logger;
    private readonly ISymLinkScafolder _linkScafolder;

    /// <summary>
    /// Clase que gestiona el ciclo de vida de una aplicación.
    /// </summary>
    /// <remarks>
    /// AppManager proporciona mecanismos para reiniciar y cerrar la aplicación, además de
    /// gestionar los servicios en segundo plano y el andamiaje de enlaces simbólicos.
    /// </remarks>
    public AppManager(IHost host, ILogger<AppManager> logger, ISymLinkScafolder linkScafolder)
    {
        _host = host;
        _logger = logger;
        _linkScafolder = linkScafolder;
    }

    /// <summary>
    /// Reinicia la aplicación de manera controlada.
    /// Este método cierra la aplicación actual y la inicia nuevamente después de una breve demora.
    /// Es útil en situaciones donde se requiere un reinicio de la aplicación debido a cambios de configuración
    /// o actualizaciones que no se pueden aplicar sin reiniciar.
    /// El proceso de reinicio comienza al registrar un mensaje de información en el registro de eventos.
    /// Luego, se crea un nuevo hilo para manejar el reinicio de tal forma que el proceso actual pueda finalizar adecuadamente.
    /// El hilo espera un breve periodo antes de reiniciar el ejecutable de la aplicación.
    /// </summary>
    /// <returns>Una tarea que representa la operación asíncrona de reinicio de la aplicación.</returns>
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

    /// <summary>
    /// Método que cierra la aplicación de manera asincrónica.
    /// </summary>
    /// <remarks>
    /// Este método se encarga de realizar un cierre ordenado de la aplicación, deteniendo
    /// todos los servicios en segundo plano y limpiando los recursos utilizados.
    /// </remarks>
    /// <returns>
    /// Un <see cref="Task"/> que representa la operación asincrónica del cierre de la aplicación.
    /// </returns>
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