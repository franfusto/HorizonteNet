using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Horizonte.Samples.RazorApp.Data;
using Horizonte;
using Horizonte.Samples.RazorApp;
using Horizonte.Samples.RazorApp.Pages;

[HorizonteModule("Horizonte.Samples.RazorApp")]
public class PanelModulo
{
    private Lazy<IHorizonteEnv> _env;
    private ILogger<PanelModulo>? _logger;
    public PanelModulo(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
    }
    
    [HorizonteRole("init")]
    [HorizonteCommand("RazorApp_Init")]
    public bool Init()
    {
        _logger = _env.Value.GetService<ILogger<PanelModulo>>();
        return true;
    }
    
    
    [HorizonteRole("configpage")]
    [HorizonteCommand("RazorApp_ConfigPage", "Widget de configuración de la aplicación Razor de ejemplo")]
    public WidgetDef AppConfig() => new WidgetDef() { Type = typeof(AppConfig), Parameters = null };



}