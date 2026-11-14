using Horizonte.Entities;

namespace Horizonte.Interfaces;

/// <summary>
/// Interfaz para la búsqueda, evaluación y descarga de paquetes NuGet y librerías DLL.
/// </summary>
public interface IPackageSolver
{
    Task<PackageResolutionResult> ResolvePackageAsync(string packageOrAssemblyName, string? requestedVersion = null, string? domainName = null);
    string? ResolveLocalDllPath(string packageName, string version);
    Task<string?> DownloadAndInstallPackageAsync(string packageName, string version);
}
