using System.Reflection;
using System.Runtime.Loader;
using Horizonte.Helpers;

namespace Horizonte.Interfaces;

/// <summary>
/// Interfaz para el subsistema de resolución de dependencias, descarga de paquetes NuGet, caché y activos web.
/// </summary>
public interface IHAssemblySolver
{
    // Registro de archivos estáticos y activos web
    StaticFileRegistry StaticFileRegistry { get; }

    // Resolución de ensamblados
    Assembly? ResolveAssemblyFromALC(AssemblyLoadContext context, AssemblyName assemblyName);
    Assembly? ResolveAssemblyFromNuGetPackages(object? sender, ResolveEventArgs args);
    string? ResolveAssemblyDllPath(string packageName, string version);

    // Gestión de paquetes y NuGet
    Task<string?> DownloadNuGetPackageAsync(string packageName, string version);
    string GetPackageName(string assemblyName);

    // Caché de ensamblados
    void CacheAssembly(string key, byte[] assemblyBytes);
    byte[]? GetCachedAssembly(string key);
    bool IsAssemblyCached(string key);
}
