using System.Reflection;
using Horizonte.Helpers;

namespace Horizonte.Interfaces;

/// <summary>
/// Interfaz del orquestador central y fachada del entorno modular de HorizonteNet.
/// </summary>
public interface IEnvCoordinator
{
    /// <summary>
    /// Obtiene el resolutor de ensamblados y paquetes del entorno.
    /// </summary>
    IHAssemblySolver AssemblySolver { get; }

    /// <summary>
    /// Obtiene el gestor de dominios y contextos de carga de ensamblados (AssemblyLoadContext).
    /// </summary>
    IHDomainManager DomainManager { get; }

    /// <summary>
    /// Obtiene el gestor de comandos del entorno.
    /// </summary>
    IHGesCom GesCom { get; }

    /// <summary>
    /// Obtiene el gestor de workers y servicios en segundo plano.
    /// </summary>
    IhWorkersManager WorkersManager { get; }

    /// <summary>
    /// Obtiene la lista consolidada de todos los ensamblados cargados en el entorno.
    /// </summary>
    IReadOnlyList<Assembly> Assemblies { get; }

    /// <summary>
    /// Obtiene un diccionario con los ensamblados agrupados por el nombre de su dominio de carga.
    /// </summary>
    IReadOnlyDictionary<string, List<Assembly>> AssembliesByDomain { get; }

    /// <summary>
    /// Obtiene el registro de archivos estáticos y activos web disponibles en el entorno.
    /// </summary>
    StaticFileRegistry StaticFileRegistry { get; }

    /// <summary>
    /// Evento que se desencadena cuando cambia la composición o el estado de los dominios cargados.
    /// </summary>
    event Action? DomainChanged;

    /// <summary>
    /// Carga un dominio modular a partir de su configuración registrada.
    /// </summary>
    /// <param name="domainName">Nombre identificador del dominio.</param>
    /// <returns>Una tarea que representa la operación asíncrona de carga.</returns>
    Task LoadDomainAsync(string domainName);

    /// <summary>
    /// Carga un dominio modular a partir de una secuencia de bytes de ensamblados en memoria.
    /// </summary>
    /// <param name="domainName">Nombre identificador del dominio.</param>
    /// <param name="assemblies">Secuencia de arreglos de bytes correspondientes a los ensamblados a cargar.</param>
    /// <returns>Una tarea que representa la operación asíncrona de carga.</returns>
    Task LoadDomainAsync(string domainName, IEnumerable<byte[]> assemblies);

    /// <summary>
    /// Descarga un dominio modular y libera sus recursos asociados.
    /// </summary>
    /// <param name="domainName">Nombre identificador del dominio a descargar.</param>
    /// <returns>Una tarea que representa la operación asíncrona de descarga.</returns>
    Task UnloadDomainAsync(string domainName);

    /// <summary>
    /// Recarga un dominio modular existente, actualizando sus ensamblados y servicios.
    /// </summary>
    /// <param name="domainName">Nombre identificador del dominio a recargar.</param>
    /// <returns>Una tarea que representa la operación asíncrona de recarga.</returns>
    Task ReloadDomainAsync(string domainName);

    /// <summary>
    /// Inicia un servicio en segundo plano (worker) por su nombre de tipo o servicio.
    /// </summary>
    /// <param name="serviceType">Nombre del tipo o servicio a iniciar.</param>
    /// <returns><see langword="true"/> si el servicio se inició con éxito; en caso contrario, <see langword="false"/>.</returns>
    bool StartBackgroundService(string serviceType);

    /// <summary>
    /// Detiene un servicio en segundo plano (worker) por su nombre de tipo o servicio.
    /// </summary>
    /// <param name="serviceType">Nombre del tipo o servicio a detener.</param>
    /// <returns><see langword="true"/> si el servicio se detuvo con éxito; en caso contrario, <see langword="false"/>.</returns>
    bool StopBackgroundService(string serviceType);

    /// <summary>
    /// Comprueba si un servicio en segundo plano (worker) está actualmente en ejecución.
    /// </summary>
    /// <param name="serviceType">Nombre del tipo o servicio a consultar.</param>
    /// <returns><see langword="true"/> si el servicio está en ejecución; en caso contrario, <see langword="false"/>.</returns>
    bool BackgroundServiceRunning(string serviceType);

    /// <summary>
    /// Carga un módulo dentro del dominio especificado.
    /// </summary>
    /// <param name="domainName">Nombre identificador del dominio.</param>
    void LoadModule(string domainName);

    /// <summary>
    /// Descarga un módulo del dominio especificado.
    /// </summary>
    /// <param name="domainName">Nombre identificador del dominio.</param>
    void UnloadModule(string domainName);

    /// <summary>
    /// Registra y carga los comandos expuestos por los ensamblados de un dominio.
    /// </summary>
    /// <param name="domainName">Nombre identificador del dominio.</param>
    void LoadCommandsByDomain(string domainName);

    /// <summary>
    /// Descarga y desregistra los comandos pertenecientes a un dominio.
    /// </summary>
    /// <param name="domainName">Nombre identificador del dominio.</param>
    void UnloadCommandsByDomain(string domainName);

    /// <summary>
    /// Crea una instancia del tipo especificado utilizando el contenedor de inyección de dependencias del entorno.
    /// </summary>
    /// <param name="type">Tipo del objeto a instanciar.</param>
    /// <returns>La instancia creada o <see langword="null"/> si no se pudo resolver.</returns>
    object? CreateInstance(Type type);

    /// <summary>
    /// Carga explícitamente una biblioteca de código nativo / no administrado (.so, .dll, .dylib) en el dominio especificado.
    /// </summary>
    /// <param name="domainName">Nombre del dominio o contexto en el que se asociará la biblioteca (si es nulo o vacío se utiliza el dominio por defecto).</param>
    /// <param name="unmanagedDllName">Nombre identificador de la biblioteca no administrada esperada por P/Invoke.</param>
    /// <param name="libraryPath">Ruta física (absoluta o relativa) al archivo binario nativo.</param>
    /// <returns>Puntero al manejador nativo cargado o <see cref="IntPtr.Zero"/> si falló la carga.</returns>
    IntPtr LoadUnmanagedDll(string? domainName, string unmanagedDllName, string libraryPath);
}
