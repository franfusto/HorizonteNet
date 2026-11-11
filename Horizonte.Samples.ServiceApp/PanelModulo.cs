using Horizonte;
using Microsoft.Extensions.Logging;

namespace Horizonte.Samples.ServiceApp;

/// <summary>
/// La clase PanelModulo opera como un módulo dentro del marco Horizonte, destinado a gestionar
/// servicios en la aplicación Horizonte.Samples.ServiceApp. Proporciona métodos para inicializar
/// el servicio y para obtener un widget de configuración asociada con el servicio.
/// </summary>
[HorizonteModule("Horizonte.Samples.ServiceApp")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PanelModulo"/>.
    /// </summary>
    /// <param name="logger">Instancia de <see cref="ILogger{TCategoryName}"/> utilizada para registrar eventos y diagnósticos del módulo.</param>
    public PanelModulo(ILogger<PanelModulo> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Inicializa el módulo Horizonte.Samples.ServiceApp.
    /// Este método es responsable de iniciar el servicio específico del módulo y registra un mensaje informativo en el log.
    /// </summary>
    /// <returns><c>true</c> si el servicio se inicializa correctamente.</returns>
    [HorizonteRole("init")]
    [HorizonteCommand("ServiceApp_Init")]
    public bool Init()
    {
        _logger.LogInformation("Horizonte.Samples.ServiceApp Iniciado");
        return true;
    }


    /// <summary>
    /// Proporciona el widget de configuración para el servicio de ejemplo.
    /// </summary>
    /// <returns>
    /// Una instancia de <see cref="WidgetDef"/> que contiene la información del tipo de componente <see cref="ServiceAppConfigPage"/>.
    /// </returns>
    [HorizonteRole("configpage")]
    [HorizonteCommand("ServiceApp_ConfigPage", "Widget de configuración del servicio de ejemplo")]
    public WidgetDef AppConfig() => new WidgetDef() { Type = typeof(ServiceAppConfigPage), Parameters = null };


}