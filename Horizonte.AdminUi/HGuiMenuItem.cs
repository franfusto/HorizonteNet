namespace Horizonte;


public class HGuiMenu
{
    public string Caption { get; set; } = string.Empty;
    public List<HGuiMenuItem> Items { get; set; } = new List<HGuiMenuItem>();
}

public class HGuiMenuItem
{
    public int Id { get; set; } = 0;
    public int ParentId { get; set; } = 0;
    public string Icon { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
        
}