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
    
    
 public static void ConfigureLog4Net(this HostApplicationBuilder builder, HContext context)
    {
        Log4NetSettings logsettings = context.Get<Log4NetSettings>()  ?? new Log4NetSettings() ;
        builder.Logging.ClearProviders();
        ConfigureDefaultsLog4Net(logsettings);
        builder.Logging.AddLog4Net(new Log4NetProviderOptions() { ExternalConfigurationSetup = true });
    }
 
    private static void ConfigureDefaultsLog4Net(Log4NetSettings settings)
    {
        var hierarchy = (Hierarchy)LogManager.GetRepository();
        var patternLayout = new PatternLayout();
        patternLayout.ConversionPattern = settings.RepoConversionPattern;
        patternLayout.ActivateOptions();
        if (settings.EnableFileAppender)
        {
            var roller = new RollingFileAppender
            {
                AppendToFile = true,
                File = settings.FileAppenderFileName,
                Layout = patternLayout,
                MaxSizeRollBackups = settings.FileAppenderMaxSizeRollBackups,
                MaximumFileSize = "10MB",
                RollingStyle = RollingFileAppender.RollingMode.Size, ///////////////////////// desde configueracion
                StaticLogFileName = settings.FileAppenderStaticLogFileName
            };
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);
        }
        if (settings.EnableConsoleAppender)
        {
            var console = new ConsoleAppender
            {
                Layout = patternLayout
            };
            console.ActivateOptions();
            hierarchy.Root.AddAppender(console);
        }
        var memory = new MemoryAppender();
        memory.ActivateOptions();
        hierarchy.Root.AddAppender(memory);
        hierarchy.Root.Level = Level.All; ////////////////////////////////////////////////////////// obtener desde config
        hierarchy.Configured = true;
    }
    public static void ConfigureWorkers(this HostApplicationBuilder builder, WorkerSettings modset,
        IHorizonteEnv env)
    {
        foreach (var workeritem in modset.List)
        {
            var servicetype = Type.GetType(workeritem.WorkerType);
            if (servicetype != null)
            {
                try
                {
                    if (Activator.CreateInstance(servicetype, env, workeritem.ServiceName,
                            workeritem.RunOnStart) is BackgroundService worker)
                    {
                        builder.Services.AddSingleton(worker);
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