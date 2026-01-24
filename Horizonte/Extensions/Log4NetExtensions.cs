using System.Net;
using Gelf4Net.Appender;
using Gelf4Net.Layout;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Horizonte.Extensions;

public static class Log4NetExtensions
{
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
