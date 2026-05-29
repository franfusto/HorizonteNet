using Gtk;
using Horizonte;
using Microsoft.Extensions.Logging;

namespace Horizonte.Samples.NotificationIcon;

/// <summary>
/// La clase IconManager es responsable de manejar la visualización y ocultación del icono de notificación en el sistema operativo.
/// Utiliza <c>NotiIconSettings</c> para la configuración del icono y del menú asociado, y puede utilizar un <c>ILogger</c> para registrar eventos y errores.
/// </summary>
/// <remarks>
/// Esta clase inicia y gestiona un ícono de notificación en el sistema operativo al ejecutar una aplicación interna mediante tareas asincrónicas.
/// </remarks>
/// <param name="notiIconSettings">
/// Configuración del icono de notificación, incluyendo el título, recursos y elementos del menú.
/// </param>
/// <param name="logger">
/// (Opcional) Instancia de <c>ILogger</c> para registrar eventos tales como errores o acciones importantes.
/// </param>
/// <param name="gesCom">
/// (Opcional) Interfaz que proporciona funcionalidades para ejecutar y administrar comandos en base a roles.
/// </param>
/// <example>
/// La clase se utiliza típicamente en combinación con otras clases de inicialización de módulos para configurar y mostrar iconos de notificación.
/// </example>
public class IconManager(NotiIconSettings notiIconSettings, ILogger? logger, IHGesCom? gesCom)
{
    private bool _running;
    private StatusIcon? _icon;
    private Menu? _icoMenu;


    /// <summary>
    /// Muestra el icono de notificación en el sistema operativo.
    /// Este método prepara y ejecuta el entorno necesario para la presentación del icono.
    /// Si ya se está ejecutando, no realiza ninguna acción.
    /// Inicia el sistema de aplicaciones, configura el menú relacionado y el icono,
    /// y luego ejecuta la aplicación para que el icono permanezca visible hasta que se decida ocultar.
    /// Cualquier error durante este proceso se registra utilizando el logger disponible.
    /// </summary>
    public void ShowIcon()
    {
        if (_running) return;
        _running = true;
        try
        {
            Task.Run(() =>
            {
                Application.Init();
                SetUpMenu();
                SetUpIcon();
                Application.Run();
            });
        }
        catch (Exception e)
        {
            logger?.LogError(e.ToString());
        }
    }

    /// <summary>
    /// Oculta el icono de notificación que actualmente se muestra en el sistema operativo.
    /// Si el icono no está en ejecución o si no ha sido inicializado, el método no realiza ninguna acción.
    /// Una vez que el icono es oculto, también se libera de la memoria mediante el método Dispose()
    /// y la aplicación se cierra llamando a Application.Quit().
    /// Cualquier error que ocurra durante este proceso se registra utilizando el logger disponible.
    /// </summary>
    public void HideIcon()
    {
        if (!_running) return;
        if (_icon == null) return;
        _running = false;
        try
        {
            _icon.Visible = false;
            _icon.Dispose();
            Application.Quit();
        }
        catch (Exception e)
        {
            logger?.LogError(e.ToString());
        }
    }

    private void SetUpIcon()
    {
        _icon = new StatusIcon(Gdk.Pixbuf.LoadFromResource(notiIconSettings.IconResource));
        _icon.Visible = true;
        _icon.Title = notiIconSettings.IconTitle;
        _icon.Activate += OnActivate;
        _icon.PopupMenu += OnPopUp;
    }
    
    private void SetUpMenu()
    {
        _icoMenu = new Menu();
        foreach (var item in notiIconSettings.MenuItems)
        {
            var menuItem = new ImageMenuItem(item.Label);
            var appimg = new Gtk.Image(item.StockIcon, IconSize.Menu);
            menuItem.Activated += delegate { gesCom?.RunCommand(item.Command); };
            menuItem.Image = appimg;
            _icoMenu.Add(menuItem);
        }
    }

    private void OnPopUp(object? o, EventArgs e)
    {
        _icoMenu?.ShowAll();
        _icoMenu?.Popup();
    }

    private void OnActivate(object? o, EventArgs e)
    {
        if (!string.IsNullOrEmpty(notiIconSettings.OnActivatedCommand))
            gesCom?.RunCommand(notiIconSettings.OnActivatedCommand);
    }
}