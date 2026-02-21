using System.Reflection;
using System.Runtime.Loader;

namespace Horizonte;

/// <summary>
/// Clase abstracta que define el modelo para gestionar ensamblados en la aplicación.
/// </summary>
public interface IhAssemblyManager
{
    /// <summary>
    /// 
    /// </summary>
    public List<Assembly> Assemblies { get; }
    public Dictionary<string, List<Assembly>> AssembliesByDomain { get; }
    public event Action? DomainChanged;

    public Assembly? ResolveAssemblyFromALC(AssemblyLoadContext context, AssemblyName assemblyName);
    public Assembly? ResolveAssemblyFromNuGetPackages(object? sender, ResolveEventArgs args);
    public string? ResolveAssemblyDllPath(string packageName, string version);

    public void UnloadDomain(string domainName);
    public void LoadDomain(string domainName);
    public void ReloadDomain(string domainName);

    public void UnloadModule(string domainName);
    public void UnloadService(string domainName);
    public void LoadModule(string domainName);
    public void LoadService(string domainName);
}