namespace Horizonte;

public class NewModuleConfig
{
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleVersion { get; set; } = string.Empty;
    public string BinPath { get; set; } = string.Empty;
    public List<string> RootFolders { get; set; } = new();
}