using Horizonte;

namespace Horizonte;

public class HGuiSettings 
{
   
    public List<HGuiMenuItem> CustomMenuItems { get; set; } = new();
    public bool ProtectApp { get; set; } = false;
    public string AppPassword { get; set; } = "admin";
    public string AppTitle { get; set; } = "Horizonte Net";
    public string DefaultTheme { get; set; } = "humanistic";
    public string HomePageComponent { get; set; } = "HorizonteGui.Components.Elements.StartPage";
    public string HomePageName { get; set; } = "Horizonte Net";
    public string AppUrl { get; set; } = "http://0.0.0.0:5000";
}


