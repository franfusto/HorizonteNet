namespace Horizonte.Settings;

/// <summary>
/// Representa un ítem de paquete que se obliga a incluir en un pedido o proceso logístico.
/// </summary>
/// <remarks>
/// La clase ForcedPackageItem se utiliza en situaciones donde ciertos artículos deben ser incluidos por defecto
/// en un paquete o envío, independientemente de las preferencias del usuario o del contexto.
/// </remarks>
public class ForcedPackageItem
{
    /// La propiedad `Active` determina si un paquete forzado está activo o inactivo.
    /// Cuando está configurada como `true`, el paquete será procesado y cargado por el sistema.
    /// Si está configurada como `false`, el paquete será ignorado y no se llevará a cabo ninguna acción sobre él.
    /// Esta propiedad es útil para controlar dinámicamente qué paquetes deben estar activos, permitiendo
    /// a los administradores del sistema gestionar los paquetes que se requieren en diferentes contextos o entorno
    /// sin necesidad de eliminar su configuración del sistema.
    /// /
    public bool Active { get; set; } = true;

    /// <summary>
    /// Propiedad que representa el identificador único de un paquete forzado.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se utiliza para gestionar paquetes en el sistema, permitiendo
    /// identificar específicamente qué paquete debe ser cargado o manipulado.
    /// Se espera que cada paquete tenga un identificador único que distinga
    /// un paquete de otro dentro de las aplicaciones que utilizan esta funcionalidad.
    /// </remarks>
    public string PackageId { get; set; } = string.Empty;

    /// Propiedad `Version` en la clase `ForcedPackageItem`, utilizada para definir la versión específica de un paquete forzado que se desea cargar o gestionar dentro del sistema.
    /// Es una cadena de texto que contiene el número de versión del paquete, y se utiliza fundamentalmente para identificar de manera precisa qué versión del paquete debe manejarse en las operaciones de carga.
    /// Esta propiedad es particularmente útil cuando se implementan métodos que requieren la carga de paquetes con versiones específicas, como en la clase `HAssemblyManager`, permitiendo el manejo correcto de diferentes versiones de paquetes.
    /// /
    public string Version { get; set; } = string.Empty;

    /// Representa el marco de trabajo (framework) específico que debe utilizarse para resolver y cargar un paquete forzado.
    /// Esta propiedad permite determinar el entorno de ejecución o conjunto de bibliotecas de clases bajo el cual el paquete está diseñado para funcionar. Algunos ejemplos de frameworks pueden incluir .NET Framework, .NET Core, y otros.
    /// Es fundamental que esta propiedad se configure apropiadamente para garantizar que el paquete se cargue y ejecute en un entorno compatible, evitando así posibles conflictos o errores de incompatibilidad.
    /// /
    public string Framework { get; set; } = string.Empty;

    /// /
    public string Domain { get; set; } = "Default";
}