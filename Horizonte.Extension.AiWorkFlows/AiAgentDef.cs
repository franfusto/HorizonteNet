using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Horizonte.Extension.AiWorkFlows;

public class AiAgentDef
{
    public string Name { get; set; }
    public string Model { get; set; }
    public string EmbedderModel { get; set; }
    public string Instructions { get; set; }
    [JsonIgnore] public IList<AITool>? Tools { get; set; }
    
}