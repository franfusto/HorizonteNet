using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Horizonte.Extension.AiWorkFlows;

public class AiAgentDef
{
    public string Name { get; set; }= string.Empty;
    public string Model { get; set; }= string.Empty;
    public string EmbedderModel { get; set; }= string.Empty;
    public string Instructions { get; set; }= string.Empty;
    [JsonIgnore] public IList<AITool>? Tools { get; set; }
    
}