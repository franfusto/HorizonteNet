using System.Reflection;
using System.Runtime.Loader;

namespace Horizonte.Interfaces;

/// <summary>
/// Interfaz para la gestión directa de contextos de carga (AssemblyLoadContext) y aislamiento de dominios.
/// </summary>
public interface IHDomainManager
{
    /// <summary>
    /// Obtiene un diccionario de solo lectura con los dominios activos indexados por su nombre.
    /// </summary>
    IReadOnlyDictionary<string, AssemblyLoadContext> Domains { get; }
    
    /// <summary>
    /// Obtiene el contexto de carga (<see cref="AssemblyLoadContext"/>) asociado al nombre de dominio indicado.
    /// </summary>
    /// <param name="domainName">Nombre del dominio a buscar.</param>
    /// <returns>El contexto de carga correspondiente o <see langword="null"/> si no existe.</returns>
    AssemblyLoadContext? GetDomain(string domainName);

    /// <summary>
    /// Crea y registra un nuevo contexto de carga de ensamblados (<see cref="AssemblyLoadContext"/>) para un dominio.
    /// </summary>
    /// <param name="domainName">Nombre del dominio a crear.</param>
    /// <param name="isCollectible">Indica si el contexto de carga admite recolección de basura (unloading).</param>
    /// <returns>El nuevo <see cref="AssemblyLoadContext"/> creado.</returns>
    AssemblyLoadContext CreateDomain(string domainName, bool isCollectible = true);
    
    /// <summary>
    /// Carga de forma asíncrona el contexto de un dominio a partir de su configuración.
    /// </summary>
    /// <param name="domainName">Nombre del dominio a cargar.</param>
    /// <returns>Una tarea que devuelve el <see cref="AssemblyLoadContext"/> cargado.</returns>
    Task<AssemblyLoadContext> LoadDomainContextAsync(string domainName);

    /// <summary>
    /// Carga de forma asíncrona el contexto de un dominio a partir de los bytes binarios de sus ensamblados.
    /// </summary>
    /// <param name="domainName">Nombre del dominio a cargar.</param>
    /// <param name="assemblyBytes">Colección de bytes de los ensamblados a cargar.</param>
    /// <returns>Una tarea que devuelve el <see cref="AssemblyLoadContext"/> cargado.</returns>
    Task<AssemblyLoadContext> LoadDomainContextAsync(string domainName, IEnumerable<byte[]> assemblyBytes);

    /// <summary>
    /// Descarga y libera el contexto de un dominio de forma asíncrona.
    /// </summary>
    /// <param name="domainName">Nombre del dominio a descargar.</param>
    /// <returns>Una tarea que representa la operación asíncrona de descarga.</returns>
    Task UnloadDomainContextAsync(string domainName);

    /// <summary>
    /// Obtiene todos los ensamblados cargados en el contexto del dominio especificado.
    /// </summary>
    /// <param name="domainName">Nombre del dominio.</param>
    /// <returns>Secuencia de ensamblados asociados al dominio.</returns>
    IEnumerable<Assembly> GetDomainAssemblies(string domainName);
}
