using Horizonte.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Horizonte;
using log4net.Appender;
using log4net.Core;
using Microsoft.Extensions.Logging;

namespace Horizonte;

public class HorizonteEnv : IHorizonteEnv
{
    public IHost HHost { get; private set; }
    public List<string> AssetsFolders { get; set; }= new List<string>();
    public string RootPath { get; private set; }

    private HostApplicationBuilder? _builder { get; set; }
    private readonly string[] _args;
    public string Contextname { get; private set; }
    private HContext _context;
    private ModulesSettings _modulesSettings;
    private WorkerSettings _workerSettings;
    private Log4NetSettings _log4NetSettings;
    private IHGesCom? _gescom;
    private IhAssemblyManager? _assemblyManager;
    private List<BackgroundService> _startServicesList = new();
    private CancellationToken _cts = new CancellationToken();

    public HorizonteEnv(string contextname, string[] appargs)
    {
        _args = appargs;
        Contextname = contextname;

        Stage1(); // Cargar contexto
        Stage2(); // Cargar emsamblados
        Stage3(); // Cargar modulos 
        Stage4(); // Crear Host
        Stage5(); // Iniciar Modulos
        Stage6(); // Iniciar Workers
        Stage7(); // Iniciar Host 
    }

    public T? GetService<T>()
    {
        return HHost.Services.GetService<T>();
    }
    
    public T? GetRequiredService<T>(T value) where T : Type
    {
        return HHost.Services.GetRequiredService<T>();
    }
    
    public void Reboot()
    {
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = false;
            var appexe =Environment.GetCommandLineArgs()[0]; // añadir resto de argumentos de inicio
            string extension = "";
            if (OperatingSystem.IsWindows()) extension = ".exe";
            Thread.Sleep(5000); // sacar a configuración
            System.Diagnostics.Process.Start(appexe.Replace(".dll",extension));
        }).Start();
        Quit();
    }

    public  void Quit()
    {
        var services= HHost.Services.GetServices<BackgroundService>();
        foreach (var service in services)
        {
            service.StopAsync(new CancellationToken());
        }
        
        HHost.StopAsync(new CancellationToken());
    }


    //stage 1 - cargar contexto
    private void Stage1()
    {
        try
        {
            Console.WriteLine("******** STAGE 1 - LOAD CONTEXT **********");

            //establecemos ruta de trabajo
            RootPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])!;
            Directory.SetCurrentDirectory(RootPath);

            //creamos contexto y obtenemos configuraciones
            _context = new HContext(Contextname);
            _modulesSettings = _context.Get<ModulesSettings>() ?? new ModulesSettings();
            _log4NetSettings = _context.Get<Log4NetSettings>() ?? new Log4NetSettings();
            _workerSettings = _context.Get<WorkerSettings>() ?? new WorkerSettings();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    //stage 2 - cargar ensamblados
    private void Stage2()
    {
        Console.WriteLine("******** STAGE 2 - LOAD ASSEMBLIES **********");
        try
        {
            _assemblyManager = new HAssemblyManager(_modulesSettings,this);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    //stage 2 - cargar módulos
    private void Stage3()
    {
        Console.WriteLine("******** STAGE 3 - LOAD MODULES **********");
        try
        {
            _gescom = new HGesCom(this);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    //stage 4 - Crear Host
    private void Stage4()
    {
        try
        {
            Console.WriteLine("******** STAGE 4 - CREATE HHOST **********");
            //creamos host
            _builder = Host.CreateEmptyApplicationBuilder(null);

            //contexto
            _builder.Configuration.Sources.Clear();
            _context = new HContext(Contextname);
            _builder.Services.AddSingleton<IHContext>(_context);

            //ensamblados
            if (_assemblyManager != null) _builder.Services.AddSingleton(_assemblyManager);

            //log 
            _builder.ConfigureLog4Net(_context);

            // traducciones
            _builder.Services.AddSingleton<IHtrans, Htrans>();

            //gestor de entorno
            _builder.Services.AddSingleton<IHorizonteEnv>(this);

            //gestor de commandos
            if (_gescom != null) _builder.Services.AddSingleton(_gescom);

            //gestión de módulos
            _builder.Services.AddSingleton<IHModManager, HModManager>();

            //Workers
            _builder.ConfigureWorkers(_workerSettings, this);

            //Host Build
            HHost = _builder.Build();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    //stage5  - Iniciar módulos
    private void Stage5()
    {
        Console.WriteLine("******** STAGE 6 - INITIALIZE MODULES **********");
        try
        {
            _gescom?.InitzializeModules();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    //stage 6 - Iniciar Workers
    private void Stage6()
    {
        Console.WriteLine("******** STAGE 6 -  WORKERS **********");
        try
        {
            RunOnStartWorkers();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    //stage 7 - Iniciar Host
    private void Stage7()
    {
        Console.WriteLine("******** STAGE 8 - RUN HOST **********");
        try
        {
            HHost.Run();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void AddOnStartWorker(BackgroundService worker)
    {
        _startServicesList.Add(worker);
    }


    private void RunOnStartWorkers()
    {
        foreach (var worker in _startServicesList)
        {
            IHservice? hserv = worker as IHservice;
            if (hserv == null) return;
            if (hserv.RunOnStart)
            {
                Console.WriteLine("Iniciando Worker: " + hserv.ServiceName);
                worker.StartAsync(new CancellationToken());
            }
        }

        _startServicesList.Clear();
    }
}