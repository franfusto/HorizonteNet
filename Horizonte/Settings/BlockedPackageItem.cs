namespace Horizonte.Settings;

/// <summary>
/// Define la política de resolución de paquetes NuGet cuando se detecta una versión bloqueada por seguridad.
/// </summary>
public enum BlockedPackagesPolicy
{
    /// <summary>
    /// Devuelve fallo de resolución (<c>Success = false</c>) y registra una entrada negativa en la caché.
    /// </summary>
    Null,

    /// <summary>
    /// Lanza una excepción de operación inválida (<see cref="System.InvalidOperationException"/>) impidiendo la carga.
    /// </summary>
    Error,

    /// <summary>
    /// Intenta resolver la menor versión superior disponible que no se encuentre bloqueada.
    /// </summary>
    Mayor
}

/// <summary>
/// Representa una regla o elemento de bloqueo para un paquete NuGet y versión específicos.
/// </summary>
public class BlockedPackageItem
{
    /// <summary>
    /// Obtiene o establece si la regla de bloqueo se encuentra activa.
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>
    /// Obtiene o establece el identificador del paquete NuGet bloqueado.
    /// </summary>
    public string PackageId { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la versión o patrón de versión bloqueada.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el motivo o justificación de seguridad del bloqueo.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
