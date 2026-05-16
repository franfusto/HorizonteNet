using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte;

public class HWorkersManager : IhWorkersManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHContext _context;
    private readonly ILogger<HWorkersManager> _logger;
    private readonly List<BackgroundService> _managedWorkers = new();
    private readonly IhAssemblyManager _assemblyManager;

    public HWorkersManager(IServiceProvider serviceProvider, IHContext context, ILogger<HWorkersManager> logger)
    {
        _serviceProvider = serviceProvider;
        _context = context;
        _logger = logger;
        _assemblyManager = _serviceProvider.GetService<IhAssemblyManager>();
    }

    public void ConfigureWorkers()
    {
        _managedWorkers.Clear();
        var workerSettings = _context.Get<WorkerSettings>() ?? new WorkerSettings();

        foreach (var workerItem in workerSettings.List.OrderBy(item => item.Order))
        {
            /*
            Type? serviceType = Type.GetType(workerItem.WorkerType);
            
            if (serviceType == null && assemblyManager != null)
            {
                // Intentar buscar en los ensamblados cargados si no se encuentra por nombre completo
                var typeName = workerItem.WorkerType.Split(',')[0].Trim();
                serviceType = assemblyManager.Assemblies
                    .Select(a => a.GetType(typeName))
                    .FirstOrDefault(t => t != null);
            }
            */
            var typeName = workerItem.WorkerType.Split(',')[0].Trim();
            Type? serviceType = null;

            if (_assemblyManager != null)
            {
                serviceType = _assemblyManager.Assemblies
                    .Select(a => a.GetType(typeName, throwOnError: false, ignoreCase: false))
                    .FirstOrDefault(t => t != null);
            }

            serviceType ??= AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(typeName, throwOnError: false, ignoreCase: false))
                .FirstOrDefault(t => t != null);

            serviceType ??= Type.GetType(workerItem.WorkerType, throwOnError: false);

            if (serviceType != null)
            {
                try
                {
                    // Intentamos crear la instancia usando ActivatorUtilities para soportar DI
                    var worker = ActivatorUtilities.CreateInstance(_serviceProvider, serviceType, workerItem.ServiceName, workerItem.RunOnStart) as BackgroundService;
                    if (worker != null)
                    {
                        _managedWorkers.Add(worker);
                        _logger.LogInformation($"Worker '{workerItem.ServiceName}' ({serviceType.Name}) configurado correctamente.");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error al configurar el worker '{workerItem.ServiceName}' de tipo '{workerItem.WorkerType}'");
                }
            }
            else
            {
                _logger.LogWarning($"Tipo de worker '{workerItem.WorkerType}' no encontrado.");
            }
        }
    }

    public void StartWorkers()
    {
        foreach (var worker in _managedWorkers)
        {
            if (worker is IHorizonteBackgroundService hService && hService.RunOnStart)
            {
                _logger.LogInformation($"Iniciando worker: {hService.ServiceName}");
                //worker.StartAsync(CancellationToken.None);
                worker.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            else
            {
                _logger.LogInformation($"Worker configurado pero no marcado para iniciar automáticamente: {worker.GetType().Name}");
            }
        }
    }

    public void StopWorkers()
    {
        foreach (var worker in _managedWorkers)
        {
            worker.StopAsync(CancellationToken.None).Wait();
        }
    }

    public List<BackgroundService> GetRunningWorkers()
    {
        return _managedWorkers;
    }
}
