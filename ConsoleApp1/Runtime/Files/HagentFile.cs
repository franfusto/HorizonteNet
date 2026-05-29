namespace ConsoleApp1.Runtime.Files;

internal sealed class HagentFile
{
    public string FileName { get; init; } = string.Empty;
    public string MimeType { get; init; } = "application/octet-stream";
    public string Encoding { get; init; } = "base64";
    public byte[] Content { get; init; } = [];

    public Dictionary<string, string> Metadata { get; init; } = new();
}