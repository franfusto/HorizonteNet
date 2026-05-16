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
        //0. Establecemos ruta de trabajo
        var rootPath = workDirectory;
        if (string.IsNullOrEmpty(rootPath)) rootPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])!;
        Directory.SetCurrentDirectory(rootPath);
        
        // 1. Cargamos contexto
        if (string.IsNullOrEmpty(contextName)) contextName = ContextNameKey;
        var hContext = new HContext(contextName, rootPath);
        hostBuilder.Services.AddSingleton<IHContext>(hContext);

        // 2. Configuramos logger
        var log4NetSettings = hContext.Get<Log4NetSettings>() ?? new Log4NetSettings();
        log4NetSettings.Configure();
        hostBuilder.Logging.ClearProviders();
        hostBuilder.Logging.AddLog4Net(new Log4NetProviderOptions() { ExternalConfigurationSetup = true });

        // 3. Cargamos gestores principales
        hostBuilder.Services.AddSingleton<IHCredManager, HCredManager>();
        hostBuilder.Services.AddSingleton<ISymLinkScafolder, SymLinkScafolder>();
        hostBuilder.Services.AddSingleton<IHtrans, Htrans>();
        hostBuilder.Services.AddSingleton<IhAssemblyManager, HAssemblyManager>();
        hostBuilder.Services.AddSingleton<IHGesCom, HGesCom>();
        hostBuilder.Services.AddSingleton<IhWorkersManager, HWorkersManager>();
    }

    public static void LoadEnviroment(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var workersManager = scope.ServiceProvider.GetRequiredService<IhWorkersManager>();


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