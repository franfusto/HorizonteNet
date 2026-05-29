namespace ConsoleApp1.Definitions;

public class ServerDef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 60;
    public Dictionary<string, string> Headers { get; set; } = new();
    public Dictionary<string, string> Settings { get; set; } = new();
}











