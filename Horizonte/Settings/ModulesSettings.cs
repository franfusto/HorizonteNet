namespace Horizonte;

public class ModulesSettings
{
    public ModulesSettings()
    {
        
        List.Add(new ModulesSettingsItem
        {
            Active = true,
            ModuleName = "Horizonte.AdminUi",
            ModuleVersion = "1.0.0",
            Path = null,
            
        });
        
        this.NugetFolders.Add(new NugetFolderItem()
        {
            Folder = "packages",
            Name = "Horizonte",
            Order = 0,
            InstallFolder = true
        });

        this.NugetServers.Add(new NugetServerItem()
        {
            Server = "https://www.nuget.org/api/v2",
            Name = "Nuget.org",
            Version = 2,
            Order = 0,
            Active = true
        });

        this.FrameworkPriorities = new List<string>
        {
            "net10.0",
            "net9.0",
            "net8.0",
            "net7.0",
            "net6.0",
            "netstandard2.1",
            "netstandard2.0",
        };
    }
    
    public List<ModulesSettingsItem> List { get; set; } = [];
    public List<NugetServerItem> NugetServers { get; set; } = new List<NugetServerItem>();
    public List<NugetFolderItem> NugetFolders { get; set; } = new List<NugetFolderItem>();
    public List<string> FrameworkPriorities { get; set; } = new List<string>();
    public List<ForcedPackageItem> ForcedPackages { get; set; } = new List<ForcedPackageItem>();
}

public class ForcedPackageItem
{
    public string PackageId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
}

public class ModulesSettingsItem
{
    public bool Active { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleVersion { get; set; }= string.Empty;
    public string? Path { get; set; }
    public bool? LoadAdditionalDlls { get; set; } = false;
}

public class NugetServerItem
{
    public string Server { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; } = 0;
    public int Version { get; set; } = 2;
    public bool Active { get; set; } = true;
}
public class NugetFolderItem
{
    public string Folder { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; } = 0;
    public bool InstallFolder { get; set; } = false;
    public bool Active { get; set; } = true;
}