namespace Horizonte;

public interface IHGesCom
{
    public void InitzializeModules();
    public List<HGesCom.RoleListItem> GetRoleCommands(string role);
    object? RunCommand(string commandKeyor, object[]? arg = null);
    T? RunCommand<T>(string commandKeyor, object[]? arg = null);
    public string? RunCommandJson(string commandKey, string[] jsonarglist);
    public IEnumerable<HCommand> GetCommandList();
}