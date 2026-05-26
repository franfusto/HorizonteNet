using Horizonte;
using Microsoft.Extensions.Logging;
using Gtk;
using Horizonte.Interfaces;

namespace Horizonte.Samples.NotificationIcon;


[HorizonteModule("Horizonte.Samples.NotificationIcon")]
public class PanelModulo
{

    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;
    private readonly IHGesCom _gescom;
    private IconManager? _iconManager;

    public PanelModulo(ILogger<PanelModulo> logger, IhContext context, IHGesCom gescom)
    {
        _logger = logger;
        _context = context;
        _gescom = gescom;
    }

    [HorizonteRole("init")]
    [HorizonteCommand("HNotiIcon_Init")]
    public bool Init()
    {
        NotiIconSettings settings = _context.Get<NotiIconSettings>() ?? new NotiIconSettings();
        _iconManager = new IconManager(settings, _logger, _gescom);
        _logger.LogInformation("Horizonte.Samples.NotificationIcon Inciciado");
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