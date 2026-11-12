namespace Horizonte.Settings;


/// <summary>
/// Representa la configuración individual de un módulo dentro del sistema Horizonte.
/// </summary>
/// <remarks>
/// Define propiedades como el estado activo, el nombre del módulo, la versión, la ruta personalizada,
/// la carga de DLLs adicionales y el dominio de aislamiento al que pertenece.
/// </remarks>
public class ModulesSettingsItem
{
    /// <summary>
    /// Propiedad que indica si un módulo o servidor NuGet está activo.
    /// </summary>
    /// <remarks>
    /// Esta propiedad es de tipo booleano y se utiliza para determinar si un elemento debe estar habilitado o funcionando dentro del contexto de configuración de módulos o servidores NuGet.
    /// En el contexto de configuración de módulos, si esta propiedad se establece en <c>true</c>, el módulo correspondiente estará activo y disponible para su carga.
    /// En el contexto de servidores NuGet, si esta propiedad se establece en <c>true</c>, el servidor estará activo y podrá ser utilizado para la gestión de paquetes.
    /// </remarks>
    public bool Active { get; set; }

    /// <summary>
    /// Obtiene o establece el nombre identificador del módulo (usualmente coincidente con el nombre de su paquete o ensamblado).
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la versión semántica requerida del módulo (por ejemplo "1.0.0").
    /// </summary>
    public string ModuleVersion { get; set; }= string.Empty;

    /// <summary>
    /// Propiedad que especifica la ruta de acceso a un archivo o directorio en el sistema de archivos.
    /// </summary>
    /// <remarks>
    /// Representa una cadena que contiene el camino absoluto o relativo al recurso en cuestión,
    /// permitiendo realizar operaciones como lectura, escritura o verificación de existencia.
    /// </remarks>
    public string? Path { get; set; }

    /// <summary>
    /// Propiedad que indica si se deben cargar ensamblados adicionales para un módulo específico.
    /// Cuando se establece en <c>true</c>, y la ruta del módulo no es <c>null</c>, permite la carga de ensamblados adicionales desde el directorio especificado.
    /// </summary>
    public bool? LoadAdditionalDlls { get; set; } = false;

    /// <summary>
    /// Propiedad que detalla el dominio para una aplicación o configuración específica.
    /// </summary>
    /// <remarks>
    /// Esta propiedad es utilizada para identificar de manera única el ámbito o área de operación dentro de un sistema,
    /// lo que facilita la segmentación y organización lógica de procesos o datos.
    /// </remarks>
    public string? Domain { get; set; } = "Default";
}