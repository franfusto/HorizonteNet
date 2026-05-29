using ConsoleApp1.Definitions;

namespace ConsoleApp1.Runtime.Files;

internal sealed class HagentFileCodec
{
    public IReadOnlyList<HagentFile> DecodeInputFiles(
        string[]? files,
        AgentFileOptions options)
    {
        // Diseño pendiente.
        return [];
    }

    public string[] EncodeOutputFiles(
        IReadOnlyList<HagentFile> files,
        AgentFileOptions options)
    {
        // Diseño pendiente.
        return [];
    }
}
