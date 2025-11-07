using Horizonte;
using Microsoft.Extensions.Logging;

namespace TestConfig;

[HorizonteModule("TestConfig")]
public class PanelModulo
{
    private ILogger<PanelModulo>? _logger;
    private Lazy<IHorizonteEnv> _env;
    public PanelModulo(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
    }
    [HorizonteRole("init")]
    [HorizonteCommand("TestConfig_Init")]
    public bool Init()
    {
        _logger = _env.Value.GetService<ILogger<PanelModulo>>();
        var context = _env.Value.GetService<IHContext>();
        //config = context?.Get<GmapsConfig>() ?? new GmapsConfig();
        _logger?.LogInformation("Módulo TestConfig Inciciado");
        
        
        return true;
    }
    [HorizonteRole("configpage")]
    [HorizonteCommand("TestConfig_ConfigPage", "Widget de configuración del módulo TestConfig")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(TestConfigConfig), Parameters = null };
}