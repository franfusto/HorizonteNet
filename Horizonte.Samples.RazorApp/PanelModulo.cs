using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Horizonte.Samples.RazorApp.Data;
using Horizonte;
using Horizonte.Samples.RazorApp;
using Horizonte.Samples.RazorApp.Pages;

[HorizonteModule("Horizonte.Samples.RazorApp")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    public PanelModulo(ILogger<PanelModulo> logger)
    {
        _logger = logger;
    }
    
    [HorizonteRole("init")]
    [HorizonteCommand("RazorApp_Init")]
    public bool Init()
    {
        _logger.LogInformation("Horizonte.Samples.RazorApp Iniciado");
        return true;
    }
    
    
    [HorizonteRole("configpage")]
    [HorizonteCommand("RazorApp_ConfigPage", "Widget de configuración de la aplicación Razor de ejemplo")]
    public WidgetDef AppConfig() => new WidgetDef() { Type = typeof(AppConfig), Parameters = null };



}