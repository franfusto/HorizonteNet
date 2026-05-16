using System.Runtime.Loader;
using Horizonte.Services;
using Horizonte.Settings;
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
        builder.Logging.AddLog4Net(new Log4NetProviderOptions() { ExternalConfigurationSetup = true });
    }


    [Obsolete("Utilice IhWorkersManager.ConfigureWorkers() en su lugar.")]
    public static void ConfigureWorkers(this HostApplicationBuilder builder, WorkerSettings modset)
    {
        /*
        var types = (env.AssemblyManager?.Assemblies ?? AssemblyLoadContext.Default.Assemblies)
            .SelectMany(a => a.GetTypes())
            .ToList();
            */
        
        foreach (var workeritem in modset.List.OrderBy(item=>item.Order))
        {
            var servicetype = Type.GetType(workeritem.WorkerType);
            /*
            if (servicetype == null)
            {
                // buscamos el tipo en types
                servicetype = types.FirstOrDefault(t => t.FullName == workeritem.WorkerType || t.Name == workeritem.WorkerType);
            }
            */
            if (servicetype != null)
            {
                try
                {
                    if (ActivatorUtilities.CreateInstance(builder.Services.BuildServiceProvider(), servicetype, workeritem.ServiceName,
                            workeritem.RunOnStart) is BackgroundService worker)
                    {
                        builder.Services.AddSingleton(worker);
                        //env.AddOnStartWorker(worker);
                    }
                }
                catch (Exception e)
                {
                    LogManager.GetLogger(typeof(HostBuilderExtensions)).Error(e);
                }
            }
            else
            {
                LogManager.GetLogger(typeof(HostBuilderExtensions)).Error($"Type '{workeritem.WorkerType}' no encontrado ");
            }
        }

    }
}