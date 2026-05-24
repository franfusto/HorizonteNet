using Microsoft.Extensions.Logging;

namespace Horizonte;

/// La clase `ExistingLoggerProvider` implementa la interfaz `ILoggerProvider` y se utiliza para proporcionar una instancia existente de `ILogger`.
/// Esta clase está diseñada para integrarse con el sistema de registro de Microsoft Extensions Logging, permitiendo que un `ILogger` existente sea utilizado como proveedor de registro.
/// El principal propósito de esta clase es reutilizar un `ILogger` pre-existente en lugar de crear uno nuevo para cada categoría de registro que se requiera.
/// ### Constructores
/// - `ExistingLoggerProvider(ILogger logger)`: Crea una nueva instancia de `ExistingLoggerProvider` utilizando la instancia de `ILogger` proporcionada.
/// ### Métodos
/// - `ILogger CreateLogger(string categoryName)`: Devuelve la instancia de `ILogger` que fue pasada al constructor, independientemente del nombre de categoría proporcionado.
/// - `void Dispose()`: Método vacío para cumplir con la interfaz `IDisposable`. No realiza ninguna acción ya que no hay recursos que liberar.
/// Esta clase es particularmente útil en aplicaciones que alojan servicios o interfaces web, donde se necesita un mecanismo de registro centralizado o compartido entre diferentes componentes del sistema.
/// /
public class ExistingLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// Almacena una instancia de <see cref="ILogger"/> utilizada para proporcionar capacidades de registro dentro de la clase <see cref="ExistingLoggerProvider"/>.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Proveedor de registro que implementa la interfaz <c>ILoggerProvider</c>.
    /// </summary>
    /// <remarks>
    /// Esta clase permite reutilizar un logger existente pasando una instancia de <c>ILogger</c> al constructor.
    /// </remarks>
    public ExistingLoggerProvider(ILogger logger) => _logger = logger;

    /// <summary>
    /// Crea un logger que es una instancia del proveedor de loggers existente.
    /// </summary>
    /// <param name="categoryName">El nombre de la categoría del logger que se está creando. Aunque este parámetro es requerido, no afecta al logger devuelto ya que siempre se retorna el logger existente proporcionado al instanciar el proveedor.</param>
    /// <return>Retorna un objeto <see cref="ILogger"/> que es el logger existente proporcionado inicialmente al proveedor.</return>
    public ILogger CreateLogger(string categoryName) => _logger;

    /// <summary>
    /// Libera los recursos utilizados por el proveedor de registros ExistingLoggerProvider.
    /// </summary>
    /// <remarks>
    /// Este método se utiliza para liberar cualquier recurso no administrado o cualquier otra limpieza necesaria
    /// antes de que el objeto ExistingLoggerProvider sea destruido.
    /// </remarks>
    public void Dispose() { }
}