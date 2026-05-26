namespace Horizonte.Settings;


/// La clase ModulesSettingsItem representa la configuración de un módulo dentro de un sistema.
/// Esta clase se utiliza para definir propiedades específicas de cada módulo, tales como su estado activo, nombre, versión, y demás configuraciones adicionales que puedan ser necesarias.
/// Propiedades:
/// - Active: Indica si el módulo está activo o no.
/// - ModuleName: Especifica el nombre del módulo.
/// - ModuleVersion: Define la versión actual del módulo.
/// - Path: Puede contener la ruta en la que se encuentra el módulo, si es aplicable.
/// - LoadAdditionalDlls: Indica si se deben cargar DLLs adicionales para el módulo.
/// - Domain: Permite especificar el dominio al que pertenece el módulo.
/// Esta clase es esencial para manejar dinámicamente la configuración de módulos en aplicaciones que requieren modularidad y extensibilidad.
/// /
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

    /// Representa el nombre de un módulo en el sistema de configuraciones de módulos.
    /// Esta propiedad es utilizada para identificar de manera única a un módulo dentro de la aplicación.
    /// Es fundamental para la carga de módulos, ya que se utiliza en múltiples procesos, como el registro
    /// de directorios de módulos y la resolución de ensamblados. La correcta asignación de este nombre
    /// asegura la correcta operación del sistema de gestión de módulos.
    /// /
    public string ModuleName { get; set; } = string.Empty;

    /// Propiedad que representa la versión del módulo en el sistema.
    /// Esta propiedad es utilizada para especificar la versión actual de un módulo dentro del sistema.
    /// Es un valor clave para asegurar la correcta gestión y carga de los distintos módulos disponibles,
    /// permitiendo mantener la integridad y compatibilidad entre ellos. Su valor típicamente sigue el
    /// formato de versión semántica (por ejemplo, "1.0.0").
    /// /
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