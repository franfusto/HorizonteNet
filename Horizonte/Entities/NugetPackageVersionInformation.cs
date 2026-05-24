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