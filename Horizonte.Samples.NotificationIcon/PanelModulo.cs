using Horizonte;
using Microsoft.Extensions.Logging;
using Gtk;

namespace Horizonte.Samples.NotificationIcon;


[HorizonteModule("Horizonte.Samples.NotificationIcon")]
public class PanelModulo
{

    private IHorizonteEnv _env;
    private IconManager? _iconManager;

    public PanelModulo(IHorizonteEnv env)
    {
        _env = env;
    }

    [HorizonteRole("init")]
    [HorizonteCommand("HNotiIcon_Init")]
    public bool Init()
    {
        var logger = _env.GetService<ILogger<PanelModulo>>();
        var context = _env.GetService<IHContext>();
        var gescom = _env.GetService<IHGesCom>();
        NotiIconSettings settings = context?.Get<NotiIconSettings>() ?? new NotiIconSettings();
        _iconManager = new IconManager(settings,logger,gescom);
        logger?.LogInformation("Horizonte.Samples.NotificationIcon Inciciado");
        return true;
    }

    [HorizonteCommand("HNotiIcon_ShowIcon")]
    public void ShowIcon() => _iconManager?.ShowIcon();

    [HorizonteCommand("HNotiIcon_HideIcon")]
    public void HideIcon() => _iconManager?.HideIcon();


    [HorizonteRole("configpage")]
    [HorizonteCommand("HNotiIcon_ConfigPage","Icono de notificación")]
    public string ConfigPage() => typeof(NotiIconConfig).AssemblyQualifiedName!;
}