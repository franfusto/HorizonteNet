namespace Horizonte;

public class SymLinkDef
{
    public string  Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public bool IsDirecory { get; set; } = false;
    public bool HardCoded { get; set; } = true;
}