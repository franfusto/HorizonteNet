using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte;

/// <summary>
/// Representa un trabajador de servicio en segundo plano que realiza tareas específicas durante sus fases de inicio y detención.
/// </summary>
/// <remarks>
/// La clase StartEndWorker está diseñada para manejar y gestionar operaciones que deben ejecutarse
/// al comienzo y al final de su ciclo de vida. Extiende la clase BackgroundService para una ejecución
/// asincrónica e implementa la interfaz IHservice para funcionalidades adicionales y garantizar el cumplimiento del contrato.
/// StartEndWorker está equipada con inyección de dependencias para acceder a los servicios y configuraciones necesarios.
/// </remarks>
public class StartEndWorker : BackgroundService
{
    /// <summary>
    /// Indica si el servicio se encuentra actualmente en ejecución.
    /// </summary>
    /// <remarks>
    /// Esta propiedad refleja el estado operativo del servicio, devolviendo true si el servicio está activo y realizando sus tareas, o false si el servicio está detenido o inactivo.
    /// </remarks>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Obtiene o establece un valor que indica si el servicio debe iniciarse automáticamente
    /// cuando el host comienza su ejecución. Si se establece en true, el servicio se iniciará
    /// automáticamente; de lo contrario, no se iniciará hasta que sea activado explícitamente.
    /// </summary>
    public bool RunOnStart { get; set; }

    /// <summary>
    /// Representa el nombre del servicio. Esta propiedad se utiliza para identificar
    /// la instancia específica del servicio y puede ser personalizada según los
    /// requisitos del servicio o los detalles de su implementación.
    /// </summary>
    public string ServiceName { get; set; }



    /// <summary>
    /// Representa la instancia del registrador utilizada por el StartEndWorker para registrar información, advertencias, errores
    /// y otros detalles en tiempo de ejecución durante la ejecución de tareas en segundo plano.
    /// </summary>
    /// <remarks>
    /// El registrador es una instancia de <see cref="ILogger"/> y se inicializa específicamente para la
    /// clase StartEndWorker. Se completa a través del contenedor de inyección de dependencias durante
    /// la fase de inicio del servicio en <see cref="StartEndWorker.StartAsync(System.Threading.CancellationToken)"/>.
    /// </remarks>
    private ILogger? _log;

    /// <summary>
    /// Instancia de la interfaz IHGesCom utilizada para gestionar y ejecutar comandos dentro de la aplicación.
    /// </summary>
    /// <remarks>
    /// Esta variable se inyecta a través de la inyección de dependencias y proporciona funcionalidades
    /// como la inicialización de módulos, la obtención de listas de comandos y la ejecución de comandos específicos.
    /// Desempeña un papel fundamental en la ejecución de los comandos de inicio y fin dentro de la clase StartEndWorker.
    /// </remarks>
    private IHGesCom? _gesCom;

    /// <summary>
    /// Un miembro privado que representa los datos contextuales u operaciones necesarias para el servicio.
    /// </summary>
    /// <remarks>
    /// Esta variable es una instancia de la interfaz <see cref="IHContext"/>, utilizada dentro del servicio
    /// para acceder o gestionar datos relacionados con el contexto. Facilita la recuperación, actualización
    /// y procesamiento de información contextual requerida para el funcionamiento del servicio. Su valor
    /// generalmente se resuelve a través de la inyección de dependencias durante la inicialización del servicio.
    /// </remarks>
    private IHContext? _context;

    /// <summary>
    /// Representa las configuraciones de los comandos de inicio y fin
    /// ejecutados durante el ciclo de vida del servicio <see cref="StartEndWorker"/>.
    /// Esto incluye la lista de comandos de inicio y fin a ejecutar
    /// y su estado de activación.
    /// </summary>
    private SeCommandsSettings _seCommandsSettings = new ();


    public StartEndWorker(ILogger<FakeWorker> logger,IHContext context,IHGesCom gesCom, string serviceName, bool runOnStart)
    {
        _log = logger;
        _context = context;
        _seCommandsSettings = _context?.Get<SeCommandsSettings>() ?? new SeCommandsSettings();
        _gesCom = gesCom;
        ServiceName = serviceName;
        RunOnStart = runOnStart;
        ServiceName = serviceName;
    }

    /// <summary>
    /// Realiza la lógica principal del servicio StartEndWorker.
    /// </summary>
    /// <param name="stoppingToken">Un CancellationToken que indica cuando la operación debe ser cancelada.</param>
    /// <remarks>
    /// Este método contiene la funcionalidad principal que se ejecuta mientras el servicio está en funcionamiento.
    /// Inicializa los comandos de inicio necesarios y registra información relevante relacionada con el ciclo de vida del servicio.
    /// El trabajo realizado en este método puede ser ampliado o personalizado según lo requiera la lógica de la aplicación.
    /// </remarks>
    private void DoWork(CancellationToken stoppingToken)
    {
        _log?.LogInformation("Iniciando tart/End Worker");
        StartCommands();
    }

    /// <summary>
    /// Ejecuta la lógica principal de procesamiento en segundo plano del StartEndWorker.
    /// </summary>
    /// <param name="stoppingToken">Un CancellationToken que se activa cuando el host se está cerrando, indicando al servicio que detenga el procesamiento.</param>
    /// <returns>Una tarea que representa la operación en segundo plano que se está ejecutando.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log?.LogInformation("Ejecutando Start/End Worker");
        IsRunning = true;
        return Task.Run(() => DoWork(stoppingToken));
    }

    /// <summary>
    /// Inicia el servicio en segundo plano e inicializa las dependencias y configuraciones requeridas.
    /// </summary>
    /// <param name="cancellationToken">
    /// Un token que permite abortar la operación asincrónica de inicio.
    /// </param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica de inicio del servicio.
    /// </returns>
    public override Task StartAsync(CancellationToken cancellationToken)
    {

        IsRunning = true;
        return base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Detiene el servicio en segundo plano de forma asincrónica, realizando tareas de limpieza y finalizando cualquier lógica al final de su ciclo de vida.
    /// </summary>
    /// <param name="cancellationToken">Un token de cancelación para señalar la solicitud de detener el servicio.</param>
    /// <returns>Una tarea que se completa cuando el servicio se ha detenido y se han finalizado las operaciones de limpieza.</returns>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log?.LogInformation("Finalizando Start/End Worker");
        IsRunning = false;
        EndCommands();
        return base.StopAsync(cancellationToken);
    }


    /// <summary>
    /// Ejecuta una colección de comandos configurados durante la fase de inicio del servicio.
    /// </summary>
    /// <remarks>
    /// Este método recorre una lista de comandos definida en la clase SeCommandsSettings y los ejecuta
    /// si están marcados como activos. Utiliza la interfaz IHGesCom para la ejecución de comandos. Generalmente, 
    /// estos comandos se utilizan para inicializar o configurar procesos necesarios para el servicio al iniciar.
    /// </remarks>
    private void StartCommands()
    {
        foreach (var item in _seCommandsSettings.StartCommands.Where(item => item.Active))
        {
            _gesCom?.RunCommand(item.CommandName);
        }
    }

    /// <summary>
    /// Ejecuta una colección predefinida de comandos durante la secuencia de apagado del servicio.
    /// </summary>
    /// <remarks>
    /// El método EndCommands recorre la lista de comandos de salida especificados en la propiedad EndCommands
    /// del objeto SeCommandsSettings. Solo se ejecutan los comandos marcados como activos.
    /// La ejecución de los comandos se delega a la interfaz IHGesCom utilizando su método RunCommand.
    /// Este método garantiza que no se ejecuten comandos si el servicio no se encuentra en ejecución.
    /// </remarks>
    private void EndCommands()
    {
        if(!IsRunning) return;
        foreach (var item in _seCommandsSettings.EndCommands.Where(item => item.Active))
        {
            _gesCom?.RunCommand(item.CommandName);
        }
    }


}