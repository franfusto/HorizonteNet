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
    /// <summary>
    /// Obtiene o establece la ruta del directorio local donde se almacenan o buscan paquetes NuGet.
    /// </summary>
    public string Folder { get; set; } = string.Empty;

    /// <summary>
    /// Propiedad que representa el nombre asociado a un objeto o entidad.
    /// </summary>
    /// <remarks>
    /// El valor de esta propiedad suele utilizarse para identificar de forma única o describir al objeto o entidad en distintas operaciones o contextos.
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el orden de prioridad asignado a este directorio NuGet al resolver dependencias.
    /// </summary>
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