namespace ConsoleApp1.Definitions;

public class ClientDef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ClientType ClientType { get; set; } = ClientType.Chat;
    public string Server { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public float? Temperature { get; set; }
    public int? MaxOutputTokens { get; set; }
    public bool EnableStreaming { get; set; } = false;
    public bool EnableTools { get; set; } = true;
    public Dictionary<string, string> Settings { get; set; } = new();
}