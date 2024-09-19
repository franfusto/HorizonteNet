using log4net.Appender;
using log4net.Core;

namespace Horizonte;
[HConfigOption]
public class Log4NetSettings
{

    public string RepoConversionPattern { get; set; } = "[%thread]  %-5p %d %5rms %-22.22c{2} %-18.18M - %m%n";
    public string RootLevel { get; set; } = "All";
    public bool EnableFileAppender { get; set; } = true;
    public bool EnableConsoleAppender { get; set; } = true;
    public string FileAppenderFileName { get; set; } = "horizonte.log";
    public int FileAppenderMaxSizeRollBackups { get; set; } = 10;
    public string FileAppenderMaximumFileSize { get; set; } = "2MB";
    public int FileAppenderRollingMode { get; set; } = 1;
    public bool FileAppenderStaticLogFileName { get; set; } = true;
}

