namespace Horizonte;

public class ModulesSettings
{
    public ModulesSettings()
    {
        this.ModulesPath = "./mod";
        
        List.Add(new ModulesSettingsItem
        {
            Active = true,
            ModuleName = "HorizonteGui",
            ModuleVersion = "0.1.0",
            Path = "/home/Datos/KONEK/HorizonteNet/HorizonteGui/bin/Debug/net8.0/HorizonteGui.dll"
        });
        List.Add(new ModulesSettingsItem
        {
            Active = true,
            ModuleName = "Modulo1",
            ModuleVersion = "0.1.0",
            Path = "/home/Datos/KONEK/HorizonteNet/Modulo1/bin/Debug/net8.0/Modulo1.dll"
        });

    }
    
    public string ModulesPath { get; set; } = string.Empty;
    public string RepoPath { get; set; } = string.Empty;
    public List<ModulesSettingsItem> List { get; set; } = new();
}

public class ModulesSettingsItem
{
    public bool Active { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleVersion { get; set; }= string.Empty;
    public string? Path { get; set; }
}