using System.Text.Json.Serialization;

namespace ConsoleApp1.Runtime.Files;

internal sealed class HagentFileEnvelope
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = "application/octet-stream";

    [JsonPropertyName("encoding")]
    public string Encoding { get; set; } = "base64";

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}