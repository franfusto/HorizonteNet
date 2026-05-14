using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Horizonte.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using log4net;
using log4net.Appender;
using log4net.Core;
using Microsoft.Extensions.Logging;
using ILogger = log4net.ILog;

namespace Horizonte;

/// <summary>
/// Representa el entorno principal para gestionar una aplicación modular.
/// Esta clase es responsable de manejar el contexto de la aplicación,
/// gestionar la carga de módulos e inicializar el marco de alojamiento.
/// Sirve como el punto central para gestionar el ciclo de vida de la aplicación modular.
/// </summary>
public class HorizonteEnv : IHorizonteEnv
{
    public IHost HHost { get; private set; }
    public string RootPath { get; private set; }
    public string Contextname { get; private set; }
    public StaticFileRegistry StaticFileRegistry { get; set; }
   // public UnmanagedDllResolver UnmanagedDllResolver { get; set; }
    public IhAssemblyManager? AssemblyManager { get; set; }

    private HostApplicationBuilder? _builder { get; set; }
    private readonly string[] _args;
    private HContext _context;
    private HCredManager _credManager;
    private ModulesSettings _modulesSettings;
    private SymLinkScafolder _linkScafolder;
    private SymLinkSettings _linkSettings;
    private WorkerSettings _workerSettings;
    private Log4NetSettings _log4NetSettings;
    private IHGesCom? _gescom;
    
    private List<BackgroundService> _startServicesList = new();
    private CancellationToken _cts = new CancellationToken();
    private ILogger _startlogger;

    //public List<Assembly> Assemblies { get; set; }
    /// <summary>
    /// Representa el entorno principal para gestionar una aplicación modular.
    /// Esta clase es responsable de manejar el contexto de la aplicación,
    /// gestionar la carga de módulos e inicializar el marco de alojamiento.
    /// Sirve como el punto central para gestionar el ciclo de vida de la aplicación modular.
    /// </summary>
    public HorizonteEnv(string contextname, string[] appargs)
    {

        _args = appargs;
        Contextname = contextname;
        StaticFileRegistry = new(this);

        Stage0(); // Registro de inicio
        Stage1(); // Cargar contexto
        Stage2_pre(); // Crear andamio de enlace simbolicos
        Stage2(); // Cargar ensamblados
        Stage3(); // Cargar moóulos 
        
    }
    
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {

        Stage4(); // Crear Host
        Stage5(); // Iniciar Módulos
        Stage6(); // Iniciar Workers
        Stage7(); // Iniciar Host 
        
    }
    

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await Quit();
    }



    
    // ********* SERVICES *********

    /// <summary>
    /// Obtiene un servicio del contenedor de dependencias configurado en el host.
    /// Este método devuelve una instancia del servicio solicitado o null si no se encuentra.
    /// Se utiliza para acceder a servicios registrados y configurados en el momento de inicialización del entorno.
    /// </summary>
    /// <typeparam name="T">Tipo del servicio que se desea recuperar.</typeparam>
    /// <returns>Una instancia del servicio solicitado o null si no está disponible.</returns>
    public T? GetService<T>()
    {
        try
        {
            if (typeof(T) == typeof(IHGesCom) && _gescom != null) return (T?)_gescom;
            return HHost != null ? HHost.Services.GetService<T>() : default;
        }
        catch (Exception e)
        {
            _startlogger.Error(e);
            return default;
        }
    }

    /// <summary>
    /// Obtiene una instancia del servicio requerido del contenedor de dependencias de la aplicación.
    /// Si el servicio no se encuentra registrado, se produce una excepción.
    /// </summary>
    /// <typeparam name="T">El tipo del servicio a resolver.</typeparam>
    /// <param name="value">Un parámetro adicional que representa el tipo que se busca. No se utiliza directamente en la resolución.</param>
    /// <returns>Una instancia del servicio solicitado del tipo especificado.</returns>
    public T? GetRequiredService<T>(T value) where T : Type
    {
        return HHost.Services.GetRequiredService<T>();
    }



    // ********* LIFECYCLE *********

    /// <summary>
    /// Reinicia la aplicación ejecutando una nueva instancia del proceso principal.
    /// La operación se realiza en un nuevo subproceso para mantener el flujo del programa y evitar bloquear el hilo actual.
    /// También incluye un breve retraso antes de iniciar el nuevo proceso y cierra el entorno actual al finalizar.
    /// </summary>
    public async Task Reboot()
    {
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = false;
            var appexe =Environment.GetCommandLineArgs()[0]; // añadir resto de argumentos de inicio
            var extension = "";
            if (OperatingSystem.IsWindows()) extension = ".exe";
            Thread.Sleep(5000); // sacar a configuración
            System.Diagnostics.Process.Start(Regex.Replace(appexe,".dll",extension,RegexOptions.IgnoreCase));
        }).Start();
        Quit();
    }

    /// <summary>
    /// Finaliza de manera controlada todos los servicios y detiene el host de la aplicación.
    /// Este método se encarga de detener servicios en segundo plano y luego solicita la terminación adecuada del host,
    /// asegurando que todos los procesos relacionados concluyan de forma correcta.
    /// </summary>
    public  async Task Quit()
    {
        var services= HHost.Services.GetServices<BackgroundService>();
        foreach (var service in services)
        {
            await service.StopAsync(new CancellationToken());
        }
        
        //eliminamos andamio de enlaces simbolicos
        _linkScafolder.CleanScafolder();
        
        await HHost.StopAsync(new CancellationToken());
    }

    
    // ********* STAGES *********
    
    // stage 0 - crear logger para registro del arranque del entorno modular
    private void Stage0()
    {
        try
        {
            /*
            new Log4NetSettings()
            {
                EnableConsoleAppender = true,
                EnableFileAppender = true,
                FileAppenderFileName = "start.log",
                FileAppenderMaxSizeRollBackups = 1,
                FileAppenderMaximumFileSize = "10MB",
                FileAppenderRollingMode = 1,
                FileAppenderStaticLogFileName = true,
            }.Configure();*/

            _startlogger = LogManager.GetLogger(typeof(HorizonteEnv));
            _startlogger.Info("******** STAGE 0 - INIT START LOGGER **********");

        }
        catch (Exception e)
        {
            _startlogger.Error(e);
        }
    }

    //stage 1 - cargar contexto
    
    /// <summary>
    /// Maneja la inicialización del contexto de la aplicación en el entorno modular.
    /// Este método es responsable de configurar el directorio de trabajo, crear el contexto de la aplicación
    /// y cargar la configuración modular, las configuraciones de registro y los ajustes de los trabajadores.
    /// Representa la primera etapa en el proceso de inicio de la aplicación modular.
    /// </summary>
    private void Stage1()
    {
        try
        {
            _startlogger.Info("******** STAGE 1 - LOAD CONTEXT **********");

            //establecemos ruta de trabajo
            RootPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])!;
            Directory.SetCurrentDirectory(RootPath);

            //creamos contexto y obtenemos configuraciones
            _context = new HContext(Contextname,new JsonSerializerOptions(){IncludeFields = true});
            
            _modulesSettings = _context.Get<ModulesSettings>() ?? new ModulesSettings();
            _workerSettings = _context.Get<WorkerSettings>() ?? new WorkerSettings();
            _log4NetSettings = _context.Get<Log4NetSettings>() ?? new Log4NetSettings();

            _log4NetSettings.Configure();//////fuera de aqui.....
            

            _credManager = new HCredManager(_startlogger);

        }
        catch (Exception e)
        {
            _startlogger.Error(e);
        }
    }
    private void Stage2_pre()
    {
        _startlogger.Info("******** STAGE 2_pre - CREATE SYMLINK SCAFOLDER **********");
        try
        {
            _linkScafolder = new SymLinkScafolder();
            _linkSettings = _context.Get<SymLinkSettings>() ??  new SymLinkSettings() ;
            _linkScafolder.CleanScafolder();
            _linkScafolder.BuildScafolder(_linkSettings.SymLinkDefs);
        }
        catch (Exception e)
        {
            _startlogger.Error(e);
        }
        
    }
    //stage 2 - cargar ensamblados

    /// <summary>
    /// Maneja el proceso de carga de ensamblados dentro del entorno de la aplicación modular.
    /// Esta etapa es responsable de inicializar el administrador de ensamblados, que gestiona la carga dinámica
    /// de ensamblados necesaria para el funcionamiento de la arquitectura modular.
    /// </summary>
    private void Stage2()
    {
        _startlogger.Info("******** STAGE 2 - LOAD ASSEMBLIES **********");
        try
        {
            AssemblyManager = new HAssemblyManager(_modulesSettings, _linkScafolder, this);
            //Assemblies = _assemblyManager.Assemblies;
        }
        catch (Exception e)
        {
            _startlogger.Error(e);
        }
    }


 

    //stage 3 - cargar módulos
    /// <summary>
    /// Maneja la carga e inicialización de los módulos dentro del entorno de la aplicación modular.
    /// Esta etapa es responsable de preparar e integrar todos los módulos necesarios para asegurar
    /// un funcionamiento adecuado durante la ejecución de la aplicación.
    /// </summary>
    private void Stage3()
    {
        _startlogger.Info("******** STAGE 3 - LOAD MODULES **********");
        try
        {
            _gescom = new HGesCom(this);
            _gescom.LoadModules();
        }
        catch (Exception e)
        {
            _startlogger.Error(e);
        }
    }

    //stage 4 - Crear Host
    /// <summary>
    /// Crea y configura el entorno de host necesario para ejecutar la aplicación modular.
    /// Este método inicializa los componentes principales de alojamiento, configura la inyección de dependencias,
    /// el registro, las traducciones, la gestión de ensamblados, la gestión de módulos y los servicios de trabajo.
    /// Esta etapa asegura que el entorno de host de la aplicación esté construido y listo para ejecutarse.
    /// </summary>
    /// <remarks>
    /// Este método es una parte crucial del proceso de inicialización de la aplicación, conectando la infraestructura
    /// y los servicios principales de la aplicación necesarios para ejecutar y gestionar el entorno modular.
    /// </remarks>
    private void Stage4()
    {
        try
        {
            _startlogger.Info("******** STAGE 4 - CREATE HHOST **********");
            //creamos host
            _builder = Host.CreateEmptyApplicationBuilder(null);

            //contexto
            _builder.Configuration.Sources.Clear();
            _builder.Services.AddSingleton<IHContext>(_context);
            
            //gestor de credenciales
            _builder.Services.AddSingleton<IHCredManager>(_credManager);

            //ensamblados
            if (AssemblyManager != null) _builder.Services.AddSingleton<IhAssemblyManager>(AssemblyManager);

            //log 
            _builder.ConfigureLog4Net(_context);

            // traducciones
            _builder.Services.AddSingleton<IHtrans, Htrans>();

            //gestor de entorno
            _builder.Services.AddSingleton<IHorizonteEnv>(this);

            //gestor de commandos
            if (_gescom != null) _builder.Services.AddSingleton(_gescom);
            
            //scafolder
            if(_linkScafolder != null!) _builder.Services.AddSingleton<ISymLinkScafolder>(_linkScafolder);

            //gestión de módulos
            //_builder.Services.AddSingleton<IHModManager, HModManager>();

            //Workers
            _builder.ConfigureWorkers(_workerSettings, this);

            //Host Build
            HHost = _builder.Build();
        }
        catch (Exception e)
        {
            _startlogger.Error(e);
        }
    }

    //stage5  - Iniciar módulos
    /// <summary>
    /// Inicializa los módulos en el entorno de la aplicación modular.
    /// Este método es responsable de desencadenar el proceso de inicialización para todos los módulos
    /// gestionados por la aplicación. Garantiza que los módulos estén correctamente preparados
    /// y cargados en el contexto de la aplicación.
    /// </summary>
    private void Stage5()
    {
        _startlogger.Info("******** STAGE 5 - INITIALIZE MODULES **********");
        try
        {
            _gescom?.InitzializeModules();
        }
        catch (Exception e)
        {
            _startlogger.Error(e);
        }
    }

    //stage 6 - Iniciar Workers
    /// <summary>
    /// Ejecuta el proceso de inicialización de los servicios de trabajo dentro del entorno de la aplicación modular.
    /// Este método es responsable de iniciar y gestionar todos los servicios de trabajo en segundo plano que están 
    /// configurados para ejecutarse durante la fase de inicio de la aplicación, asegurando un flujo de ejecución adecuado 
    /// y manejando cualquier excepción que pueda ocurrir durante el proceso.
    /// </summary>
    private void Stage6()
    {
        _startlogger.Info("******** STAGE 6 - WORKERS **********");
        try
        {
            RunOnStartWorkers();
        }
        catch (Exception e)
        {
            _startlogger.Error(e);
        }
    }

    //stage 7 - Iniciar Host
    /// <summary>
    /// Representa la etapa final en el proceso de inicialización del entorno de alojamiento
    /// para una aplicación modular. Este método es responsable de iniciar y ejecutar
    /// el host de la aplicación, asegurando que todos los servicios y configuraciones
    /// necesarios se ejecuten para mantener el ciclo de vida en tiempo de ejecución de la aplicación.
    /// </summary>
    /// <remarks>
    /// Durante esta etapa, el host se ejecuta utilizando la configuración establecida, como
    /// la inyección de dependencias, los servicios en segundo plano y otras configuraciones del host.
    /// Cualquier excepción encontrada durante la ejecución del host es capturada y registrada.
    /// </remarks>
    private void Stage7()
    {
        _startlogger.Info("******** STAGE 7 - RUN HOST **********");
        try
        {
            
            HHost.Run();
        }
        catch (Exception e)
        {
            _startlogger.Error(e);
        }
    }

    
    // ********* WORKERS *********


    /// <summary>
    /// Agrega un servicio en segundo plano a la lista de trabajadores que se iniciarán
    /// cuando se inicialice la aplicación. Esto permite añadir trabajadores personalizados
    /// al proceso de arranque de la aplicación modular.
    /// </summary>
    /// <param name="worker">El servicio en segundo plano que se añadirá a la lista de trabajadores de inicio.</param>
    public void AddOnStartWorker(BackgroundService worker)
    {
        _startServicesList.Add(worker);
    }


    /// <summary>
    /// Ejecuta los trabajadores de inicio añadidos al entorno. Este método itera
    /// a través de la lista de trabajadores, verifica si están configurados para ejecutarse al inicio
    /// e inicia su ejecución. Después de la ejecución, la lista de trabajadores se limpia.
    /// </summary>
    /// <remarks>
    /// Los trabajadores se procesan de manera ordenada según los nombres de sus tipos.
    /// Solo se ejecutarán los trabajadores que implementen la interfaz <see cref="IHorizonteBackgroundService"/> con
    /// <c>RunOnStart</c> establecido en true.
    /// </remarks>
    private void RunOnStartWorkers()
    {
        foreach (var worker in _startServicesList.OrderBy(x => x.GetType().Name))
        {
            IHorizonteBackgroundService? hserv = worker as IHorizonteBackgroundService;
            if (hserv == null) return;
            if (hserv.RunOnStart)
            {
                _startlogger.Info("Iniciando Worker: " + hserv.ServiceName);
                worker.StartAsync(CancellationToken.None); // aqui podemos leer la configuración y crear los servicios que se pasan
            }
        }

        _startServicesList.Clear();
    }
}