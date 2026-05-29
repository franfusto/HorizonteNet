using Horizonte;
using Microsoft.Extensions.Logging;
using Gtk;
using Horizonte.Interfaces;

namespace Horizonte.Samples.NotificationIcon;

/// <summary>
/// La clase <c>PanelModulo</c> gestiona las operaciones relacionadas con el icono de notificación dentro del módulo
/// Horizonte.Samples.NotificationIcon.
/// </summary>
/// <remarks>
/// <c>PanelModulo</c> está equipada con atributos como <c>HorizonteModule</c>, permitiendo ejecutar sus métodos
/// como comandos del sistema Horizonte. Los métodos están decorados con atributos adicionales como
/// <c>HorizonteCommand</c> y <c>HorizonteRole</c>, especificando el comportamiento y los permisos del contexto del módulo.
/// Esta clase emplea un <c>IconManager</c> para manejar la visibilidad del icono de notificación según las configuraciones
/// proporcionadas. Además, proporciona acceso a la página de configuración asociada al icono.
/// Los métodos públicos habilitan la inicialización del módulo, la visibilidad del icono de notificación, y la obtención
/// de información sobre la configuración. Su funcionalidad depende de las configuraciones de <c>NotiIconSettings</c>.
/// </remarks>
[HorizonteModule("Horizonte.Samples.NotificationIcon")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;
    private readonly IHGesCom _gescom;
    private IconManager? _iconManager;

    /// <summary>
    /// Representa un módulo de panel dentro del contexto de iconos de notificación en la aplicación.
    /// </summary>
    /// <remarks>
    /// Esta clase está marcada como un módulo de Horizonte y proporciona funcionalidad relacionada
    /// con la gestión de iconos de notificación, incluyendo su inicialización, visualización y
    /// ocultación, así como el acceso a la página de configuración del icono.
    /// </remarks>
    /// <param name="logger">
    /// El logger utilizado para registrar mensajes de diagnóstico o información dentro del módulo.
    /// </param>
    /// <param name="context">
    /// El contexto de la aplicación que proporciona métodos y propiedades necesarios para el
    /// funcionamiento del módulo.
    /// </param>
    /// <param name="gescom">
    /// La interfaz de gestión de comunicación utilizada para interactuar con otros componentes en
    /// el entorno Horizonte.
    /// </param>
    public PanelModulo(ILogger<PanelModulo> logger, IhContext context, IHGesCom gescom)
    {
        _logger = logger;
        _context = context;
        _gescom = gescom;
    }

    /// <summary>
    /// Inicializa el módulo de iconos de notificación utilizando la configuración disponible en el contexto.
    /// </summary>
    /// <returns>
    /// Devuelve un valor booleano que indica si la inicialización se ha realizado de manera exitosa.
    /// </returns>
    [HorizonteRole("init")]
    [HorizonteCommand("HNotiIcon_Init")]
    public bool Init()
    {
        NotiIconSettings settings = _context.Get<NotiIconSettings>() ?? new NotiIconSettings();
        _iconManager = new IconManager(settings, _logger, _gescom);
        _logger.LogInformation("Horizonte.Samples.NotificationIcon Inciciado");
        return true;
    }

    /// <summary>
    /// Muestra el icono de notificación utilizando el gestor de iconos disponible en el módulo.
    /// </summary>
    [HorizonteCommand("HNotiIcon_ShowIcon")]
    public void ShowIcon() => _iconManager?.ShowIcon();

    /// <summary>
    /// Oculta el icono de notificación si está visible actualmente en el sistema.
    /// </summary>
    [HorizonteCommand("HNotiIcon_HideIcon")]
    public void HideIcon() => _iconManager?.HideIcon();


    /// <summary>
    /// Recupera el nombre calificado del ensamblado de la página de configuración del icono de notificación.
    /// </summary>
    /// <returns>
    /// Devuelve una cadena que representa el nombre completamente calificado del tipo de configuración del icono de notificación.
    /// </returns>
    [HorizonteRole("configpage")]
    [HorizonteCommand("HNotiIcon_ConfigPage","Icono de notificación")]
    public string ConfigPage() => typeof(NotiIconConfig).AssemblyQualifiedName!;
}