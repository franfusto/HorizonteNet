namespace ConsoleApp1.Runtime.Files;

internal sealed class HagentFileEnvelope
{
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = "application/octet-stream";
    public string Encoding { get; set; } = "base64";
    public string Content { get; set; } = string.Empty;
}