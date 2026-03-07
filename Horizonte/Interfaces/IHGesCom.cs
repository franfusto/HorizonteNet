namespace Horizonte;

/// <summary>
/// Interfaz que define la funcionalidad central del sistema HGesCom, 
/// un sistema de gestión de comandos que permite la ejecución y administración de comandos basados en roles.
/// </summary>
public interface IHGesCom
{
    /// <summary>
    /// Inicializa varios módulos dentro del sistema ejecutando sus respectivos comandos de inicialización.
    /// </summary>
    /// <remarks>
    /// Este método recupera los comandos de inicialización asociados con el rol "init" de la lista de comandos basada en roles 
    /// y los ejecuta secuencialmente. Además, registra información relevante durante el proceso de inicialización.
    /// </remarks>
    public void InitzializeModules();

    /// <summary>
    /// Recupera una lista de comandos asociados con un rol específico.
    /// </summary>
    /// <param name="role">El rol para el cual se deben obtener los comandos.</param>
    /// <returns>Una lista de objetos <see cref="HGesCom.RoleListItem"/> que contiene información sobre los comandos disponibles para el rol especificado.</returns>
    public List<HGesCom.RoleListItem> GetRoleCommands(string role);

    /// <summary>
    /// Ejecuta un comando específico identificado por la clave proporcionada y, opcionalmente, acepta argumentos.
    /// </summary>
    /// <param name="commandKeyor">La clave o identificador del comando que se va a ejecutar.</param>
    /// <param name="arg">Un arreglo opcional de argumentos para pasar al comando. Por defecto es null.</param>
    /// <returns>Devuelve el resultado de la ejecución como un objeto, o null si el comando no produce un resultado.</returns>
    object? RunCommand(string commandKeyor, object[]? arg = null);
    
    /// <summary>
    /// Ejecuta un comando identificado por la clave proporcionada y, opcionalmente, le pasa argumentos.
    /// </summary>
    /// <typeparam name="T">El tipo del resultado devuelto por la ejecución del comando.</typeparam>
    /// <param name="commandKeyor">La clave o identificador asociado con el comando específico a ejecutar.</param>
    /// <param name="arg">Un arreglo opcional de argumentos para pasar al comando. Por defecto es null si no se proporcionan argumentos.</param>
    /// <returns>Devuelve el resultado de la ejecución del comando como el tipo especificado <typeparamref name="T"/>. Devuelve null si el comando no produce un resultado o si el tipo especificado es nullable.</returns>
    T? RunCommand<T>(string commandKeyor, object[]? arg = null);


    Task<object?> RunCommandAsync(string commandKeyor, object[]? arg = null);

    Task<T> RunCommandAsync<T>(string commandKeyor, object[]? arg = null);
    

    /// <summary>
    /// Ejecuta un comando basado en su clave de comando y opcionalmente argumentos de entrada proporcionados como cadenas JSON.
    /// Convierte los argumentos en formato JSON a sus tipos respectivos e invoca el comando correspondiente.
    /// </summary>
    /// <param name="commandKey">El identificador único o clave del comando que se va a ejecutar.</param>
    /// <param name="jsonarglist">Opcional. Un arreglo de argumentos serializados en formato JSON requeridos por el comando.</param>
    /// <returns>El objeto con la respuesta de la ejecución del comando o null si la ejecución falla.</returns>
    public string? RunCommandJson(string commandKey, string[]? jsonarglist);

    
    public Task<string?> RunCommandJsonAsync(string commandKey, string[]? jsonarglist);
    
    
    public bool IsAsyncCommand(string commandKey);
    
    /// <summary>
    /// Comprueba si existe un comando dada su clave.
    /// </summary>
    /// <param name="commandKey">La clave del comando a comprobar.</param>
    /// <returns>True si el comando existe, False en caso contrario.</returns>
    public bool ExistCommand(string commandKey);

    /// <summary>
    /// Recupera una lista de comandos disponibles.
    /// </summary>
    /// <returns>
    /// Una colección enumerable de objetos <see cref="HCommand"/>.
    /// </returns>
    public IEnumerable<HCommand> GetCommandList();

    /// <summary>
    /// Recupera una instancia de un HCommand asociada con la clave de comando especificada.
    /// </summary>
    /// <param name="commandKey">El identificador único del comando.</param>
    /// <returns>La instancia de HCommand o null si no se encuentra.</returns>
    public HCommand? GetHCommand(string commandKey);

    /// <summary>
    /// Elimina los comandos asociados a un dominio específico y limpia las instancias creadas.
    /// </summary>
    /// <param name="domainName">El nombre del dominio (ALC).</param>
    public void UnloadCommandsByDomain(string domainName);

    /// <summary>
    /// Carga los comandos de los ensamblados asociados a un dominio específico.
    /// </summary>
    /// <param name="domainName">El nombre del dominio (ALC).</param>
    public void LoadCommandsByDomain(string domainName);
}