using Horizonte.Extension.Ai.Definitions;

namespace Horizonte.Ai.Agents;

public class AgentsConfig
{
    public ConfigSet ConfigSet { get; set; } =  new ConfigSet();
    public ChatConfig ChatConfig { get; set; } =  new ChatConfig();
    
}
public class ChatConfig
{
    public string Name { get; set; } = string.Empty;
    public string SelectedId { get; set; } =  string.Empty;
}