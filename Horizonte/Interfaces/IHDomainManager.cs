using System.Reflection;
using System.Runtime.Loader;

namespace Horizonte.Interfaces;

/// <summary>
/// Interfaz para la gestión directa de contextos de carga (AssemblyLoadContext) y aislamiento de dominios.
/// </summary>
public interface IHDomainManager
{
    IReadOnlyDictionary<string, AssemblyLoadContext> Domains { get; }
    
    AssemblyLoadContext? GetDomain(string domainName);
    AssemblyLoadContext CreateDomain(string domainName, bool isCollectible = true);
    
    Task<AssemblyLoadContext> LoadDomainContextAsync(string domainName);
    Task<AssemblyLoadContext> LoadDomainContextAsync(string domainName, IEnumerable<byte[]> assemblyBytes);
    Task UnloadDomainContextAsync(string domainName);

    IEnumerable<Assembly> GetDomainAssemblies(string domainName);
}
