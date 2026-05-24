namespace Horizonte;

/// <summary>
/// La clase NugetPackageVersionInformation representa la información de versión de un paquete NuGet.
/// </summary>
/// <remarks>
/// Esta clase proporciona detalles sobre la versión específica de un paquete NuGet, incluyendo su identificador de paquete,
/// el framework de destino para el cual está diseñado y la ruta del archivo DLL asociado.
/// </remarks>
public class NugetPackageVersionInformation()
{
    /// La propiedad `VersionString` representa la cadena de texto de la versión de un paquete NuGet en la clase `NugetPackageVersionInformation`.
    /// Se utiliza para almacenar la información de la versión en formato de cadena, tal como se encuentra en la estructura del directorio del paquete.
    /// Esta propiedad es útil para mantener la representación exacta de la versión del paquete que puede incluir etiquetas de pre-lanzamiento o metadatos no disponibles en el objeto `Version`.
    public string VersionString { get; set; } = string.Empty;
    /// Representa la versión de un paquete NuGet.
    /// La propiedad `Version` se utiliza para definir la versión asociada a un paquete NuGet específico que se describe en la clase `NugetPackageVersionInformation`.
    /// Esta versión juega un papel crucial en la identificación y gestión de las dependencias de software, asegurando que las aplicaciones utilicen las versiones correctas de las bibliotecas.
    /// /
    public Version Version { get; set; } = new Version();

    /// La propiedad PackageId representa el identificador único de un paquete NuGet.
    /// Es utilizada para diferenciar este paquete de otros dentro del sistema,
    /// y normalmente corresponde al nombre del paquete en el servidor de NuGet.
    /// Tipo de dato: cadena de texto.
    /// /
    public string PackageId { get; set; } =string.Empty;

    /// Obtiene o establece el marco de trabajo (framework) que se asocia con el paquete NuGet.
    /// Este valor representa la configuración objetivo para la cual el paquete NuGet está diseñado.
    /// /
    public string Framework { get; set; } = string.Empty;

    /// DllPath es una propiedad de tipo cadena que representa la ruta de acceso del archivo DLL relacionado con la información de la versión del paquete NuGet.
    /// Esta propiedad permite establecer y obtener la ubicación específica del archivo DLL en el sistema de archivos,
    /// lo que es crucial para operaciones que requieran cargar o gestionar dicho archivo.
    /// /
    public string DllPath { get; set; }= string.Empty;
}

