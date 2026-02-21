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

    public Assembly? ResolveAssemblyFromALC(AssemblyLoadContext context, AssemblyName assemblyName);
    public Assembly? ResolveAssemblyFromNuGetPackages(object? sender, ResolveEventArgs args);
    public string? ResolveAssemblyDllPath(string packageName, string version);
}