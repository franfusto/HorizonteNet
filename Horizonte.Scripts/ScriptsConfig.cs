using Horizonte.Extensions.Interactive;

namespace Horizonte.Scripts;

public class ScriptsConfig
{
    public ScriptsConfig()
    {
        Sources.Add(new ScriptsSourceItem { Name = "App", Path = "./scripts" });
        Sources.Add(new ScriptsSourceItem { Name = "Privado", Path = "~/horizonte/scripts" });
    }

    public List<ScriptsSourceItem> Sources { get; set; } = new List<ScriptsSourceItem>();
    public List<ScriptDef> Scripts { get; set; } = new List<ScriptDef>();
}

public class ScriptsSourceItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}
