using Microsoft.Extensions.Hosting;

namespace Horizonte.Entities;

/// `BackgroundServiceState` es una clase que representa el estado de un servicio de fondo en ejecución dentro de un entorno de aplicación.
/// Esta clase es sellada, lo que significa que no puede ser heredada.
/// La clase contiene propiedades para manejar y rastrear varios aspectos de un servicio de fondo, tales como:
/// - El tipo de trabajador (`WorkerType`) para identificar el tipo de servicio que se está ejecutando.
/// - El nombre del dominio (`DomainName`) al que pertenece el servicio de fondo.
/// - Una instancia del servicio de fondo (`Instance`), que es una instancia de `BackgroundService`.
/// - Un `CancellationTokenSource` para controlar la cancelación de la ejecución del servicio (`RunCancellationTokenSource`).
/// - Otro `CancellationTokenSource` para gestionar la cancelación al intentar detener el servicio (`StopCancellationTokenSource`).
/// - Un indicador booleano (`IsRunning`) que muestra el estado actual de ejecución del servicio.
/// - Un indicador booleano (`RestartOnDomainLoad`) que determina si el servicio debe reiniciarse automáticamente cuando se carga un dominio.
/// Esta clase es utilizada por la clase `HAssemblyManager` para gestionar servicios de fondo, asegurando que los servicios se inicien, detengan y gestionen adecuadamente en función de los dominios de aplicación y otras condiciones definidas.
/// /
public sealed class BackgroundServiceState
{
    /// `WorkerType` representa el tipo o categoría de un servicio en segundo plano dentro del sistema.
    /// Esta propiedad es de solo lectura y se inicializa en el momento de la creación de un objeto de `BackgroundServiceState`.
    /// Se utiliza para identificar de manera única el tipo de servicio que está siendo gestionado, permitiendo operaciones como
    /// iniciar o detener servicios según su tipo específico.
    /// /
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

    /// `RunCancellationTokenSource` es una propiedad que encapsula un `CancellationTokenSource` utilizado para gestionar la señal de cancelación de un servicio en ejecución dentro de un dominio particular.
    /// Esta propiedad permite cancelar de manera explícita la operación activa de un servicio, garantizando que este pueda ser detenido cuando sea necesario.
    /// Al configurar esta propiedad, se puede establecer un nuevo origen de token de cancelación, el cual puede ser utilizado para controlar la ejecución del servicio.
    /// Además, es fundamental disponer de los recursos asociados a este origen una vez completada la operación para evitar fugas de recursos.
    public CancellationTokenSource? RunCancellationTokenSource { get; set; }

    /// <summary>
    /// Obtiene o establece un <see cref="CancellationTokenSource"/> utilizado para gestionar la cancelación del servicio en segundo plano durante su proceso de detención.
    /// Específicamente, esta propiedad es utilizada para controlar el tiempo de espera y garantizar que un servicio en segundo plano se detenga correctamente cuando se solicita.
    /// </summary>
    public CancellationTokenSource? StopCancellationTokenSource { get; set; }

    /// Indica si el servicio en segundo plano se está ejecutando actualmente.
    /// Esta propiedad es de tipo booleano, y su valor es `true` cuando el servicio
    /// está activo y funcionando, y `false` cuando el servicio está detenido o no se está ejecutando.
    /// Es útil para verificar el estado actual de un servicio en segundo plano dentro del sistema antes de
    /// realizar ciertas operaciones, como intentar detenerlo o reiniciarlo.
    /// Cambios en esta propiedad reflejan el inicio o la terminación de la ejecución del servicio correspondiente.
    /// /
    public bool IsRunning { get; set; }

    /// Propiedad que indica si el servicio en segundo plano debe reiniciarse automáticamente
    /// al cargar el dominio al que pertenece.
    /// Esta propiedad es utilizada dentro del proceso de gestión de servicios en segundo plano
    /// para determinar si un servicio específico debe ser reiniciado cuando el dominio se carga o
    /// se reinicia.
    /// Un valor true indica que el servicio se reiniciará automáticamente al cargar el dominio,
    /// mientras que un valor false indica lo contrario.
    /// /
    public bool RestartOnDomainLoad { get; set; }
}