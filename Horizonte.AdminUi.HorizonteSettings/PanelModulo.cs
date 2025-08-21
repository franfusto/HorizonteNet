using Horizonte;
using Horizonte.AdminUi.HorizonteSettings.Widgets;
using Microsoft.Extensions.Logging;

namespace Horizonte.AdminUi.HorizonteSettings;

[HorizonteModule("Horizonte.AdminUi.HorizonteSettings")]
public class PanelModulo
{
    private IHorizonteEnv _env;
    private ILogger<PanelModulo>? _logger;

    public PanelModulo(IHorizonteEnv env)
    {
        _env = env;
    }

    //widgets

    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_Workers",
        "Este widget permite ver y descargar el contexto o archivo de configuración de la aplicación")]
    public WidgetDef Workers() => new WidgetDef() { Type = typeof(Workers), Parameters = null };

    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_ViewContext",
        "Este widget permite ver y descargar el contexto o archivo de configuración de la aplicación")]
    public WidgetDef ViewContext() => new WidgetDef() { Type = typeof(Context), Parameters = null };

    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_Modules", "")]
    public WidgetDef Modules() => new WidgetDef() { Type = typeof(Modules), Parameters = null };
    /*
    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_Repository", "")]
    public WidgetDef Repository() => new WidgetDef() { Type = typeof(Repository), Parameters = null };    
    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_NewModule", "este widget permite crear un nuevo módulo")]
    public WidgetDef NewModule() => new WidgetDef() { Type = typeof(NewModule), Parameters = null };    
    [HorizonteRole("configpage")]
    [HorizonteCommand("HorizonteConfig_ModulesConfig", "este widget permite instalar, desintalar, o cambiar la versión de los módulos")]
    public WidgetDef ModulesConfig() => new WidgetDef() { Type = typeof(ModulesConfig), Parameters = null };    
    */
    
    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_Commands", "")]
    public WidgetDef Commands() => new WidgetDef() { Type = typeof(Commands), Parameters = null };    
    
    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_Log", "este widget permite ver el registro de la aplicación")]
    public WidgetDef Log() => new WidgetDef() { Type = typeof(Log), Parameters = null };    
    
    //config
    
   
    [HorizonteRole("configpage")]
    [HorizonteCommand("HorizonteConfig_OnStartCommands", "")]
    public WidgetDef OnStartCommands() => new WidgetDef() { Type = typeof(OnStartCommands), Parameters = null };
    
    [HorizonteRole("configpage")]
    [HorizonteCommand("HorizonteConfig_OnEndCommands", "")]
    public WidgetDef OnEndCommands() => new WidgetDef() { Type = typeof(OnEndCommands), Parameters = null };
       
    
    [HorizonteRole("configpage")]
    [HorizonteCommand("HorizonteConfig_WorkersConfig", "")]
    public WidgetDef WorkersConfig() => new WidgetDef() { Type = typeof(WorkersConfig), Parameters = null };
           

    
    
}