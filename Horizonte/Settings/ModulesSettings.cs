namespace Horizonte;

public class ModulesSettings
{
    public ModulesSettings()
    {
        this.ModulesPath = "mod";
        
        List.Add(new ModulesSettingsItem
        {
            Active = true,
            ModuleName = "HorizonteGui",
            ModuleVersion = "1.0.0",
            Path = null
        });
    }
    
    public string ModulesPath { get; set; }
    public string RepoPath { get; set; } = string.Empty;
    public List<ModulesSettingsItem> List { get; set; } = [];
}

public class ModulesSettingsItem
{
    public bool Active { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleVersion { get; set; }= string.Empty;
    public string? Path { get; set; }
}