using System.Reflection;

namespace Horizonte.Extensions.Interactive;

public class ScriptDefCompileResponse
{
    public byte[] Assembly { get; set; } = Array.Empty<byte>();
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}
