using Horizonte;
using Horizonte.Interfaces;
using Horizonte.PorcupineMod.Widgets;
using Microsoft.Extensions.Logging;

namespace Horizonte.PorcupineMod;

[HorizonteModule("Horizonte.PorcupineMod")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;

    public PanelModulo(ILogger<PanelModulo> logger, IhContext context)
    {
        _logger = logger;
        _context = context;
    }
    [HorizonteRole("init")]
    [HorizonteCommand("Porcupine_Init")]
    public bool Init()
    {
        _logger.LogInformation("Módulo Porcupine Iniciado");
        return true;
    }
    /*
    */
    [HorizonteRole("configpage")]
    [HorizonteCommand("Porcupine_ConfigPage", "Widget de configuración del módulo Porcupine")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(PorcupineCfgWidget), Parameters = null };

}