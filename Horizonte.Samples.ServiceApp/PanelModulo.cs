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
    /// La clase PanelModulo sirve como un módulo dentro del marco Horizonte.
    /// </summary>
    public PanelModulo(ILogger<PanelModulo> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Método inicializador del módulo Horizonte.Samples.ServiceApp.
    /// Este método es responsable de iniciar el servicio específico del módulo,
    /// y registra un mensaje informativo en el log cuando se inicia exitosamente.
    /// </summary>
    /// <returns>Devuelve true si el servicio se inicia correctamente.</returns>
    [HorizonteRole("init")]
    [HorizonteCommand("ServiceApp_Init")]
    public bool Init()
    {
        _logger.LogInformation("Horizonte.Samples.ServiceApp Iniciado");
        return true;
    }


    /// <summary>
    /// Método que proporciona un widget de configuración para el servicio de ejemplo.
    /// </summary>
    /// <returns>
    /// Una instancia de <c>WidgetDef</c> que contiene la información del tipo del widget
    /// como <c>ServiceAppConfigPage</c> y ningún parámetro adicional.
    /// </returns>
    [HorizonteRole("configpage")]
    [HorizonteCommand("ServiceApp_ConfigPage", "Widget de configuración del servicio de ejemplo")]
    public WidgetDef AppConfig() => new WidgetDef() { Type = typeof(ServiceAppConfigPage), Parameters = null };


}