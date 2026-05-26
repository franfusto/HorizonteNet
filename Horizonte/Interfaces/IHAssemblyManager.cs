using System.Reflection;
using System.Runtime.Loader;

namespace Horizonte;

/// <summary>
/// La interfaz IhAssemblyManager define un conjunto de métodos y propiedades
/// para gestionar ensamblados en un contexto dinámico dentro del sistema.
/// </summary>
public interface IhAssemblyManager
{
    /// <summary>
    /// Proporciona una lista de todas las ensamblados cargados en el contexto predeterminado de carga de ensamblados
    /// así como en los distintos contextos de carga de ensamblados personalizados que puedan existir en la aplicación.
    /// </summary>
    /// <remarks>
    /// Los ensamblados se obtienen tanto del contexto de carga de ensamblados por defecto como de los contextos adicionales
    /// definidos dentro de la aplicación, permitiendo así un manejo organizado y estructurado de los recursos de la aplicación.
    /// </remarks>
    public List<Assembly> Assemblies { get; }

    /// <summary>
    /// Propiedad que proporciona un diccionario donde las claves son nombres de dominio
    /// y los valores son listas de ensamblados (.NET assemblies) asociados a cada dominio.
    /// Esta estructura permite gestionar y organizar los ensamblados por dominios específicos,
    /// facilitando la carga, revisión y manipulación de estos en la aplicación.
    /// </summary>
    public Dictionary<string, List<Assembly>> AssembliesByDomain { get; }

    /// <summary>
    /// Evento que se dispara cuando hay un cambio en el dominio.
    /// </summary>
    /// <remarks>
    /// Es utilizado para notificar a los suscriptores que ha ocurrido una modificación en el dominio gestionado.
    /// </remarks>
    public event Action? DomainChanged;

    /// <summary>
    /// Método que intenta resolver y cargar un ensamblado dado su contexto de carga y su nombre.
    /// </summary>
    /// <param name="context">El contexto de carga de ensamblados actual, que representa el dominio de carga donde se está intentando resolver el ensamblado.</param>
    /// <param name="assemblyName">El nombre del ensamblado que se está intentando cargar y resolver.</param>
    /// <returns>Retorna el ensamblado que se pudo resolver y cargar, o <c>null</c> si no se pudo cargar el ensamblado especificado.</returns>
    public Assembly? ResolveAssemblyFromALC(AssemblyLoadContext context, AssemblyName assemblyName);

    /// <summary>
    /// Intenta resolver y cargar un ensamblado desde los paquetes de NuGet, utilizando
    /// un evento de resolución que proporciona detalles sobre el ensamblado a cargar.
    /// </summary>
    /// <param name="sender">Origen del evento de resolución, puede ser nulo.</param>
    /// <param name="args">Argumentos que especifican el nombre completo del ensamblado a resolver y cargar.</param>
    /// <returns>El ensamblado resuelto y cargado desde los paquetes de NuGet, o <c>null</c> si no se pudo cargar.</returns>
    public Assembly? ResolveAssemblyFromNuGetPackages(object? sender, ResolveEventArgs args);

    /// <summary>
    /// Método que resuelve la ruta del archivo DLL de un paquete NuGet específico, dado su nombre y versión.
    /// </summary>
    /// <param name="packageName">El nombre del paquete NuGet del cual se desea obtener la ruta de su archivo DLL.</param>
    /// <param name="version">La versión específica del paquete NuGet requerida.</param>
    /// <returns>Retorna la ruta absoluta al archivo DLL del paquete si se encuentra; de lo contrario, retorna <c>null</c>.</returns>
    public string? ResolveAssemblyDllPath(string packageName, string version);

    /// <summary>
    /// Descarga un dominio específico de la aplicación.
    /// </summary>
    /// <param name="domainName">El nombre del dominio que se desea descargar.</param>
    /// <return>Una tarea que representa la operación asincrónica de descarga del dominio.</return>
    public Task UnloadDomain(string domainName);

    /// <summary>
    /// Carga un dominio específico en el contexto de la aplicación.
    /// </summary>
    /// <param name="domainName">El nombre del dominio que se va a cargar.</param>
    /// <returns>Un <see cref="Task"/> que representa la operación asincrónica de carga del dominio.</returns>
    public Task LoadDomain(string domainName);

    /// <summary>
    /// Método que carga un dominio específico con un conjunto de ensamblados proporcionados.
    /// </summary>
    /// <param name="domainName">
    /// El nombre del dominio que se va a cargar.
    /// </param>
    /// <param name="assemblies">
    /// Una colección de arreglos de bytes que representan los ensamblados que se deben cargar en el dominio especificado.
    /// </param>
    /// <returns>
    /// Un objeto <c>Task</c> que representa la operación asincrónica de carga del dominio.
    /// </returns>
    public Task LoadDomain(string domainName, IEnumerable<byte[]> assemblies);

    /// <summary>
    /// Recarga un dominio especificado. Este método primero descarga el dominio
    /// si está cargado y luego carga nuevamente el dominio con el mismo nombre.
    /// No se realizará ninguna acción si el nombre del dominio es igual al nombre
    /// del dominio predeterminado.
    /// </summary>
    /// <param name="domainName">
    /// El nombre del dominio que se desea recargar.
    /// </param>
    /// <return>
    /// Un objeto <see cref="Task"/> que representa la operación asincrónica de recarga del dominio.
    /// </return>
    public Task ReloadDomain(string domainName);


    /// <summary>
    /// Método que descarga un módulo asociado a un dominio específico.
    /// </summary>
    /// <param name="domainName">El nombre del dominio del cual se debe descargar el módulo.</param>
    public void UnloadModule(string domainName);

    /// <summary>
    /// Método que se encarga de descargar los servicios asociados a un dominio específico.
    /// </summary>
    /// <param name="domainName">
    /// El nombre del dominio cuyas tareas de servicio se van a descargar. Si el nombre del dominio es el dominio predeterminado, la operación no se realiza.
    /// </param>
    public void UnloadService(string domainName);

    /// <summary>
    /// Método que carga los módulos asociados a un nombre de dominio específico.
    /// </summary>
    /// <param name="domainName">El nombre del dominio para el cual se deben cargar los módulos.</param>
    public void LoadModule(string domainName);

    /// <summary>
    /// Método que carga y gestiona los servicios en segundo plano para un dominio específico.
    /// Este método asegura que los servicios adecuados se inicien para el dominio especificado,
    /// dependiendo de la configuración predefinida de los trabajadores.
    /// </summary>
    /// <param name="domainName">
    /// El nombre del dominio que se va a cargar y para el cual se deben iniciar los servicios en segundo plano.
    /// El método verifica la validez del dominio antes de proceder con la carga de servicios.
    /// </param>
    public void LoadService(string domainName);

    /// <summary>
    /// StaticFileRegistry es una clase dentro del espacio de nombres Horizonte.
    /// Esta clase se utiliza para gestionar y registrar archivos estáticos dentro del contexto de la aplicación que la emplea.
    /// </summary>
    public StaticFileRegistry StaticFileRegistry { get; }

    /// <summary>
    /// Método que descarga los comandos asociados a un dominio específico.
    /// </summary>
    /// <param name="domainName">El nombre del dominio cuyos comandos se deben descargar.</param>
    public void UnloadCommandsByDomain(string domainName);

    /// <summary>
    /// Método que carga y procesa los comandos asociados a un dominio específico, identificando el conjunto de ensamblados pertinentes y gestionando cualquier error que pueda surgir durante el proceso.
    /// </summary>
    /// <param name="domainName">El nombre del dominio para el cual se cargan y procesan los comandos de ensamblados.</param>
    public void LoadCommandsByDomain(string domainName);

    /// <summary>
    /// Crea una instancia de un tipo especificado utilizando el proveedor de servicios.
    /// Intenta resolver los parámetros del constructor automáticamente.
    /// </summary>
    /// <param name="type">El tipo del cual se desea crear una instancia.</param>
    /// <returns>Una instancia del tipo especificado si se pudo crear; de lo contrario, <c>null</c>.</returns>
    /// <exception cref="MissingMethodException">Se lanza si el tipo no tiene constructores públicos.</exception>
    /// <exception cref="InvalidOperationException">Se lanza si no se puede crear una instancia del tipo debido a la imposibilidad de resolver los parámetros del constructor.</exception>
    public object? CreateInstance(Type type);

    /// <summary>
    /// Inicia un servicio en segundo plano especificado por el tipo de servicio proporcionado.
    /// </summary>
    /// <param name="serviceType">El tipo de servicio que se desea iniciar. Este valor no debe ser nulo, vacío ni contener solo espacios en blanco.</param>
    /// <returns>Devuelve <c>true</c> si el servicio en segundo plano se inicia correctamente; de lo contrario, devuelve <c>false</c>.</returns>
    public bool StartBackgroundService(string serviceType);

    /// <summary>
    /// Método que intenta detener un servicio en segundo plano identificado por su tipo específico.
    /// </summary>
    /// <param name="serviceType">El tipo del servicio en segundo plano que se intenta detener. Se requiere que no sea una cadena vacía o solo espacios en blanco.</param>
    /// <returns>Devuelve <c>true</c> si el servicio fue detenido con éxito o si ya estaba detenido; en caso contrario, devuelve <c>false</c>.</returns>
    public bool StopBackgroundService(string serviceType);

    /// <summary>
    /// Verifica si un servicio de fondo específico está en ejecución.
    /// </summary>
    /// <param name="serviceType">El tipo del servicio cuyo estado se desea verificar.</param>
    /// <returns>Retorna <c>true</c> si el servicio de fondo está en ejecución y su instancia no es nula; de lo contrario, <c>false</c>.</returns>
    public bool BackgroundServiceRunning(string serviceType);
}