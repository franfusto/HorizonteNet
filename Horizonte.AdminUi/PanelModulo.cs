using System.Diagnostics;
using Horizonte;
using Horizonte.AdminUi.Components;
using Horizonte.AdminUi.Widgets;
using Horizonte.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Horizonte.AdminUi;

/// <summary>
/// Clase <c>PanelModulo</c> que representa un módulo específico dentro del entorno de administración de la interfaz de usuario de Horizonte.
/// </summary>
/// <remarks>
/// La clase <c>PanelModulo</c> se utiliza para gestionar la lógica de la interfaz de usuario en aplicaciones basadas en Horizonte.
/// Está decorada con el atributo <c>HorizonteModule</c>, que identifica el nombre del módulo como "Horizonte.AdminUi".
/// Esta clase integra varios comandos que permiten la inicialización y configuración de diferentes componentes de la interfaz de usuario.
/// </remarks>
/// <para>
/// Los comandos principales incluyen:
/// - <c>Init</c>: Inicializa el módulo, preparando las configuraciones necesarias para la interfaz de usuario.
/// - <c>ConfigPage</c>: Configura la página principal de la aplicación.
/// - <c>MenuEditor</c>: Edita las configuraciones del menú principal.
/// - <c>NetConfig</c>: Configura las propiedades de red.
/// - <c>OpenGui</c>: Abre la interfaz de usuario.
/// - <c>RenderWidget</c>: Renderiza un widget específico basado en la definición proporcionada.
/// </para>
/// <para>
/// Cada método está complementado con atributos de comando específicos que permiten su integración dentro del marco de Horizonte,
/// facilitando la gestión modular y la extensión de funcionalidades sin modificar el núcleo de la aplicación.
/// </para>
[HorizonteModule("Horizonte.AdminUi")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;
    private HGuiSettings? _settings;

    /// <summary>
    /// Clase <c>PanelModulo</c> que representa un módulo específico dentro del entorno de administración de la interfaz de usuario de Horizonte.
    /// </summary>
    /// <remarks>
    /// La clase está decorada con el atributo <c>HorizonteModule</c> que especifica el nombre del módulo como "Horizonte.AdminUi". Provee varios comandos y funciones
    /// para inicializar y gestionar partes de la interfaz de usuario, tales como configurar páginas, editar el menú y configurar la red.
    /// </remarks>
    public PanelModulo(ILogger<PanelModulo> logger, IhContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Inicia el módulo Horizonte.AdminUi.
    /// </summary>
    /// <remarks>
    /// Este método registra un evento de información en el registro para indicar que el módulo ha sido iniciado
    /// exitosamente. También inicializa los ajustes del GUI al obtener la configuración de contexto disponible
    /// en IhContext. Es parte de los comandos declarados en la clase <c>PanelModulo</c>.
    /// </remarks>
    /// <returns>
    /// Devuelve <c>true</c> si la inicialización se realizó de manera satisfactoria.
    /// </returns>
    [HorizonteRole("init")]
    [HorizonteCommand("HorizonteGui_Init")]
    public bool Init()
    {
        _logger.LogInformation("Horizonte.AdminUi Inciciado");
        _settings = _context.Get<HGuiSettings>();
        return true;
    }

    /// <summary>
    /// Método que retorna una instancia de <see cref="WidgetDef"/> asociada a un panel de aplicación.
    /// </summary>
    /// <returns>
    /// Una instancia de <see cref="WidgetDef"/> que define un panel de aplicación (AppPanel) sin parámetros adicionales.
    /// </returns>
    [HorizonteCommand("HorizonteGui_ConfigPage", "Aplicación")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(AppPanel), Parameters = null };

    /// <summary>
    /// Representa la definición de un widget utilizado para la edición de menús en la interfaz de usuario.
    /// </summary>
    /// <return>Un objeto <see cref="WidgetDef"/> que contiene el tipo <see cref="MenuEditor"/> y parámetros opcionales para la configuración del widget.</return>
    [HorizonteRole("configpage")]
    [HorizonteCommand("HorizonteGui_MenuEditor", "Menu Editor")]
    public WidgetDef MenuEditor() => new WidgetDef() { Type = typeof(MenuEditor), Parameters = null };

    /// <summary>
    /// Método que representa una configuración de red dentro del módulo de la interfaz de administración de Horizonte.
    /// Este método se utiliza para generar un objeto <see cref="WidgetDef"/> que contiene la información esencial para
    /// renderizar un widget de tipo <see cref="NetConfig"/>.
    /// </summary>
    /// <returns>Un objeto <see cref="WidgetDef"/> que define el tipo de widget <see cref="NetConfig"/> sin parámetros adicionales.</returns>
    [HorizonteRole("configpage")]
    [HorizonteCommand("HorizonteGui_NetConfig", "")]
    public WidgetDef NetConfig() => new WidgetDef() { Type = typeof(NetConfig), Parameters = null };

    /// <summary>
    /// El método <c>OpenGui</c> tiene como propósito abrir una interfaz gráfica de usuario en un navegador web.
    /// </summary>
    /// <remarks>
    /// Este método intenta abrir una URL especificada por las configuraciones de la aplicación
    /// contenidas en <c>HGuiSettings</c>. Si la propiedad <c>AppUrl</c> no es nula o vacía,
    /// se inicia un proceso para abrir el navegador en la dirección proporcionada.
    /// En caso de que ocurra una excepción durante este proceso, se registra un error usando un logger.
    /// </remarks>
    [HorizonteCommand("HorizonteGui_OpenGui")]
    public void OpenGui()
    {
        try
        {
            if (!string.IsNullOrEmpty(_settings?.AppUrl))
                Process.Start(new ProcessStartInfo(_settings.AppUrl) { UseShellExecute = true });
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
    }

    /// <summary>
    /// Renderiza un fragmento de interfaz de usuario basándose en la definición de widget proporcionada.
    /// </summary>
    /// <param name="widgetDef">Definición del widget que contiene el tipo y cualquier parámetro necesario para el renderizado.</param>
    /// <return>Un fragmento de renderizado que representa el widget definido.</return>
    [HorizonteCommand("HorizonteGui_RenderWidget",
        description: "Renderiza un widget en base a un WindgetDef proporcionado")]
    public RenderFragment RenderWidget(WidgetDef widgetDef) => (builder) =>
    {
        builder.OpenComponent<Widget>(0);
        builder.AddAttribute(1, "WidgetDef", widgetDef);
        builder.CloseComponent();
    };
   
}