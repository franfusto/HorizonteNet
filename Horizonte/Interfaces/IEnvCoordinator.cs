using System.Reflection;
using Horizonte.Helpers;

namespace Horizonte.Interfaces;

/// <summary>
/// Interfaz del orquestador central y fachada de compatibilidad para el entorno modular de HorizonteNet.
/// </summary>
public interface IEnvCoordinator
{
    // Acceso a los subsistemas delegados principales
    IHAssemblySolver AssemblySolver { get; }
    IHDomainManager DomainManager { get; }
    IHGesCom GesCom { get; }
    IhWorkersManager WorkersManager { get; }

    // Estado global consolidado (consumido por UI, Scripts, Middleware)
    IReadOnlyList<Assembly> Assemblies { get; }
    IReadOnlyDictionary<string, List<Assembly>> AssembliesByDomain { get; }
    StaticFileRegistry StaticFileRegistry { get; }

    // Eventos de ciclo de vida
    event Action? DomainChanged;

    // Coordinación de ciclo de vida de dominios (Hot Reload y Gestión)
    Task LoadDomainAsync(string domainName);
    Task LoadDomainAsync(string domainName, IEnumerable<byte[]> assemblies);
    Task UnloadDomainAsync(string domainName);
    Task ReloadDomainAsync(string domainName);

    // Métodos de compatibilidad de servicios en segundo plano (Workers)
    bool StartBackgroundService(string serviceType);
    bool StopBackgroundService(string serviceType);
    bool BackgroundServiceRunning(string serviceType);

    // Métodos de compatibilidad para gestión de módulos y comandos por dominio
    void LoadModule(string domainName);
    void UnloadModule(string domainName);
    void LoadCommandsByDomain(string domainName);
    void UnloadCommandsByDomain(string domainName);

    // Instanciación y resolución mediante DI
    object? CreateInstance(Type type);
}
