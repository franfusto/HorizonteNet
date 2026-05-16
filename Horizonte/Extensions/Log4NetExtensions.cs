using System.Net;
using Gelf4Net.Appender;
using Gelf4Net.Layout;
using Horizonte.Settings;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Horizonte.Extensions;

public static class Log4NetExtensions
{
    /// <summary>
    /// Configura la infraestructura de Log4Net a partir de los valores definidos en
    /// una instancia de <see cref="Log4NetSettings"/>.
    /// </summary>
    /// <param name="settings">
    /// Opciones de configuración que determinan el patrón de formato, el nivel raíz
    /// del repositorio y los appenders que deben habilitarse.
    /// </param>
    /// <remarks>
    /// Este método inicializa el repositorio principal de Log4Net y registra, según
    /// la configuración proporcionada, los siguientes destinos de salida:
    /// <list type="bullet">
    /// <item><description>Appender GELF asíncrono para envío remoto de eventos.</description></item>
    /// <item><description>Appender de archivo con rotación.</description></item>
    /// <item><description>Appender de consola.</description></item>
    /// <item><description>Appender en memoria para almacenamiento temporal.</description></item>
    /// </list>
    /// Además, establece el nivel raíz del repositorio usando el valor indicado en
    /// <paramref name="settings"/>. Si el nivel configurado no existe en el mapa de niveles,
    /// se utilizará <see cref="Level.All"/> como valor predeterminado.
    /// </remarks>
    public static void Configure(this Log4NetSettings settings)
    {
        var hierarchy = (Hierarchy)LogManager.GetRepository();
        var patternLayout = new PatternLayout();
        patternLayout.ConversionPattern = settings.RepoConversionPattern;
        patternLayout.ActivateOptions();

        if (settings.EnableGelfAppender)
        {
            var gelfLayout = new GelfLayout
            {
                Facility = settings.GelfFacility,
                IncludeLocationInformation = settings.GelfIncludeLocationInformation,
                SendTimeStampAsString = settings.GelfSendTimeStampAsString,
                AdditionalFields = $"app:{settings.GelfApplicationName},version:1.0,Environment:{settings.GelfEnvironment},Level:%level"
            };
            gelfLayout.ActivateOptions();

            var gelfAppender = new AsyncGelfUdpAppender
            {
                Layout = gelfLayout,
                RemoteAddress = IPAddress.Parse(settings.GelfRemoteHostName),
                RemotePort = settings.GelfRemotePort,
                BufferSize = settings.GelfBufferSize,
                Threads = settings.GelfThreads
            };
            gelfAppender.ActivateOptions();
            hierarchy.Root.AddAppender(gelfAppender);
        }

        if (settings.EnableFileAppender)
        {
            var roller = new RollingFileAppender
            {
                AppendToFile = true,
                File = settings.FileAppenderFileName,
                Layout = patternLayout,
                MaxSizeRollBackups = settings.FileAppenderMaxSizeRollBackups,
                MaximumFileSize = settings.FileAppenderMaximumFileSize,
                RollingStyle = (RollingFileAppender.RollingMode)settings.FileAppenderRollingMode,
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
        
        var repository = LogManager.GetRepository();
        var levelValue = repository.LevelMap[settings.RootLevel];
        if (levelValue != null)
        {
            hierarchy.Root.Level = levelValue;
        }
        else
        {
            hierarchy.Root.Level = Level.All;
        }
        
        hierarchy.Configured = true;
        hierarchy.RaiseConfigurationChanged(EventArgs.Empty);
    }
}
