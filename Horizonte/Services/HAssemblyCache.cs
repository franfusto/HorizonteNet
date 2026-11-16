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

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="HAssemblyCache"/>.
    /// </summary>
    /// <param name="logger">Instancia opcional del registrador de eventos.</param>
    public HAssemblyCache(ILogger<HAssemblyCache>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void Add(AssemblyCacheItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.AssemblyName);

        var key = BuildKey(item.AssemblyName, item.Version, item.DomainName);
        _assemblyCache[key] = item;
        _logger?.LogDebug("Guardado en caché de ensamblados '{Key}' (Found={Found}, IsResolved={IsResolved})",
            key, item.Found, item.IsResolved);
    }

    /// <inheritdoc/>
    public bool Contains(string assemblyName, string? version = null, string? domainName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyName);
        var key = BuildKey(assemblyName, version, domainName);
        return _assemblyCache.ContainsKey(key);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _assemblyCache.Clear();
        _versionResourcesCache.Clear();
        _logger?.LogInformation("Caché unificada limpiada por completo.");
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
