namespace Horizonte.Samples.ServiceApp;

[HorizonteModule("Horizonte.Samples.ServiceApp")]
public class PanelModulo
{
    private Lazy<IHorizonteEnv> _env;
    public PanelModulo(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
    }
    
    [HorizonteRole("init")]
    [HorizonteCommand("ServiceApp_Init")]
    public bool Init()
    {
        return true;
    }
    
    
    [HorizonteRole("configpage")]
    [HorizonteCommand("ServiceApp_ConfigPage", "Widget de configuración del servicio de ejemplo")]
    public WidgetDef AppConfig() => new WidgetDef() { Type = typeof(ServiceAppConfigPage), Parameters = null };


}