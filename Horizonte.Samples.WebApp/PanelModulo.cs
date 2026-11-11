using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;

namespace Horizonte.Samples.WebApp;

/// <summary>
/// Representa el módulo de la aplicación web de ejemplo dentro del marco Horizonte.
/// </summary>
/// <remarks>
/// Identificado mediante el atributo <see cref="HorizonteModule"/>, este módulo proporciona los puntos
/// de entrada para inicializar la aplicación web de ejemplo en el entorno de Horizonte.
/// </remarks>
[HorizonteModule("Horizonte.Samples.WebApp")]
public class PanelModulo
{

    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PanelModulo"/>.
    /// </summary>
    /// <param name="logger">Instancia de <see cref="ILogger{TCategoryName}"/> utilizada para registrar eventos y diagnósticos del módulo.</param>
    /// <param name="context">Instancia de <see cref="IhContext"/> que proporciona acceso al contexto y configuración de Horizonte.</param>
    public PanelModulo(ILogger<PanelModulo> logger, IhContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Inicializa el módulo de aplicación web Horizonte.
    /// </summary>
    /// <remarks>
    /// Este método es invocado por el marco Horizonte para inicializar el módulo y registra un mensaje informativo en el log.
    /// </remarks>
    /// <returns><c>true</c> si el módulo se inicializa correctamente.</returns>
    [HorizonteRole("init")]
    [HorizonteCommand("WebApp_Init")]
    public bool Init()
    {
        _logger.LogInformation("WebApp Inciciado");
        return true;
    }

}