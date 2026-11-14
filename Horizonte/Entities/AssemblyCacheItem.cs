using System.Reflection;

namespace Horizonte.Entities;

/// <summary>
/// Representa un elemento persistido o consultado en la caché de resolución de ensamblados.
/// </summary>
public class AssemblyCacheItem
{
    public string AssemblyName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string DomainName { get; set; } = "Default";
    public string? PackageId { get; set; }
    public string? PackageVersion { get; set; }
    public string? DllPath { get; set; }
    public Assembly? ResolvedAssembly { get; set; }
    public bool IsResolved { get; set; }
    public bool Found { get; set; }
    public DateTime ResolvedAtUtc { get; set; } = DateTime.UtcNow;
}
