using System.Reflection;

namespace Horizonte.Extensions.Interactive;

public class ScriptDefCompileResponse
{
    public Assembly? Assembly { get; set; }
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}
