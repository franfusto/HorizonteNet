namespace Horizonte.Extension.Ai.Definitions;

public class AgentDef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public string ChatClientId { get; set; } = string.Empty;
    public string[] Tools { get; set; } = [];
    public AgentFileOptions FileOptions { get; set; } = new();
    public AgentExecutionOptions ExecutionOptions { get; set; } = new();
    public Dictionary<string, string> Settings { get; set; } = new();
}