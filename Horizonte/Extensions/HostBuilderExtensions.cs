using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte.Extensions;

public static class HostBuilderExtensions
{
    
    
    //esto a otro archivo.....
    public static T DeepClone<T>(this T obj)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(obj, typeof(T));
        return System.Text.Json.JsonSerializer.Deserialize<T>(json);
			
    }


    
    public static void ConfigureLog4Net(this HostApplicationBuilder builder, HContext _context)
    {
        Log4NetSettings logsettings = _context.Get<Log4NetSettings>()  ?? new Log4NetSettings() ;

        builder.Logging.ClearProviders();
        ConfigureDefaultsLog4Net(logsettings);
        builder.Logging.AddLog4Net(new Log4NetProviderOptions() { ExternalConfigurationSetup = true });
    }
    private static void ConfigureDefaultsLog4Net(Log4NetSettings settings)
    {
        Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

        PatternLayout patternLayout = new PatternLayout();
        patternLayout.ConversionPattern = settings.RepoConversionPattern;
        patternLayout.ActivateOptions();

        if (settings.EnableFileAppender)
        {
            RollingFileAppender roller = new RollingFileAppender();
            roller.AppendToFile = true;
            roller.File = settings.FileAppenderFileName;
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = settings.FileAppenderMaxSizeRollBackups;
            roller.MaximumFileSize = "10MB";
            roller.RollingStyle = RollingFileAppender.RollingMode.Size; ///////////////////////// desde configueracion
            roller.StaticLogFileName = settings.FileAppenderStaticLogFileName;
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);
        }

        if (settings.EnableConsoleAppender)
        {
            ConsoleAppender console = new ConsoleAppender();
            console.Layout = patternLayout;
            console.ActivateOptions();
            hierarchy.Root.AddAppender(console);
        }


        MemoryAppender memory = new MemoryAppender();
        memory.ActivateOptions();
        hierarchy.Root.AddAppender(memory);

        hierarchy.Root.Level = Level.All; ////////////////////////////////////////////////////////// obtener desde config
        hierarchy.Configured = true;
    }
    public static void ConfigureWorkers(this HostApplicationBuilder builder, WorkerSettings modset,
        IHorizonteEnv env)
    {

        Type? servicetype;
        BackgroundService? worker;
        foreach (var workeritem in modset.List)
        {
            servicetype = Type.GetType(workeritem.WorkerType);
            if (servicetype != null)
            {
                try
                {
                    worker = Activator.CreateInstance(servicetype, env, workeritem.ServiceName,
                        workeritem.RunOnStart) as BackgroundService;
                    if (worker != null)
                    {
                        builder.Services.AddSingleton<BackgroundService>(worker);
                        env.AddOnStartWorker(worker);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                Console.WriteLine($"Type '{workeritem.WorkerType}' no encontrado ");
            }
        }

    }
}