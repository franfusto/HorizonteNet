using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte.Extensions;

/// <summary>
/// Proporciona métodos de extensión para configurar un <see cref="Microsoft.Extensions.Hosting.HostApplicationBuilder"/>
/// con funcionalidades adicionales, como el registro con Log4Net y la configuración de servicios de trabajo (worker service).
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    /// Configura el registro con Log4Net para la instancia especificada de HostApplicationBuilder.
    /// </summary>
    /// <param name="builder">La instancia de HostApplicationBuilder para la que se configurará el registro.</param>
    /// <param name="context">La instancia de HContext que contiene Log4NetSettings u otra información de contexto requerida.</param>
    public static void ConfigureLog4Net(this HostApplicationBuilder builder, HContext context)
    {
        Log4NetSettings logsettings = context.Get<Log4NetSettings>()  ?? new Log4NetSettings() ;
        builder.Logging.ClearProviders();
        ConfigureDefaultsLog4Net(logsettings);
        builder.Logging.AddLog4Net(new Log4NetProviderOptions() { ExternalConfigurationSetup = true });
    }

    /// <summary>
    /// Configura los ajustes predeterminados para el registro con Log4Net basado en la instancia proporcionada de Log4NetSettings.
    /// </summary>
    /// <param name="settings">La instancia de Log4NetSettings que contiene la configuración para los apéndices (appenders), niveles de registro y patrones de Log4Net.</param>
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
                MaximumFileSize = "10MB",///////////////////////// desde configueracion
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


    /// <summary>
    /// Configura los trabajadores (workers) para la instancia especificada de HostApplicationBuilder utilizando los WorkerSettings y el entorno proporcionados.
    /// </summary>
    /// <param name="builder">La instancia de HostApplicationBuilder para la que se configurarán los trabajadores.</param>
    /// <param name="modset">La instancia de WorkerSettings que contiene los detalles de configuración para los trabajadores.</param>
    /// <param name="env">La instancia de IHorizonteEnv que representa el entorno en el cual se ejecutará la configuración.</param>
    public static void ConfigureWorkers(this HostApplicationBuilder builder, WorkerSettings modset,
        IHorizonteEnv env)
    {
        foreach (var workeritem in modset.List.OrderBy(item=>item.Order))
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