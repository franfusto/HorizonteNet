using System.Reflection;
using System.Runtime.Loader;
using Horizonte.Helpers;

namespace Horizonte.Interfaces;

/// <summary>
/// Interfaz para el subsistema de resolución de dependencias, descarga de paquetes NuGet, caché y activos web.
/// </summary>
public interface IHAssemblySolver
{
    /// <summary>
    /// Obtiene el registro de archivos estáticos y activos web extraídos de los paquetes y módulos.
    /// </summary>
    StaticFileRegistry StaticFileRegistry { get; }

    /// <summary>
    /// Obtiene la instancia de la caché unificada de ensamblados.
    /// </summary>
    IHAssemblyCache AssemblyCache { get; }

    /// <summary>
    /// Intenta resolver un ensamblado solicitado dentro de un contexto de carga específico (<see cref="AssemblyLoadContext"/>).
    /// </summary>
    /// <param name="context">Contexto de carga de ensamblados.</param>
    /// <param name="assemblyName">Nombre del ensamblado a resolver.</param>
    /// <returns>El ensamblado resuelto o <see langword="null"/> si no se pudo resolver.</returns>
    Assembly? ResolveAssemblyFromALC(AssemblyLoadContext context, AssemblyName assemblyName);

    /// <summary>
    /// Resuelve un ensamblado desde paquetes NuGet en respuesta al evento de resolución del dominio de aplicación.
    /// </summary>
    /// <param name="sender">Origen del evento de resolución.</param>
    /// <param name="args">Argumentos del evento con la información del ensamblado solicitado.</param>
    /// <returns>El ensamblado resuelto o <see langword="null"/> si no se pudo encontrar.</returns>
    Assembly? ResolveAssemblyFromNuGetPackages(object? sender, ResolveEventArgs args);

    /// <summary>
    /// Resuelve la ruta local al archivo DLL de un paquete NuGet y versión determinados utilizando el dominio por defecto.
    /// </summary>
    /// <param name="packageName">Identificador del paquete NuGet.</param>
    /// <param name="version">Versión del paquete.</param>
    /// <returns>La ruta absoluta a la DLL o <see langword="null"/> si no se encuentra.</returns>
    string? ResolveAssemblyDllPath(string packageName, string version);

    /// <summary>
    /// Resuelve la ruta local al archivo DLL de un paquete NuGet y versión determinados especificando un Target Framework prioritario.
    /// </summary>
    /// <param name="packageName">Identificador del paquete NuGet.</param>
    /// <param name="version">Versión del paquete.</param>
    /// <param name="targetFramework">Target Framework Moniker (TFM) prioritario opcional.</param>
    /// <returns>La ruta absoluta a la DLL o <see langword="null"/> si no se encuentra.</returns>
    string? ResolveAssemblyDllPath(string packageName, string version, string? targetFramework);

    /// <summary>
    /// Resuelve la ruta local al archivo DLL de un paquete NuGet y versión determinados especificando el Target Framework y el dominio que solicita la resolución.
    /// </summary>
    /// <param name="packageName">Identificador del paquete NuGet.</param>
    /// <param name="version">Versión del paquete.</param>
    /// <param name="targetFramework">Target Framework Moniker (TFM) prioritario opcional.</param>
    /// <param name="domain">Nombre del dominio que solicita la resolución.</param>
    /// <returns>La ruta absoluta a la DLL o <see langword="null"/> si no se encuentra.</returns>
    string? ResolveAssemblyDllPath(string packageName, string version, string? targetFramework, string domain);

    /// <summary>
    /// Descarga e instala asíncronamente un paquete NuGet con la versión solicitada.
    /// </summary>
    /// <param name="packageName">Identificador del paquete NuGet a descargar.</param>
    /// <param name="version">Versión del paquete.</param>
    /// <param name="targetFramework">Target Framework Moniker (TFM) prioritario opcional.</param>
    /// <returns>Una tarea que devuelve la ruta base de extracción del paquete o <see langword="null"/> si falla la descarga.</returns>
    Task<string?> DownloadNuGetPackageAsync(string packageName, string version, string? targetFramework = null);

    /// <summary>
    /// Obtiene el nombre del paquete NuGet correspondiente a partir del nombre de un ensamblado.
    /// </summary>
    /// <param name="assemblyName">Nombre del ensamblado.</param>
    /// <returns>El nombre del paquete asociado.</returns>
    string GetPackageName(string assemblyName);

    /// <summary>
    /// Registra manualmente o actualiza el mapeo entre el nombre de un ensamblado y su identificador de paquete NuGet.
    /// </summary>
    /// <param name="assemblyName">Nombre del ensamblado.</param>
    /// <param name="packageName">Identificador del paquete NuGet asociado.</param>
    void RegisterAssemblyMapping(string assemblyName, string packageName);

    /// <summary>
    /// Escanea un ensamblado en busca de definiciones con el atributo <see cref="AssemblyMappingDefinition"/> y registra los mapeos descubiertos.
    /// </summary>
    /// <param name="assembly">Ensamblado a inspeccionar.</param>
    void ScanAssemblyForMappings(Assembly assembly);

    /// <summary>
    /// Almacena un ensamblado en la caché en memoria bajo una clave determinada.
    /// </summary>
    /// <param name="key">Clave identificadora en la caché.</param>
    /// <param name="assemblyBytes">Bytes del ensamblado binario.</param>
    void CacheAssembly(string key, byte[] assemblyBytes);

    /// <summary>
    /// Recupera los bytes de un ensamblado almacenado en la caché a partir de su clave.
    /// </summary>
    /// <param name="key">Clave identificadora en la caché.</param>
    /// <returns>Arreglo de bytes del ensamblado o <see langword="null"/> si no existe en caché.</returns>
    byte[]? GetCachedAssembly(string key);

    /// <summary>
    /// Comprueba si existe un ensamblado almacenado en caché bajo la clave dada.
    /// </summary>
    /// <param name="key">Clave identificadora en la caché.</param>
    /// <returns><see langword="true"/> si el ensamblado está en caché; de lo contrario, <see langword="false"/>.</returns>
    bool IsAssemblyCached(string key);

    /// <summary>
    /// Resuelve y carga una biblioteca de código nativo / no administrado solicitada dentro de un contexto ALC.
    /// </summary>
    /// <param name="context">Contexto de carga de ensamblados que originó la solicitud.</param>
    /// <param name="unmanagedDllName">Nombre o ruta de la biblioteca no administrada solicitada.</param>
    /// <returns>Puntero al manejador de la biblioteca nativa cargada o <see cref="IntPtr.Zero"/> si no pudo resolverse.</returns>
    IntPtr ResolveUnmanagedDll(AssemblyLoadContext? context, string unmanagedDllName);

    /// <summary>
    /// Registra manualmente una biblioteca de código nativo / no administrado asociada a un dominio o contexto ALC.
    /// </summary>
    /// <param name="domainName">Nombre del dominio o contexto.</param>
    /// <param name="unmanagedDllName">Nombre identificador de la biblioteca no administrada.</param>
    /// <param name="handle">Puntero al manejador de la biblioteca nativa cargada.</param>
    /// <param name="libraryPath">Ruta física al binario nativo.</param>
    void RegisterUnmanagedDll(string domainName, string unmanagedDllName, IntPtr handle, string libraryPath);
}
