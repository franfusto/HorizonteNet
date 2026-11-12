using Microsoft.Extensions.Hosting;

namespace Horizonte.Entities;

/// <summary>
/// Representa el estado y control del ciclo de vida de un servicio en segundo plano (<see cref="BackgroundService"/>) en Horizonte.
/// </summary>
/// <remarks>
/// Almacena la instancia del servicio, el tipo de trabajador, el dominio al que pertenece,
/// los tokens de cancelación para ejecución y detención, y los flags de estado y reinicio.
/// </remarks>
public sealed class BackgroundServiceState
{
    /// <summary>
    /// Obtiene el nombre calificado del tipo de trabajador asociado al servicio.
    /// </summary>
    public string WorkerType { get; init; } = string.Empty;

    /// <summary>
    /// Representa el nombre del dominio al que pertenece el servicio en segundo plano.
    /// </summary>
    /// <remarks>
    /// Este valor se utiliza para identificar y gestionar servicios en segundo plano específicos
    /// de un dominio. Es crucial para las operaciones de carga y descarga de servicios.
    /// El nombre del dominio puede ser comparado con valores predefinidos, como <c>Const.DefaultDomainName</c>,
    /// para determinar comportamientos especiales como reinicios al cargar dominios.
    /// </remarks>
    public string DomainName { get; set; } = string.Empty;

    /// <summary>
    /// La propiedad <c>Instance</c> es una instancia de la clase <c>BackgroundService</c> que representa el servicio en segundo plano asociado al estado actual.
    /// Esta propiedad permite iniciar o detener el servicio, así como gestionar su ciclo de vida en el contexto de un dominio específico.
    /// Puede ser establecida o consultada para manipular el estado del servicio conforme a las operaciones definidas en la aplicación.
    /// </summary>
    public BackgroundService? Instance { get; set; }

    /// <summary>
    /// Obtiene o establece el <see cref="CancellationTokenSource"/> utilizado para gestionar la señal de cancelación de la ejecución activa del servicio.
    /// </summary>
    public CancellationTokenSource? RunCancellationTokenSource { get; set; }

    /// <summary>
    /// Obtiene o establece un <see cref="CancellationTokenSource"/> utilizado para gestionar la cancelación del servicio en segundo plano durante su proceso de detención.
    /// Específicamente, esta propiedad es utilizada para controlar el tiempo de espera y garantizar que un servicio en segundo plano se detenga correctamente cuando se solicita.
    /// </summary>
    public CancellationTokenSource? StopCancellationTokenSource { get; set; }

    /// <summary>
    /// Obtiene o establece un valor que indica si el servicio en segundo plano se está ejecutando actualmente.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Obtiene o establece un valor que indica si el servicio en segundo plano debe reiniciarse automáticamente al recargar el dominio al que pertenece.
    /// </summary>
    public bool RestartOnDomainLoad { get; set; }
}