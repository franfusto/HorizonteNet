using Horizonte.Entities;

namespace Horizonte.Interfaces;

/// <summary>
/// Interfaz para la caché en memoria y negativa de resolución de ensamblados.
/// </summary>
public interface IHAssemblyCache
{
    AssemblyCacheItem? Get(string assemblyName, string? version = null, string? domainName = null);
    void Add(AssemblyCacheItem item);
    void Clear();
    bool Contains(string assemblyName, string? version = null, string? domainName = null);
}
