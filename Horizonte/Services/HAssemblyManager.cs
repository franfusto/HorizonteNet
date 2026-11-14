using Horizonte.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Horizonte.Services;

/// <summary>
/// Clase de compatibilidad hacia atrás.
/// Ha sido reemplazada por <see cref="EnvCoordinator"/>.
/// </summary>
[Obsolete("HAssemblyManager ha sido reemplazado por EnvCoordinator. Utilice IEnvCoordinator o IhAssemblyManager inyectados.")]
public class HAssemblyManager : EnvCoordinator
{
    public HAssemblyManager(
        IHDomainManager domainManager,
        IHAssemblySolver assemblySolver,
        IHGesCom gesCom,
        IhWorkersManager workersManager,
        IServiceProvider serviceProvider,
        ILogger<EnvCoordinator> logger)
        : base(domainManager, assemblySolver, gesCom, workersManager, serviceProvider, logger)
    {
    }

    [Obsolete("Constructor mantenido por compatibilidad. Utilice EnvCoordinator a través de Inyección de Dependencias.")]
    public HAssemblyManager(ISymLinkScafolder linkScafolder, IServiceProvider serviceProvider)
        : base(
            serviceProvider.GetRequiredService<IHDomainManager>(),
            serviceProvider.GetRequiredService<IHAssemblySolver>(),
            serviceProvider.GetRequiredService<IHGesCom>(),
            serviceProvider.GetRequiredService<IhWorkersManager>(),
            serviceProvider,
            serviceProvider.GetRequiredService<ILogger<EnvCoordinator>>())
    {
    }
}
