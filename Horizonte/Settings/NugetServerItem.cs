namespace Horizonte.Settings;

/// <summary>
/// Representa un servidor de NuGet utilizado dentro de la configuración de módulos.
/// </summary>
/// <remarks>
/// La clase NugetServerItem permite la definición de un servidor NuGet,
/// especificando detalles como la URL del servidor, el nombre, el orden y la versión del API de NuGet que utiliza.
/// </remarks>
public class NugetServerItem
{
    /// <summary>
    /// Representa la URL del servidor NuGet utilizado para obtener y gestionar paquetes.
    /// </summary>
    /// <remarks>
    /// La propiedad <c>Server</c> especifica la dirección base del servidor NuGet que
    /// se utiliza para conectarse y realizar operaciones relacionadas con los paquetes
    /// NuGet, como la descarga, la consulta de versiones disponibles y la indexación
    /// de paquetes.
    /// </remarks>
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Propiedad que representa el nombre de un objeto o entidad.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se utiliza para identificar de manera única a un objeto dentro de su contexto
    /// y puede estar sujeta a restricciones de formato o longitud dependiendo de su implementación.
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// La propiedad <c>Order</c> representa el orden en el que se procesan los elementos de tipo <c>NugetServerItem</c>
    /// y <c>NugetFolderItem</c>. Este valor entero determina la secuencia de prioridad para operaciones como la descarga
    /// de paquetes en servidores NuGet o la instalación de carpetas NuGet, donde los elementos se procesan de menor a
    /// mayor orden. Un valor por defecto de 0 suele indicarse para representar el primer lugar en el orden de ejecución.
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Propiedad que indica la versión actual de la aplicación o componente.
    /// </summary>
    /// <remarks>
    /// La versión sigue el formato común de versionado, generalmente compuesto por
    /// números mayor, menor, de revisión y compilación.
    /// </remarks>
    public int Version { get; set; } = 2;

    /// <summary>
    /// Propiedad que indica si el objeto actual está activo o no.
    /// </summary>
    /// <remarks>
    /// Un valor de <c>true</c> significa que el objeto está activo, mientras que <c>false</c> indica que está inactivo.
    /// Esta propiedad es útil para determinar el estado actual del objeto en escenarios donde se requiere saber si debe procesarse o no.
    /// </remarks>
    public bool Active { get; set; } = true;
}