namespace Horizonte.Entities;

/// <summary>
/// Resultado del proceso de resolución física o descarga de un paquete NuGet.
/// </summary>
public class PackageResolutionResult
{
    /// <summary>
    /// Indica si la resolución del paquete fue exitosa y la librería DLL está disponible.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Identificador del paquete NuGet resuelto.
    /// </summary>
    public string? PackageId { get; set; }

    /// <summary>
    /// Versión del paquete resuelto.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Ruta absoluta al archivo DLL resuelto en el sistema de archivos.
    /// </summary>
    public string? DllPath { get; set; }

    /// <summary>
    /// Mensaje descriptivo en caso de error o bloqueo.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Indica si el paquete fue bloqueado por una regla de seguridad de BlockedPackages.
    /// </summary>
    public bool Blocked { get; set; }

    /// <summary>
    /// Indica si el paquete resuelto es puramente de runtime nativo (sin DLL administrada en lib/).
    /// </summary>
    public bool IsRuntimePackage { get; set; }

    /// <summary>
    /// Indica si el paquete contiene activos nativos en /runtimes/ para el identificador de runtime (RID) actual.
    /// </summary>
    public bool HasNativeAssets { get; set; }

    /// <summary>
    /// Ruta absoluta al directorio raíz de la versión del paquete resuelto en el sistema de archivos.
    /// </summary>
    public string? PackageRootDirectory { get; set; }
}
