namespace ConsoleApp1.Definitions;

public class AgentToolDef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AgentToolType ToolType { get; set; } = AgentToolType.RagSearch;
    public bool Enabled { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 60;
    public string? ClientId { get; set; }
    public string? RagId { get; set; }
    public Dictionary<string, string> Settings { get; set; } = new();
}