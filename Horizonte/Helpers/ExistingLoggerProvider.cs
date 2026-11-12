using Microsoft.Extensions.Logging;

namespace Horizonte;

/// <summary>
/// Proveedor de registro (<see cref="ILoggerProvider"/>) que reutiliza una instancia existente de <see cref="ILogger"/>.
/// </summary>
/// <remarks>
/// Diseñado para integrarse con Microsoft.Extensions.Logging devolviendo la misma instancia compartida de registrador
/// independientemente de la categoría solicitada.
/// </remarks>
public class ExistingLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// Almacena una instancia de <see cref="ILogger"/> utilizada para proporcionar capacidades de registro dentro de la clase <see cref="ExistingLoggerProvider"/>.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ExistingLoggerProvider"/> con un registrador existente.
    /// </summary>
    /// <param name="logger">Instancia de <see cref="ILogger"/> a reutilizar.</param>
    public ExistingLoggerProvider(ILogger logger) => _logger = logger;

    /// <summary>
    /// Crea o devuelve el registrador que es la instancia preexistente del proveedor.
    /// </summary>
    /// <param name="categoryName">El nombre de la categoría del logger.</param>
    /// <returns>Retorna el objeto <see cref="ILogger"/> existente proporcionado al instanciar el proveedor.</returns>
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