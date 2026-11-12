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
    /// <summary>
    /// Obtiene o establece la representación en texto de la versión del paquete NuGet.
    /// </summary>
    public string VersionString { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el objeto <see cref="System.Version"/> correspondiente a la versión del paquete.
    /// </summary>
    public Version Version { get; set; } = new Version();

    /// <summary>
    /// Obtiene o establece el identificador único del paquete NuGet (ej. "Newtonsoft.Json").
    /// </summary>
    public string PackageId { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el marco de trabajo de destino (ej. "net10.0") para el que está compilado el paquete.
    /// </summary>
    public string Framework { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la ruta completa al archivo DLL del paquete NuGet en el sistema de archivos.
    /// </summary>
    public string DllPath { get; set; } = string.Empty;
}

