namespace Horizonte.Extension.Ai.Definitions;


public class RaGdef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public RagType RagType { get; set; } = RagType.InMemory;
    public string EmbedderClient { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
    public int Size { get; set; } = 0;
    public int TopK { get; set; } = 5;
    public float MinScore { get; set; } = 0.0f;
    public int ChunkSize { get; set; } = 1000;
    public int ChunkOverlap { get; set; } = 150;
    public int MaxContextItems { get; set; } = 8;
    public bool EnabledAsTool { get; set; } = true;
    public Dictionary<string, string> Settings { get; set; } = new();
}