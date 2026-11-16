using Horizonte.Entities;

namespace Horizonte.Interfaces;

/// <summary>
/// Interfaz para la búsqueda, evaluación y descarga de paquetes NuGet y librerías DLL.
/// </summary>
public interface IPackageSolver
{
    /// <summary>
    /// Resuelve las dependencias y la versión óptima de un paquete NuGet o ensamblado.
    /// </summary>
    /// <param name="packageOrAssemblyName">Nombre del paquete o del ensamblado a resolver.</param>
    /// <param name="requestedVersion">Versión solicitada explícitamente, o <see langword="null"/> para resolver la más adecuada.</param>
    /// <param name="domainName">Nombre opcional del dominio en el que se ejecuta la resolución.</param>
    /// <returns>Una tarea que devuelve el resultado de la resolución de paquete (<see cref="PackageResolutionResult"/>).</returns>
    Task<PackageResolutionResult> ResolvePackageAsync(string packageOrAssemblyName, string? requestedVersion = null, string? domainName = null);

    /// <summary>
    /// Resuelve la ruta local hacia la DLL correspondiente a un paquete y versión determinados.
    /// </summary>
    /// <param name="packageName">Identificador del paquete.</param>
    /// <param name="version">Versión del paquete.</param>
    /// <returns>La ruta absoluta a la DLL o <see langword="null"/> si no se encuentra en las carpetas locales.</returns>
    string? ResolveLocalDllPath(string packageName, string version);

    /// <summary>
    /// Descarga e instala asíncronamente un paquete NuGet con la versión especificada.
    /// </summary>
    /// <param name="packageName">Identificador del paquete NuGet.</param>
    /// <param name="version">Versión del paquete.</param>
    /// <returns>Una tarea que devuelve la ruta local de instalación/extracción o <see langword="null"/> si se produce un error.</returns>
    Task<string?> DownloadAndInstallPackageAsync(string packageName, string version);
}
