using Horizonte.Entities;
using Microsoft.Extensions.Logging;

namespace Horizonte;

/// <summary>
/// Implementa la gestión de workers de Horizonte a partir de la configuración disponible en el contexto.
/// </summary>
/// <remarks>
/// Esta clase resuelve definiciones de workers mediante <see cref="IHContext"/> y delega en
/// <see cref="IhAssemblyManager"/> el inicio, detención y consulta del estado de los servicios
/// en segundo plano. También registra en el sistema de logging los eventos relevantes del ciclo
/// de vida de cada worker.
/// </remarks>
public class HWorkersManager : IhWorkersManager
{
    private readonly IHContext _context;
    private readonly ILogger<HWorkersManager> _logger;
    private readonly IhAssemblyManager _assemblyManager;
    
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="HWorkersManager"/>.
    /// </summary>
    /// <param name="assemblyManager">
    /// Gestor encargado de arrancar, detener y consultar servicios en segundo plano a partir
    /// del tipo configurado para cada worker.
    /// </param>
    /// <param name="context">
    /// Contexto de Horizonte desde el que se obtienen las definiciones de workers disponibles.
    /// </param>
    /// <param name="logger">
    /// Registrador utilizado para informar de errores, advertencias y eventos de ejecución
    /// relacionados con los workers.
    /// </param>

    public HWorkersManager(
        IhAssemblyManager assemblyManager,
        IHContext context,
        ILogger<HWorkersManager> logger)
    {
        _context = context;
        _logger = logger;
        _assemblyManager = assemblyManager;
    }

    /// <inheritdoc />
    public void StartWorkers()
    {
        var workers = _context.Get<WorkerDef>()?.List ?? new List<WorkerDefItem>();

        foreach (var worker in workers.Where(x => x.RunOnStart))
        {
            try
            {
                var started = StartWorker(worker.ServiceName);

                if (!started)
                {
                    _logger.LogWarning("No se pudo iniciar el worker '{ServiceName}'.", worker.ServiceName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar el worker '{ServiceName}'.", worker.ServiceName);
            }
        }
    }

    /// <inheritdoc />
    public List<WorkerDefItem> GetAvailableWorkers()
    {
        return _context.Get<WorkerDef>()?.List ?? new List<WorkerDefItem>();
    }
    
    /// <inheritdoc />
    public bool WorkerIsRunning(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return false;

        var worker = GetWorkerByServiceName(serviceName);
        if (worker == null)
            return false;

        return _assemblyManager.BackgroundServiceRunning(worker.WorkerType);
    }
    
    /// <inheritdoc />
    public bool StartWorker(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return false;

        var worker = GetWorkerByServiceName(serviceName);
        if (worker == null)
        {
            _logger.LogWarning("Worker no encontrado: '{ServiceName}'.", serviceName);
            return false;
        }

        var started = _assemblyManager.StartBackgroundService(worker.WorkerType);

        if (started)
            _logger.LogInformation("Worker iniciado: '{ServiceName}'.", serviceName);
        else
            _logger.LogWarning("No se pudo iniciar el worker: '{ServiceName}'.", serviceName);

        return started;
    }
    /// <inheritdoc />
    public bool StopWorker(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return false;

        var worker = GetWorkerByServiceName(serviceName);
        if (worker == null)
        {
            _logger.LogWarning("Worker no encontrado: '{ServiceName}'.", serviceName);
            return false;
        }

        var stopped = _assemblyManager.StopBackgroundService(worker.WorkerType);

        if (stopped)
            _logger.LogInformation("Worker detenido: '{ServiceName}'.", serviceName);
        else
            _logger.LogWarning("No se pudo detener el worker: '{ServiceName}'.", serviceName);

        return stopped;
    }

    private WorkerDefItem? GetWorkerByServiceName(string serviceName)
    {
        return _context.Get<WorkerDef>()?.List
            .FirstOrDefault(x => string.Equals(x.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase));
    }
}