using Horizonte;
using HorizonteGui.Components.Elements;

namespace HorizonteGui;


[HorizonteModule("HorizonteGui")]
public class PanelModulo
{
    private Lazy<IHorizonteEnv> _env;
    private ILogger<PanelModulo>? _logger;
    //private IConfigurationRoot? _context;
    public PanelModulo(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
    }
    
    [HorizonteCommand("HorizonteGui_Command1")]
    public DateTime Command1()
    {
        _logger?.LogInformation("desde el módulo HorizonteGui");
        return  DateTime.Now;
    }
    
    [HorizonteRole("init")]
    [HorizonteCommand("HorizonteGui_Init")]
    public bool Init()
    {
        _logger =_env.Value.HHost.Services.GetService<ILogger<PanelModulo>>();
        //_context =_env.Value.HHost.Services.GetService<IConfigurationRoot>();
        _logger?.LogInformation("HorizonteGui Inciciado");
        /*
        HGuiSettingsItem? modset = _context?.GetSection("HGuiSettings").Get<HGuiSettingsItem>();
        if (modset == null)
        {
            modset = new HGuiSettingsItem();
            _context?.GetSection("HGuiSettings").Set(modset);
        }
        */
        return true;
       
    }
    [HorizonteRole("configpage")]
    [HorizonteCommand("HorizonteGui_ConfigPage","Aplicación")]
    public string ConfigPage() => typeof(AppPanel).FullName!;
    


}