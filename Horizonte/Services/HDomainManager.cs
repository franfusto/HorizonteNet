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

    /// <summary>
    /// Diccionario de solo lectura con los contextos de dominio activos y registrados.
    /// </summary>
    public IReadOnlyDictionary<string, AssemblyLoadContext> Domains => _domains;

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
        if (assemblySolver != null)
        {
            return assemblySolver.ResolveAssemblyFromALC(context, assemblyName);
        }

        var assemblyManager = _serviceProvider.GetService<IhAssemblyManager>();
        return assemblyManager?.ResolveAssemblyFromALC(context, assemblyName);
    }

    /// <summary>
    /// Obtiene el contexto de carga asociado a un nombre de dominio, retornando el contexto Default si es el dominio predeterminado.
    /// </summary>
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

    /// <summary>
    /// Crea o recupera un contexto de carga de ensamblados para el dominio especificado.
    /// </summary>
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

    /// <summary>
    /// Carga y asegura la existencia de un contexto de dominio para el nombre especificado.
    /// </summary>
    public Task<AssemblyLoadContext> LoadDomainContextAsync(string domainName)
    {
        var alc = CreateDomain(domainName, isCollectible: true);
        return Task.FromResult(alc);
    }

    /// <summary>
    /// Carga un conjunto de ensamblados en memoria dentro del contexto de carga del dominio especificado.
    /// </summary>
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

    /// <summary>
    /// Descarga y libera un contexto de dominio coleccionable, forzando la recolección de basura.
    /// </summary>
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

    /// <summary>
    /// Retorna los ensamblados cargados en el contexto del dominio solicitado.
    /// </summary>
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
