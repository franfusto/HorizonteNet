namespace Horizonte;

public interface IHModManager
{
    public void RemoveFiles(ModulesSettingsItem item);
    public bool InstallFromFile(string filename);
    public bool InstallFromRepo(string filename);
    public List<ModulesSettingsItem> GetRepoFiles();
    public byte[] BuildModule(NewModuleConfig newModuleConfig);

}