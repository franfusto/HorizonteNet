using Horizonte.Settings;

namespace Horizonte;

/// <summary>
/// Gestiona la configuración global de módulos, servidores NuGet, carpetas locales de paquetes, prioridades de framework y dominios en Horizonte.
/// </summary>
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


    /// <summary>
    /// Especifica si se debe utilizar el directorio de paquetes NuGet del usuario.
    /// </summary>
    public bool UseUserNugetFolder { get; set; } = false;
    
    /// <summary>
    /// Obtiene o establece la lista ordenada de frameworks .NET por prioridad de resolución (ej. "net10.0", "net9.0", "net8.0").
    /// </summary>
    public List<string> FrameworkPriorities { get; set; } = new List<string>();

    /// <summary>
    /// Obtiene o establece la lista de paquetes forzados (<see cref="ForcedPackageItem"/>) requeridos obligatoriamente por el sistema.
    /// </summary>
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