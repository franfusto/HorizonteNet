namespace HNotiIcon;

public class NotiIconSettings
{
    public NotiIconSettings()
    {
        MenuItems.Add(new IconMenuItem { Label = "Quit", StockIcon = "gtk-quit", Command = "Horizonte_Quit" });
        MenuItems.Add(new IconMenuItem { Label = "Horizonte Net", StockIcon = "gtk-home", Command = "HorizonteGui_OpenGui" });
    }

    public string IconTitle { get; set; } = "Horizonte Net";
    public string? OnActivatedCommand { get; set; }
    public string IconResource { get; set; } = "HNotiIcon.icons.home.png";
    public List<IconMenuItem> MenuItems { get; set; } = new List<IconMenuItem>();
    
    
}

public class IconMenuItem()
{
    public string Label { get; set; } = string.Empty;
    public string StockIcon { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
}