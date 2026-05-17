using Horizonte.Entities;

namespace Horizonte;

/// <summary>
/// Define las operaciones para consultar, iniciar y detener los workers configurados en el entorno.
/// </summary>
/// <remarks>
/// Esta interfaz abstrae la gestión del ciclo de vida de los servicios de trabajo en segundo plano,
/// permitiendo arrancar automáticamente los workers configurados, obtener la lista de workers
/// disponibles y controlar su ejecución individualmente mediante su nombre de servicio.
/// </remarks>
public interface IhWorkersManager
{
    /// <summary>
    /// Inicia todos los workers configurados para ejecutarse automáticamente al arranque.
    /// </summary>
    /// <remarks>
    /// Normalmente este método se invoca durante la inicialización del entorno para poner en marcha
    /// los workers marcados como de inicio automático.
    /// </remarks>
    void StartWorkers();

    /// <summary>
    /// Obtiene la colección de workers disponibles definidos en la configuración actual.
    /// </summary>
    /// <returns>
    /// Una lista con las definiciones de los workers disponibles. Si no existen definiciones,
    /// se devuelve una lista vacía.
    /// </returns>
    List<WorkerDefItem> GetAvailableWorkers();

    /// <summary>
    /// Intenta iniciar un worker concreto a partir de su nombre de servicio.
    /// </summary>
    /// <param name="serviceName">
    /// Nombre del servicio asociado al worker que se desea iniciar.
    /// </param>
    /// <returns>
    /// <see langword="true"/> si el worker se inició correctamente; en caso contrario,
    /// <see langword="false"/>.
    /// </returns>
    bool StartWorker(string serviceName);

    /// <summary>
    /// Intenta detener un worker concreto a partir de su nombre de servicio.
    /// </summary>
    /// <param name="serviceName">
    /// Nombre del servicio asociado al worker que se desea detener.
    /// </param>
    /// <returns>
    /// <see langword="true"/> si el worker se detuvo correctamente; en caso contrario,
    /// <see langword="false"/>.
    /// </returns>
    bool StopWorker(string serviceName);

    /// <summary>
    /// Indica si un worker concreto se encuentra actualmente en ejecución.
    /// </summary>
    /// <param name="serviceName">
    /// Nombre del servicio asociado al worker que se desea consultar.
    /// </param>
    /// <returns>
    /// <see langword="true"/> si el worker está en ejecución; en caso contrario,
    /// <see langword="false"/>.
    /// </returns>
    bool WorkerIsRunning(string serviceName);
}