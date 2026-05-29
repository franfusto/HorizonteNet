using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;

namespace Horizonte.Samples.WebApp;

/// <summary>
/// La clase PanelModulo representa un módulo dentro de la aplicación Horizonte.Samples.WebApp.
/// Está marcada con el atributo <see cref="HorizonteModule"/> para identificarla como parte del marco Horizonte.
/// Proporciona funcionalidad de inicialización de la aplicación mediante el método <c>Init</c>.
/// </summary>
[HorizonteModule("Horizonte.Samples.WebApp")]
public class PanelModulo
{

    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;

    /// <summary>
    /// Clase que representa un módulo dentro de la aplicación WebApp del marco Horizonte.
    /// </summary>
    /// <remarks>
    /// Esta clase está destinada a ser utilizada como un módulo en el entorno de ejecución de Horizonte,
    /// proporcionando funcionalidad específica para la inicialización de la aplicación web.
    /// </remarks>
    public PanelModulo(ILogger<PanelModulo> logger, IhContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Método que inicia el módulo de aplicación web Horizonte. Al ser invocado, registra una información de que el módulo ha sido iniciado exitosamente.
    /// </summary>
    /// <returns>Devuelve un valor booleano indicando el éxito del inicio del módulo, que en este caso es siempre verdadero.</returns>
    [HorizonteRole("init")]
    [HorizonteCommand("WebApp_Init")]
    public bool Init()
    {
        _logger.LogInformation("HWebApp Inciciado");
        return true;
    }

}