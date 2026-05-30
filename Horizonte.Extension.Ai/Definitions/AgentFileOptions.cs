namespace Horizonte.Extension.Ai.Definitions;

public class AgentFileOptions
{
    public bool AllowInputFiles { get; set; } = true;
    public bool AllowOutputFiles { get; set; } = true;
    public string TransportEncoding { get; set; } = "base64";
    public int MaxInputFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    public int MaxOutputFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    public string[] AllowedInputMimeTypes { get; set; } = [];
    public string[] AllowedOutputMimeTypes { get; set; } = [];
    public bool AutoProcessImages { get; set; } = true;
    public bool AutoProcessDocuments { get; set; } = false;
    public Dictionary<string, string> Settings { get; set; } = new();
}