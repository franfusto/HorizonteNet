using System.Reflection;
using System.Runtime.Loader;
using Horizonte.Interfaces;
using Horizonte.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Horizonte.Services;

/// <summary>
/// Gestiona directamente los contextos de carga de ensamblados (AssemblyLoadContext) y el ciclo de vida de los dominios aislados.
/// </summary>
public class HDomainManager : IHDomainManager
{
    private readonly ILogger<HDomainManager> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ModulesSettings _settings;
    private readonly Dictionary<string, AssemblyLoadContext> _domains = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, AssemblyLoadContext> Domains => _domains;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="HDomainManager"/>.
    /// </summary>
    /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
    /// <param name="logger">Servicio de registro de eventos para el gestor de dominios.</param>
    public HDomainManager(IServiceProvider serviceProvider, ILogger<HDomainManager> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = serviceProvider.GetService<IhContext>()?.Get<ModulesSettings>() ?? new ModulesSettings();

        SetUpConfiguredDomains();
    }

    private void SetUpConfiguredDomains()
    {
        try
        {
            foreach (var domainName in _settings.Domains)
            {
                if (string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
                {
                    AssemblyLoadContext.Default.Resolving -= OnResolving;
                    AssemblyLoadContext.Default.Resolving += OnResolving;
                    continue;
                }

                if (_domains.ContainsKey(domainName))
                    continue;

                _logger.LogInformation("Creando dominio (ALC): {DomainName}", domainName);
                CreateDomain(domainName, isCollectible: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configurando dominios iniciales.");
        }
    }

    private Assembly? OnResolving(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        var assemblySolver = _serviceProvider.GetService<IHAssemblySolver>();
        return assemblySolver?.ResolveAssemblyFromALC(context, assemblyName);
    }

    /// <inheritdoc/>
    public AssemblyLoadContext? GetDomain(string domainName)
    {
        if (string.IsNullOrWhiteSpace(domainName) ||
            string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
        {
            return AssemblyLoadContext.Default;
        }

        if (_domains.TryGetValue(domainName, out var alc))
        {
            return alc;
        }

        return null;
    }

    /// <inheritdoc/>
    public AssemblyLoadContext CreateDomain(string domainName, bool isCollectible = true)
    {
        if (string.IsNullOrWhiteSpace(domainName) ||
            string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
        {
            return AssemblyLoadContext.Default;
        }

        if (_domains.TryGetValue(domainName, out var existingAlc))
        {
            return existingAlc;
        }

        _logger.LogInformation("Creating domain AssemblyLoadContext: {DomainName} (isCollectible: {IsCollectible})", domainName, isCollectible);
        var alc = new AssemblyLoadContext(domainName, isCollectible: isCollectible);
        alc.Resolving += OnResolving;
        _domains[domainName] = alc;
        return alc;
    }

    /// <inheritdoc/>
    public Task<AssemblyLoadContext> LoadDomainContextAsync(string domainName)
    {
        var alc = CreateDomain(domainName, isCollectible: true);
        return Task.FromResult(alc);
    }

    /// <inheritdoc/>
    public Task<AssemblyLoadContext> LoadDomainContextAsync(string domainName, IEnumerable<byte[]> assemblyBytes)
    {
        var alc = CreateDomain(domainName, isCollectible: true);

        if (assemblyBytes != null)
        {
            foreach (var asmData in assemblyBytes)
            {
                if (asmData == null || asmData.Length == 0)
                    continue;

                try
                {
                    using var ms = new MemoryStream(asmData);
                    alc.LoadFromStream(ms);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading assembly from stream into domain {DomainName}", domainName);
                }
            }
        }

        return Task.FromResult(alc);
    }

    /// <inheritdoc/>
    public Task UnloadDomainContextAsync(string domainName)
    {
        if (string.IsNullOrWhiteSpace(domainName) ||
            string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        if (_domains.TryGetValue(domainName, out var alc))
        {
            _logger.LogInformation("Unloading domain ALC: {DomainName}", domainName);
            alc.Resolving -= OnResolving;
            alc.Unload();
            _domains.Remove(domainName);

            // Forzar recolección de basura para liberar el ALC coleccionable
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public IEnumerable<Assembly> GetDomainAssemblies(string domainName)
    {
        if (string.IsNullOrWhiteSpace(domainName) ||
            string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
        {
            return AssemblyLoadContext.Default.Assemblies;
        }

        if (_domains.TryGetValue(domainName, out var alc))
        {
            return alc.Assemblies;
        }

        return Enumerable.Empty<Assembly>();
    }
}
