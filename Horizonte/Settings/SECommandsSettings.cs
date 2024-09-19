namespace Horizonte;

public class SECommandsSettings
{
    public List<SECommandsSettingsItem> StartCommands { get; set; } = new List<SECommandsSettingsItem>() { new SECommandsSettingsItem(){CommandName = "dd1",Active = true} };
    public List<SECommandsSettingsItem> EndCommands { get; set; } = new List<SECommandsSettingsItem>() { new SECommandsSettingsItem(){CommandName = "dd1dd",Active = true} };
}

public class SECommandsSettingsItem
{
    public string CommandName { get; set; } = "sub";
    public bool Active { get; set; } = false;
    
}