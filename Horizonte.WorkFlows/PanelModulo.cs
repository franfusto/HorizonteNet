using Horizonte;
using Horizonte.WorkFlows.Widgets;

namespace Horizonte.WorkFlows;

[HorizonteModule("Horizonte.WorkFlows")]
public class PanelModulo
{
    private readonly IHorizonteEnv _env;

    public PanelModulo(IHorizonteEnv env)
    {
        _env = env;
    }

    [HorizonteRole("init")]
    [HorizonteCommand("WorkFlows_Init")]
    public bool Init()
    {
        return true;
    }
    
    [HorizonteRole("widget")]
    [HorizonteCommand("WorkFlows_Dashboard", "")]
    public WidgetDef ScriptsViewerWidget() => new WidgetDef() { Type = typeof(Dashboard), Parameters = null };

    
}
