using Gtk;
using Horizonte;
using Microsoft.Extensions.Logging;

namespace HNotiIcon;

public class IconManager(NotiIconSettings notiIconSettings, ILogger? logger, IHGesCom? gesCom)
{
#pragma warning disable CS0612 // Type or member is obsolete
    private bool _running;
    private StatusIcon? _icon;
    private Menu? _icoMenu;


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
#pragma warning restore CS0612 // Type or member is obsolete
}