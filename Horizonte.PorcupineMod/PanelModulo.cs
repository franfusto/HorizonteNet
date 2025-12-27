using Horizonte.PorcupineMod.Widgets;
using Microsoft.Extensions.Logging;

namespace Horizonte.PorcupineMod;

[HorizonteModule("Horizonte.PorcupineMod")]
public class PanelModulo
{
    private Lazy<IHorizonteEnv> _env;
    private ILogger<PanelModulo>? _logger;
    public PanelModulo(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
    }
    [HorizonteRole("init")]
    [HorizonteCommand("Porcupine_Init")]
    public bool Init()
    {
        _logger = _env.Value.GetService<ILogger<PanelModulo>>();
        var context = _env.Value.GetService<IHContext>();
        _logger?.LogInformation("Módulo Porcupine Inciciado");
        return true;
    }
    /*
    */
    [HorizonteRole("configpage")]
    [HorizonteCommand("Porcupine_ConfigPage", "Widget de configuración del módulo Porcupine")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(PorcupineCfgWidget), Parameters = null };

}