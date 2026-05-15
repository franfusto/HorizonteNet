using Horizonte.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using log4net;

namespace Horizonte;

public static class Extensions_refac
{
    public const string ContextNameKey = "horizonte";

    public static void ConfigureContext(this HostApplicationBuilder hostBuilder, string contextName = "",
        string workDirectory = "")
    {
        // 1. Cargamos contexto
        if (string.IsNullOrEmpty(contextName)) contextName = ContextNameKey;
        var hContext = new HContext(contextName);
        hostBuilder.Services.AddSingleton<IHContext>(hContext);

        // 2. Configuramos logger
        var log4NetSettings = hContext.Get<Log4NetSettings>() ?? new Log4NetSettings();
        log4NetSettings.Configure();
        hostBuilder.Logging.ClearProviders();
        hostBuilder.Logging.AddLog4Net(new Log4NetProviderOptions() { ExternalConfigurationSetup = true });

        // 3. Cargamos servicios básicos
        hostBuilder.Services.AddSingleton<IHCredManager>(sp => 
        {
            var logger = LogManager.GetLogger(typeof(HCredManager));
            return new HCredManager(logger);
        });
        hostBuilder.Services.AddSingleton<ISymLinkScafolder, SymLinkScafolder>();
        hostBuilder.Services.AddSingleton<IHtrans, Htrans>();
        
        // Registramos HorizonteEnv como servicio (legado/wrapper)
       hostBuilder.Services.AddSingleton<IHorizonteEnv>(sp => new HorizonteEnv(contextName, hostBuilder.Configuration["args"]?.Split(' ') ?? Array.Empty<string>()));
        
        var modulesSettings = hContext.Get<ModulesSettings>() ?? new ModulesSettings();
       hostBuilder.Services.AddSingleton(modulesSettings);
        
        var workerSettings = hContext.Get<WorkerSettings>() ?? new WorkerSettings();
        hostBuilder.Services.AddSingleton(workerSettings);

        // 4. Cargamos gestores principales
        hostBuilder.Services.AddSingleton<IhAssemblyManager, HAssemblyManager>();
        hostBuilder.Services.AddSingleton<IHGesCom, HGesCom>();
        hostBuilder.Services.AddSingleton<IhWorkersManager, HWorkersManager>();

        // 5. Inicializamos componentes que deben estar listos antes del Build del Host si es necesario
        // Pero siguiendo el patrón del usuario, los módulos se cargan después.
    }

    public static void StartWorkers(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var workersManager = scope.ServiceProvider.GetRequiredService<IhWorkersManager>();
        
        // Sincronizar HorizonteEnv legado si existe
        var hEnv = scope.ServiceProvider.GetService<IHorizonteEnv>() as HorizonteEnv;
        hEnv?.SetHost(host);

        // Antes de iniciar workers, debemos asegurar que los módulos estén cargados e inicializados
        var gescom = scope.ServiceProvider.GetRequiredService<IHGesCom>();
        gescom.LoadModules();
        gescom.InitzializeModules();
        
        // También el andamio de archivos simbólicos
        var symLinkScafolder = scope.ServiceProvider.GetRequiredService<ISymLinkScafolder>();
        var hContext = scope.ServiceProvider.GetRequiredService<IHContext>();
        var symLinkSettings = hContext.Get<SymLinkSettings>() ?? new SymLinkSettings();
        symLinkScafolder.BuildScafolder(symLinkSettings.SymLinkDefs);

        // Finalmente configuramos y arrancamos workers
        workersManager.ConfigureWorkers();
        workersManager.StartWorkers();
    }
}
