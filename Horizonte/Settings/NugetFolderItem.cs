namespace Horizonte.Settings;

/// <summary>
/// Representa un elemento de configuración para una carpeta de paquetes NuGet dentro del sistema.
/// </summary>
/// <remarks>
/// La clase NugetFolderItem se utiliza para definir las propiedades de una carpeta donde se
/// alojan los paquetes NuGet. Esto es parte de la configuración de un módulo que maneja
/// diferentes carteras de NuGet.
/// </remarks>
public class NugetFolderItem
{
    /// La propiedad `Folder` representa la ubicación del directorio asociada con un elemento de Nuget dentro de la configuración de módulos.
    /// Esta propiedad es utilizada principalmente para especificar el directorio donde se almacenan los paquetes Nuget, y puede ser clave
    /// para la instalación y gestión de dichos paquetes en la aplicación.
    /// En el contexto de gestión de ensamblados, como se usa en `HAssemblyManager`, `Folder` es parte del proceso para determinar rutas de búsqueda
    /// y la carpeta de instalación para los paquetes Nuget. Asegura que el directorio existe y organiza las rutas de búsqueda en base a
    /// la configuración especificada en `ModulesSettings`.
    /// El uso correcto de `Folder` es importante para mantener la estructura de directorios adecuada y para que el sistema pueda acceder
    /// a los recursos Nuget necesarios sin inconvenientes.
    /// /
    public string Folder { get; set; } = string.Empty;

    /// <summary>
    /// Propiedad que representa el nombre asociado a un objeto o entidad.
    /// </summary>
    /// <remarks>
    /// El valor de esta propiedad suele utilizarse para identificar de forma única o describir al objeto o entidad en distintas operaciones o contextos.
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// La propiedad "Order" indica el orden de prioridad que se debe asignar a un elemento dentro de una colección, como puede ser una lista de servidores NuGet o carpetas NuGet.
    /// Esta propiedad es utilizada para determinar el orden en el que se procesan o se muestran los elementos.
    /// Un valor numérico más bajo generalmente indicará una mayor prioridad en el orden de manejo.
    /// /
    public int Order { get; set; } = 0;

    /// <summary>
    /// Propiedad que indica si una carpeta específica debe ser utilizada como la carpeta de instalación.
    /// </summary>
    /// <remarks>
    /// Esta propiedad determina si la carpeta asociada será utilizada para la instalación de paquetes NuGet.
    /// Si se establece en <c>true</c>, el directorio correspondiente será designado como la carpeta de
    /// instalación donde se gestionarán y almacenarán los paquetes descargados.
    /// </remarks>
    public bool InstallFolder { get; set; } = false;

    /// <summary>
    /// Indica si el estado actual es activo o no.
    /// </summary>
    /// <remarks>
    /// La propiedad se puede utilizar para verificar y establecer si un objeto o proceso está
    /// en funcionamiento o habilitado dentro del contexto de la aplicación.
    /// </remarks>
    public bool Active { get; set; } = true;
}