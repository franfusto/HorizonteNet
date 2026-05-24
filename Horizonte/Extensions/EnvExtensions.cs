using Horizonte.Helpers;
using Horizonte.Interfaces;
using Horizonte.Services;
using Horizonte.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte.Extensions;

/// <summary>
/// Proporciona métodos de extensión para configurar e inicializar el entorno principal de Horizonte
/// sobre un <see cref="HostApplicationBuilder"/> y un <see cref="IHost"/>.
/// </summary>
/// <remarks>
/// Esta clase centraliza la creación del contexto, la configuración del sistema de registro
/// y el registro de los servicios base necesarios para el funcionamiento del framework.
/// </remarks>
public static class EnvExtensions
{
    /// <summary>
    /// Nombre de contexto predeterminado utilizado cuando no se especifica uno explícitamente.
    /// </summary>
    public const string ContextNameKey = "horizonte";

    /// <summary>
    /// Configura el entorno base de Horizonte en el contenedor de servicios del host.
    /// </summary>
    /// <param name="hostBuilder">
    /// Constructor del host sobre el que se registrarán el contexto, el registro y los servicios principales.
    /// </param>
    /// <param name="contextName">
    /// Nombre del contexto que se utilizará para cargar la configuración. Si está vacío,
    /// se usará <see cref="ContextNameKey"/>.
    /// </param>
    /// <param name="rootPath">
    /// Ruta raíz de trabajo de la aplicación. Si no se especifica, se utilizará la carpeta
    /// del ejecutable actual.
    /// </param>
    /// <remarks>
    /// Este método realiza las siguientes operaciones:
    /// <list type="number">
    /// <item><description>Establece el directorio de trabajo actual.</description></item>
    /// <item><description>Crea y registra una instancia de <see cref="IHContext"/>.</description></item>
    /// <item><description>Configura el registro usando Log4Net.</description></item>
    /// <item><description>Registra los servicios principales de Horizonte en el contenedor de dependencias.</description></item>
    /// </list>
    /// </remarks>
    public static void ConfigureContext(this HostApplicationBuilder hostBuilder, string contextName = "",
        string rootPath = "")
    {
        // Establecemos ruta de trabajo
        if (string.IsNullOrEmpty(rootPath)) rootPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])!;
        Directory.SetCurrentDirectory(rootPath);
    
        // Cargamos contexto
        if (string.IsNullOrEmpty(contextName)) contextName = ContextNameKey;
        var hContext = new HContext(contextName, rootPath);
        hostBuilder.Services.AddSingleton<IHContext>(hContext);

        // Configuramos logger
        var log4NetSettings = hContext.Get<Log4NetSettings>() ?? new Log4NetSettings();
        log4NetSettings.Configure();
        hostBuilder.Logging.ClearProviders();
        hostBuilder.Logging.AddLog4Net(new Log4NetProviderOptions() { ExternalConfigurationSetup = true });

        // Cargamos gestores principales
        hostBuilder.Services.AddSingleton<IHCredManager, HCredManager>();
        hostBuilder.Services.AddSingleton<ISymLinkScafolder, SymLinkScafolder>();
        hostBuilder.Services.AddSingleton<IhAssemblyManager, HAssemblyManager>();
        hostBuilder.Services.AddSingleton<IHGesCom, HGesCom>();
        hostBuilder.Services.AddSingleton<IhWorkersManager, HWorkersManager>();
        hostBuilder.Services.AddSingleton<IAppManager, AppManager>();
    }

    /// <summary>
    /// Finaliza la carga del entorno una vez construido el host.
    /// </summary>
    /// <param name="host">
    /// Host ya construido desde el que se resolverán los servicios necesarios para la inicialización.
    /// </param>
    /// <remarks>
    /// Este método:
    /// <list type="bullet">
    /// <item><description>Carga e inicializa los módulos registrados.</description></item>
    /// <item><description>Construye el andamio de enlaces simbólicos configurado.</description></item>
    /// <item><description>Configura y arranca los workers disponibles.</description></item>
    /// </list>
    /// Debe invocarse después de haber llamado previamente a <see cref="ConfigureContext"/>.
    /// </remarks>
    public static void LoadEnviroment(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var workersManager = scope.ServiceProvider.GetRequiredService<IhWorkersManager>();


        // Cargamos e iniciamos los modulos
        var gescom = scope.ServiceProvider.GetRequiredService<IHGesCom>();
        gescom.LoadModules();
        gescom.InitzializeModules();


        // Configuramos y arrancamos workers
        workersManager.StartWorkers();
    }
    
    
}