using System.Collections.Concurrent;
using Horizonte.Entities;
using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;

namespace Horizonte.Services;

/// <summary>
/// Implementación thread-safe de la caché unificada de resolución de ensamblados y recursos de versiones.
/// </summary>
public class HAssemblyCache : IHAssemblyCache
{
    private readonly ILogger<HAssemblyCache>? _logger;
    private readonly ConcurrentDictionary<string, AssemblyCacheItem> _assemblyCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, VersionResources> _versionResourcesCache = new(StringComparer.OrdinalIgnoreCase);

    public HAssemblyCache(ILogger<HAssemblyCache>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtiene un elemento de caché para el ensamblado, versión y dominio especificados.
    /// </summary>
    public AssemblyCacheItem? Get(string assemblyName, string? version = null, string? domainName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyName);

        var key = BuildKey(assemblyName, version, domainName);
        if (_assemblyCache.TryGetValue(key, out var item))
        {
            _logger?.LogDebug("Cache hit para ensamblado '{Key}' (Found={Found})", key, item.Found);
            return item;
        }

        _logger?.LogDebug("Cache miss para ensamblado '{Key}'", key);
        return null;
    }

    /// <summary>
    /// Agrega o actualiza un elemento en la caché de ensamblados.
    /// </summary>
    public void Add(AssemblyCacheItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.AssemblyName);

        var key = BuildKey(item.AssemblyName, item.Version, item.DomainName);
        _assemblyCache[key] = item;
        _logger?.LogDebug("Guardado en caché de ensamblados '{Key}' (Found={Found}, IsResolved={IsResolved})",
            key, item.Found, item.IsResolved);
    }

    /// <summary>
    /// Comprueba si existe una entrada en caché para el ensamblado, versión y dominio indicados.
    /// </summary>
    public bool Contains(string assemblyName, string? version = null, string? domainName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyName);
        var key = BuildKey(assemblyName, version, domainName);
        return _assemblyCache.ContainsKey(key);
    }

    /// <summary>
    /// Limpia todas las entradas de caché de ensamblados y recursos de versiones.
    /// </summary>
    public void Clear()
    {
        _assemblyCache.Clear();
        _versionResourcesCache.Clear();
        _logger?.LogInformation("Caché unificada limpiada por completo.");
    }

    /// <summary>
    /// Invalida las entradas de caché de ensamblados asociadas a un dominio específico.
    /// </summary>
    public void Clear(string domainName)
    {
        if (string.IsNullOrWhiteSpace(domainName)) return;

        var domainPrefix = $"domain:{domainName.Trim()}:".ToLowerInvariant();
        var keysToRemove = _assemblyCache.Keys
            .Where(k => k.StartsWith(domainPrefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _assemblyCache.TryRemove(key, out _);
        }

        _logger?.LogInformation("Invalidadas {Count} entradas de caché para el dominio '{DomainName}'.",
            keysToRemove.Count, domainName);
    }

    /// <summary>
    /// Obtiene los recursos de versión cacheados para un identificador de paquete NuGet.
    /// </summary>
    public VersionResources? GetVersionResources(string packageId)
    {
        if (string.IsNullOrWhiteSpace(packageId)) return null;

        var key = packageId.Trim().ToLowerInvariant();
        if (_versionResourcesCache.TryGetValue(key, out var resources))
        {
            _logger?.LogDebug("Cache hit para VersionResources de paquete '{PackageId}'", packageId);
            return resources;
        }

        return null;
    }

    /// <summary>
    /// Almacena o actualiza los recursos de versiones descubiertos para un identificador de paquete.
    /// </summary>
    public void SetVersionResources(string packageId, VersionResources versionResources)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentNullException.ThrowIfNull(versionResources);

        var key = packageId.Trim().ToLowerInvariant();
        _versionResourcesCache[key] = versionResources;
        _logger?.LogDebug("Guardado VersionResources para paquete '{PackageId}' con {SourceCount} orígenes.",
            packageId, versionResources.Sources.Count);
    }

    private static string BuildKey(string assemblyName, string? version, string? domainName)
    {
        var domain = string.IsNullOrWhiteSpace(domainName) ? "Default" : domainName.Trim();
        var name = assemblyName.Trim();
        var ver = string.IsNullOrWhiteSpace(version) ? "any" : version.Trim();

        return $"domain:{domain}:name:{name}:v:{ver}".ToLowerInvariant();
    }
}
