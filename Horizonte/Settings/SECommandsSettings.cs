namespace Horizonte;

public class SeCommandsSettings
{
    public List<SeCommandsSettingsItem> StartCommands { get; set; } =
        [new SeCommandsSettingsItem() { CommandName = "cmd", Active = false }];
    public List<SeCommandsSettingsItem> EndCommands { get; set; } =
        [new SeCommandsSettingsItem() { CommandName = "cmd", Active = false }];
}

public class SeCommandsSettingsItem
{
    public string CommandName { get; set; } = "cmd";
    public bool Active { get; set; } = false;
    
}