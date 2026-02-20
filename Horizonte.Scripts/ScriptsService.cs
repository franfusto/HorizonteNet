using Horizonte.Extensions.Interactive;

namespace Horizonte.Scripts;

/// <summary>
/// 
/// </summary>
public class ScriptsService
{
    
    public Dictionary<string, IEnumerable<ScriptDef>> Scripts { get; set; } = new Dictionary<string, IEnumerable<ScriptDef>>();
    
    
}