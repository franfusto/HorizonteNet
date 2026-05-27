using Horizonte.Settings;

namespace Horizonte;

/// La clase ModulesSettings se utiliza para gestionar la configuración de módulos, servidores NuGet, carpetas NuGet, prioridades de frameworks y dominios.
/// En el constructor de la clase, se inicializa una lista de configuraciones predeterminadas, incluida la definición de un módulo activo, la especificación de un servidor NuGet, y una lista de prioridades de framework.
/// Proporciona varias colecciones para mantener configuraciones específicas:
/// - List: Almacena configuraciones individuales de módulos.
/// - NugetServers: Contiene configuraciones de servidores NuGet.
/// - NugetFolders: Lista las carpetas de instalación de NuGet.
/// - FrameworkPriorities: Define una lista de prioridades para diferentes versiones de frameworks.
/// - ForcedPackages: Mantiene una lista de paquetes que están forzados dentro del entorno.
/// - Domains: Lista de dominios utilizados en la aplicación.
/// /
public class ModulesSettings
{

    /// <summary>
    /// Propiedad que representa una lista de objetos de tipo <c>ModulesSettingsItem</c>.
    /// Esta lista se utiliza para almacenar configuraciones de módulos relacionadas
    /// con la aplicación. Los elementos de esta lista determinan las características
    /// y el comportamiento de los módulos, como su activación, nombre y versión.
    /// </summary>
    public List<ModulesSettingsItem> List { get; set; } = [];

    /// <summary>
    /// Propiedad que representa una lista de servidores NuGet configurados.
    /// Cada elemento de la lista es una instancia de <see cref="NugetServerItem"/>,
    /// que contiene la información necesaria para interactuar con un servidor de paquetes NuGet específico,
    /// como la URL del servidor, el nombre, la versión del API soportada,
    /// el orden de preferencia en que se debe acceder a los servidores,
    /// y un indicador de si el servidor está activo.
    /// Esta configuración permite gestionar y priorizar múltiples fuentes de paquetes NuGet
    /// en la aplicación al resolver dependencias de paquetes.
    /// </summary>
    public List<NugetServerItem> NugetServers { get; set; } = new List<NugetServerItem>();

    /// <summary>
    /// Propiedad que representa una lista de directorios de paquetes NuGet configurados.
    /// </summary>
    /// <remarks>
    /// Cada elemento de la lista es una instancia de <c>NugetFolderItem</c>, que define las propiedades
    /// específicas de un directorio de paquetes, como el nombre de la carpeta, el nombre identificador,
    /// el orden dentro de la lista y si se debe instalar.
    /// </remarks>
    public List<NugetFolderItem> NugetFolders { get; set; } = new List<NugetFolderItem>();

    /// La propiedad `FrameworkPriorities` es una lista de cadenas que especifica las prioridades de los frameworks .NET
    /// que se utilizarán en el contexto de configuración de módulos. Esta propiedad determina el orden de preferencia
    /// para seleccionar el framework adecuado durante la gestión de módulos y puede incluir versiones específicas de .NET,
    /// como "net10.0", "net9.0", "net8.0", entre otras. Facilita la identificación de qué versiones de framework deben recibir
    /// prioridad al implementar o ejecutar aplicaciones y bibliotecas dentro del sistema.
    /// /
    public List<string> FrameworkPriorities { get; set; } = new List<string>();

    /// La propiedad ForcedPackages es una lista que contiene elementos de tipo ForcedPackageItem.
    /// Esta lista representa un conjunto de paquetes que se deben incluir de manera obligatoria en el contexto del sistema de ensamblado del proyecto.
    /// Cada entrada en esta lista incluye detalles específicos sobre el paquete, como su identificador y versión.
    /// Uso típico de ForcedPackages incluye la gestión de dependencias que no deben ser omitidas durante el cargado de dominios o módulos.
    /// Los paquetes forzados garantizan que ciertas bibliotecas necesarias estén presentes independientemente de otras configuraciones del entorno.
    /// /
    public List<ForcedPackageItem> ForcedPackages { get; set; } = new List<ForcedPackageItem>();


    /// <summary>
    /// Propiedad que representa un diccionario para mapear nombres de ensamblados a sus paquetes correspondientes.
    /// Este diccionario se utiliza para asociar ensamblados con los paquetes NuGet que los proveen, facilitando la gestión
    /// de dependencias dentro de la aplicación. Las claves del diccionario son los nombres de los ensamblados y los valores
    /// son los identificadores de los paquetes. La comparación de cadenas para las claves se realiza sin distinguir entre mayúsculas y minúsculas.
    /// </summary>
    public Dictionary<string, string> AssemblyToPackageMap { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Representa una lista de dominios asociados a la configuración de módulos dentro de la aplicación.
    /// </summary>
    /// <remarks>
    /// Los dominios pueden ser utilizados para categorizar o segmentar diferentes espacios de trabajo
    /// o entornos dentro de la aplicación. Inicialmente, la lista contiene un dominio por defecto y
    /// puede ser expandida según las necesidades del proyecto.
    /// </remarks>
    public List<string> Domains { get; set; } = new List<string>();
}