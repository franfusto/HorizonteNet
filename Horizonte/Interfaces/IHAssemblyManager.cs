using System.Reflection;
using System.Runtime.Loader;

namespace Horizonte;


public interface IhAssemblyManager
{

    public List<Assembly> Assemblies { get; }
    public Dictionary<string, List<Assembly>> AssembliesByDomain { get; }
    public event Action? DomainChanged;

    public Assembly? ResolveAssemblyFromALC(AssemblyLoadContext context, AssemblyName assemblyName);
    public Assembly? ResolveAssemblyFromNuGetPackages(object? sender, ResolveEventArgs args);
    public string? ResolveAssemblyDllPath(string packageName, string version);

    public Task UnloadDomain(string domainName);
    public Task LoadDomain(string domainName);
    public Task LoadDomain(string domainName, IEnumerable<byte[]> assemblies);
    public Task ReloadDomain(string domainName);


    public void UnloadModule(string domainName);
    public void UnloadService(string domainName);
    public void LoadModule(string domainName);
    public void LoadService(string domainName);
    public StaticFileRegistry StaticFileRegistry { get; }
    
    public void UnloadCommandsByDomain(string domainName);
    public void LoadCommandsByDomain(string domainName);

    public object? CreateInstance(Type type);
    
    public bool StartBackgroundService(string serviceType);
    public bool StopBackgroundService(string serviceType);
    public bool BackgroundServiceRunning(string serviceType);
}