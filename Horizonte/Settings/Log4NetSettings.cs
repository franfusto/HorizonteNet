
namespace Horizonte.Settings;

/// <summary>
/// Representa la configuración de salida y comportamiento del sistema de registro basado en Log4Net.
/// </summary>
/// <remarks>
/// Esta clase agrupa las opciones utilizadas para configurar los appenders disponibles
/// del sistema de logging, incluyendo salida a fichero, consola y GELF.
/// Sus valores son consumidos durante la inicialización del entorno para construir
/// dinámicamente la configuración del repositorio de Log4Net.
/// </remarks>
public class Log4NetSettings
{
    /// <summary>
    /// Patrón de conversión utilizado por el repositorio para formatear los mensajes
    /// en los appenders que emplean <c>PatternLayout</c>.
    /// </summary>
    public string RepoConversionPattern { get; set; } = "[%thread]  %-5p %d %5rms %-22.22c{2} %-18.18M - %m%n";
    /// <summary>
    /// Nivel raíz de logging que se aplicará al repositorio.
    /// </summary>
    /// <remarks>
    /// Debe coincidir con un nivel reconocido por Log4Net, por ejemplo <c>All</c>,
    /// <c>Debug</c>, <c>Info</c>, <c>Warn</c>, <c>Error</c> o <c>Fatal</c>.
    /// Si el valor no existe en el mapa de niveles, se utilizará <c>All</c>.
    /// </remarks>
    public string RootLevel { get; set; } = "All";

    /// <summary>
    /// Indica si debe habilitarse el appender de escritura a fichero.
    /// </summary>
    public bool EnableFileAppender { get; set; } = true;

    /// <summary>
    /// Indica si debe habilitarse el appender de salida por consola.
    /// </summary>
    public bool EnableConsoleAppender { get; set; } = true;

    /// <summary>
    /// Nombre o ruta del fichero de log utilizado por el appender de fichero.
    /// </summary>
    public string FileAppenderFileName { get; set; } = "horizonte.log";

    /// <summary>
    /// Número máximo de copias de respaldo que conservará el appender rotativo de fichero.
    /// </summary>
    public int FileAppenderMaxSizeRollBackups { get; set; } = 10;

    /// <summary>
    /// Tamaño máximo del fichero antes de realizar una rotación.
    /// </summary>
    /// <remarks>
    /// Se expresa en un formato entendido por Log4Net, por ejemplo <c>2MB</c> o <c>10MB</c>.
    /// </remarks>
    public string FileAppenderMaximumFileSize { get; set; } = "2MB";

    /// <summary>
    /// Modo de rotación del fichero de log.
    /// </summary>
    /// <remarks>
    /// Este valor se convierte a <c>RollingFileAppender.RollingMode</c>.
    /// Debe corresponder con un valor válido de esa enumeración.
    /// </remarks>
    public int FileAppenderRollingMode { get; set; } = 1;

    /// <summary>
    /// Indica si el nombre del fichero de log principal permanece estático al rotar.
    /// </summary>
    public bool FileAppenderStaticLogFileName { get; set; } = true;

    /// <summary>
    /// Indica si debe habilitarse el appender GELF para envío remoto de logs.
    /// </summary>
    public bool EnableGelfAppender { get; set; } = true;

    /// <summary>
    /// Dirección IP o nombre de host del servidor remoto que recibirá los mensajes GELF.
    /// </summary>
    public string GelfRemoteHostName { get; set; } = "127.0.0.1";

    /// <summary>
    /// Puerto UDP del receptor GELF remoto.
    /// </summary>
    public int GelfRemotePort { get; set; } = 12201;

    /// <summary>
    /// Nombre de la aplicación que se incluirá como metadato adicional en los mensajes GELF.
    /// </summary>
    public string GelfApplicationName { get; set; } = "Horizonte";

    /// <summary>
    /// Nombre del entorno de ejecución que se añadirá a los mensajes GELF.
    /// </summary>
    /// <remarks>
    /// Suele utilizarse para distinguir entornos como <c>dev</c>, <c>test</c> o <c>prod</c>.
    /// </remarks>
    public string GelfEnvironment { get; set; } = "dev";

    /// <summary>
    /// Tamaño del búfer utilizado por el appender GELF asíncrono.
    /// </summary>
    public int GelfBufferSize { get; set; } = 2000;

    /// <summary>
    /// Número de hilos utilizados para el envío asíncrono de mensajes GELF.
    /// </summary>
    public int GelfThreads { get; set; } = 2;

    /// <summary>
    /// Nombre de la facility que se asociará a los mensajes GELF.
    /// </summary>
    public string GelfFacility { get; set; } = "Horizonte";

    /// <summary>
    /// Indica si deben incluirse datos de ubicación, como clase o método de origen,
    /// en los mensajes GELF.
    /// </summary>
    public bool GelfIncludeLocationInformation { get; set; } = true;

    /// <summary>
    /// Indica si la marca temporal debe enviarse como cadena en los mensajes GELF.
    /// </summary>
    public bool GelfSendTimeStampAsString { get; set; } = true;
}

