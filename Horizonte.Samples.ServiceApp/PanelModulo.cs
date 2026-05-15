using Horizonte;
using Microsoft.Extensions.Logging;

namespace Horizonte.Samples.ServiceApp;

[HorizonteModule("Horizonte.Samples.ServiceApp")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    public PanelModulo(ILogger<PanelModulo> logger)
    {
        _logger = logger;
    }
    
    [HorizonteRole("init")]
    [HorizonteCommand("ServiceApp_Init")]
    public bool Init()
    {
        _logger.LogInformation("Horizonte.Samples.ServiceApp Iniciado");
        return true;
    }
    
    
    [HorizonteRole("configpage")]
    [HorizonteCommand("ServiceApp_ConfigPage", "Widget de configuración del servicio de ejemplo")]
    public WidgetDef AppConfig() => new WidgetDef() { Type = typeof(ServiceAppConfigPage), Parameters = null };


}