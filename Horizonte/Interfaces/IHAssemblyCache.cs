using Horizonte.Entities;

namespace Horizonte.Interfaces;

/// <summary>
/// Interfaz para la caché unificada en memoria (resolución de ensamblados y recursos de versiones).
/// </summary>
public interface IHAssemblyCache
{
    /// <summary>
    /// Obtiene un elemento de caché para el ensamblado, versión y dominio indicados.
    /// </summary>
    AssemblyCacheItem? Get(string assemblyName, string? version = null, string? domainName = null);

    /// <summary>
    /// Agrega o actualiza un elemento de caché de ensamblado.
    /// </summary>
    void Add(AssemblyCacheItem item);

    /// <summary>
    /// Comprueba si existe una entrada en caché para el ensamblado, versión y dominio indicados.
    /// </summary>
    bool Contains(string assemblyName, string? version = null, string? domainName = null);

    /// <summary>
    /// Limpia por completo la caché unificada.
    /// </summary>
    void Clear();

    /// <summary>
    /// Invalida las entradas de caché correspondientes a un dominio específico.
    /// </summary>
    void Clear(string domainName);

    /// <summary>
    /// Obtiene los recursos de versiones descubiertos para un identificador de paquete.
    /// </summary>
    VersionResources? GetVersionResources(string packageId);

    /// <summary>
    /// Almacena los recursos de versiones descubiertos para un identificador de paquete.
    /// </summary>
    void SetVersionResources(string packageId, VersionResources versionResources);
}
