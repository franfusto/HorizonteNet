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

    public AppManager(IHost host, ILogger<AppManager> logger, ISymLinkScafolder linkScafolder)
    {
        _host = host;
        _logger = logger;
        _linkScafolder = linkScafolder;
    }

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