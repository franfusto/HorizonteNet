namespace Horizonte.Extension.Ai.Definitions;

public class AgentExecutionOptions
{
    public int MaxTurns { get; set; } = 8;
    public int MaxToolCalls { get; set; } = 5;
    public int TimeoutSeconds { get; set; } = 120;
    public bool AutoInvokeTools { get; set; } = true;
    public bool ReturnToolMessages { get; set; } = true;
    public bool ReturnTraceSteps { get; set; } = false;
    public Dictionary<string, string> Settings { get; set; } = new();
}