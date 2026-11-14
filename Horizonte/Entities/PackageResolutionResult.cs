namespace Horizonte.Entities;

public class PackageResolutionResult
{
    public bool Success { get; set; }
    public string? PackageId { get; set; }
    public string? Version { get; set; }
    public string? DllPath { get; set; }
    public string? ErrorMessage { get; set; }
    public bool Blocked { get; set; }
}
