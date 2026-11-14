namespace Horizonte.Settings;

public enum BlockedPackagesPolicy
{
    Null,
    Error,
    Mayor
}

public class BlockedPackageItem
{
    public bool Active { get; set; } = true;
    public string PackageId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
