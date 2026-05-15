using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using log4net;


namespace Horizonte;

/// <summary>
/// La clase HGesCom es responsable de gestionar la ejecución de comandos en el sistema Horizonte.
/// Ofrece funcionalidades para inicializar, ejecutar y recuperar comandos, junto con la capacidad
/// de filtrar comandos basados en roles. Implementa la interfaz <see cref="IHGesCom"/>.
/// </summary>
/// <remarks>
/// Esta clase actúa como un punto central para manejar objetos de tipo HCommand. Ofrece capacidades como:
/// - Ejecutar comandos con argumentos.
/// - Retornar resultados en formato JSON tras la ejecución de comandos.
/// - Recuperar una lista de comandos disponibles para un rol específico.
/// Los comandos se cargan e inicializan al construir la instancia de HGesCom.
/// </remarks>
public class HGesCom : IHGesCom
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(HGesCom));
    /// <summary>
    /// Representa un elemento en la lista de comandos específicos de un rol.
    /// </summary>
    /// <remarks>
    /// Cada instancia de esta clase contiene información básica sobre un comando,
    /// incluyendo su nombre y una descripción de su funcionalidad.
    /// </remarks>
    public record RoleListItem(string CommandName, string Description);

    /// <summary>
    /// Un diccionario que sirve como almacenamiento central para todos los comandos disponibles en el sistema.
    /// </summary>
    /// <remarks>
    /// Cada comando está identificado por una clave única de tipo cadena (<c>string</c>) y es representado por una instancia
    /// de la clase <c>HCommand</c>. Este diccionario se utiliza para registrar dinámicamente comandos
    /// durante la carga de módulos y para acceder a metadatos asociados, como descripciones,
    /// roles, especificaciones de entrada/salida y detalles de ejecución.
    /// </remarks>
    private readonly Dictionary<string, HCommand> _commandList = new();

    /// <summary>
    /// Una instancia cargada perezosamente (lazy-loaded) de la interfaz IHorizonteEnv, que proporciona acceso
    /// a los servicios, configuraciones y operaciones del entorno de Horizonte.
    /// Esta variable se utiliza para gestionar la carga de módulos, inicialización y registro de logs dentro de la clase HGesCom.
    /// </summary>
    private readonly Lazy<IhAssemblyManager> _assemblyManager;

    /// <summary>
    /// Una clase que implementa la interfaz IHGesCom, responsable de gestionar y ejecutar comandos
    /// dentro del entorno de Horizonte. Carga módulos de comandos, gestiona los comandos disponibles
    /// y facilita la interacción con estos comandos usando entradas basadas en objetos o JSON.
    /// </summary>
    public HGesCom(IServiceProvider serviceProvider)
    {
        _assemblyManager = new Lazy<IhAssemblyManager>(() => serviceProvider.GetRequiredService<IhAssemblyManager>());
    }

    /// <summary>
    /// Carga e inicializa los módulos decorados con el atributo <see cref="HorizonteModule"/> 
    /// desde los ensamblados del dominio de aplicación actual.
    /// Este método delega la carga a HAssemblyManager.
    /// </summary>
    public void LoadModules()
    {
        try
        {
            var assemblyManager = _assemblyManager.Value;
            if (assemblyManager == null)
            {
                Log.Error("IhAssemblyManager not found. Cannot load modules.");
                return;
            }

            // Limpiamos comandos previos si los hubiera (útil para re-carga)
            _commandList.Clear();

            var assembliesByDomain = assemblyManager.AssembliesByDomain;
            foreach (var domainName in assembliesByDomain.Keys)
            {
                assemblyManager.LoadCommandsByDomain(domainName);
            }

            Log.Info($"Loaded {_commandList.Count} HCommands");
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
    }

    /// <summary>
    /// Inicializa varios módulos en el sistema invocando sus respectivos comandos de inicialización.
    /// </summary>
    /// <remarks>
    /// Este método recupera los comandos de inicialización asociados con el rol "init" de la lista
    /// de comandos por rol y los ejecuta secuencialmente. También registra información relevante
    /// durante el proceso de inicialización.
    /// </remarks>
    public void InitzializeModules()
    {
        foreach (var item in GetRoleCommands("init"))
        {
            Log.Info("Ejecutando Init: " + item.CommandName);
            RunCommand(item.CommandName);
        }

        Log.Info("Módulos iniciados");
    }

    /// <summary>
    /// Recupera una lista de comandos asociados con un rol específico.
    /// </summary>
    /// <param name="role">El rol para el que deben recuperarse los comandos.</param>
    /// <returns>
    /// Una lista de objetos <c>HGesCom.RoleListItem</c> que representan los comandos disponibles para el rol especificado.
    /// </returns>
    public List<RoleListItem> GetRoleCommands(string role)
    {
        return _commandList
            .Where(x => x.Value.Roles.Contains(role))
            .Select(item => new RoleListItem(item.Value.CommandKey, item.Value.Description))
            .ToList();
    }

    /// <summary>
    /// Registra un comando en el sistema.
    /// </summary>
    /// <param name="command">El comando a registrar.</param>
    public void RegisterCommand(HCommand command)
    {
        if (command == null) return;
        if (string.IsNullOrEmpty(command.CommandKey)) return;
        
        if (_commandList.ContainsKey(command.CommandKey))
        {
            _commandList[command.CommandKey] = command;
        }
        else
        {
            _commandList.Add(command.CommandKey, command);
        }
    }

    /// <summary>
    /// Ejecuta un comando identificado por su clave usando argumentos en formato JSON.
    /// Este método deserializa los argumentos, ejecuta el comando y serializa el resultado.
    /// </summary>
    /// <param name="commandKey">Clave única que identifica el comando a ejecutar.</param>
    /// <param name="jsonarglist">Matriz de cadenas JSON que representan los argumentos requeridos por el comando.</param>
    /// <returns>
    /// Una cadena JSON que representa el resultado de la ejecución del comando o null si falla.
    /// </returns>
    public string? RunCommandJson(string commandKey, string[]? jsonarglist)
    {
        try
        {
            return RunCommandJsonAsync(commandKey, jsonarglist).GetAwaiter().GetResult();
        }
        catch (TargetInvocationException ex)
        {
            var inner = ex.InnerException ?? ex;
            Log.Error("RunCommandJson: Invocación fallida: " + inner.ToString());
            throw inner;
        }
        catch (Exception e)
        {
            Log.Error("RunCommandJson: " + e.ToString());
            throw;
        }
    }


    /// <summary>
    /// Ejecuta un comando de forma asíncrona basado en su clave y argumentos en formato JSON.
    /// </summary>
    /// <param name="commandKey">El identificador único o clave del comando que se va a ejecutar.</param>
    /// <param name="jsonarglist">Opcional. Un arreglo de argumentos serializados en formato JSON.</param>
    /// <returns>Una tarea que devuelve el resultado de la ejecución serializado en JSON o null.</returns>
    public async Task<string?> RunCommandJsonAsync(string commandKey, string[]? jsonarglist = null)
    {
        try
        {
            var method = GetHCommand(commandKey);
            if (method == null) throw new KeyNotFoundException($"Comando {commandKey} no encontrado.");

            var inobjparams = PrepareParameters(method, jsonarglist);
            var resobj = await RunCommandAsync(method.CommandKey, inobjparams!);

            return JsonSerializer.Serialize(resobj);
        }
        catch (TargetInvocationException ex)
        {
            var inner = ex.InnerException ?? ex;
            Log.Error("RunCommandJsonAsync: Invocación fallida: " + inner.ToString());
            throw inner;
        }
        catch (Exception e)
        {
            Log.Error("RunCommandJsonAsync: " + e.ToString());
            throw;
        }
    }

    /// <summary>
    /// Prepara los parámetros para la ejecución de un comando a partir de una lista de argumentos en formato JSON.
    /// </summary>
    /// <param name="method">El comando para el cual se preparan los parámetros.</param>
    /// <param name="jsonarglist">Lista de argumentos en formato JSON.</param>
    /// <returns>Un arreglo de objetos con los parámetros procesados.</returns>
    private object?[]? PrepareParameters(HCommand method, string[]? jsonarglist)
    {
        
        var parameters = method.CommandAction!.GetParameters();
        if (parameters.Length == 0) return null;

        var tmpobjlst = new List<object?>();
        var jsonIdx = 0;

        foreach (var param in parameters)
        {
            if (param.ParameterType == typeof(CancellationToken))
            {
                tmpobjlst.Add(CancellationToken.None);
                continue;
            }

            if (jsonarglist != null && jsonIdx < jsonarglist.Length)
            {
                var T = param.ParameterType;
                string jsonValue = jsonarglist[jsonIdx++];

                // Comprobar si el tipo de entrada es string o DateTime y agregar comillas si faltan
                if ((T == typeof(string) || T == typeof(DateTime)) && !jsonValue.StartsWith("\"") && !jsonValue.EndsWith("\""))
                {
                    jsonValue = $"\"{jsonValue}\"";
                }

                var serob = JsonSerializer.Deserialize(jsonValue, T);
                tmpobjlst.Add(serob);
            }
            else
            {
                // Manejar argumentos faltantes si no es un CancellationToken
                tmpobjlst.Add(param.HasDefaultValue ? param.DefaultValue : null);
            }
        }

        return tmpobjlst.ToArray();
    }

    /// <summary>
    /// Determina si un comando está marcado como asíncrono.
    /// </summary>
    /// <param name="commandKey">La clave del comando a comprobar.</param>
    /// <returns>True si el comando es asíncrono, False en caso contrario.</returns>
    public bool IsAsyncCommand(string commandKey)
    {
        var method = GetHCommand(commandKey);
        return method is { IsAsync: true };
    }

    /// <summary>
    /// Comprueba si existe un comando dada su clave.
    /// </summary>
    /// <param name="commandKey">La clave del comando a comprobar.</param>
    /// <returns>True si el comando existe, False en caso contrario.</returns>
    public bool ExistCommand(string commandKey)
    {
        return _commandList.ContainsKey(commandKey);
    }

    /// <summary>
    /// Ejecuta un comando de forma asíncrona identificado por la clave proporcionada.
    /// </summary>
    /// <param name="commandKeyor">La clave o identificador del comando que se va a ejecutar.</param>
    /// <param name="arg">Un arreglo opcional de argumentos para pasar al comando. Por defecto es null.</param>
    /// <returns>Una tarea que representa la operación asíncrona y devuelve el resultado de la ejecución como un objeto, o null.</returns>
    public async Task<object?> RunCommandAsync(string commandKeyor, object[]? arg = null)
    {
        return await RunCommandAsync<object?>(commandKeyor, arg).ConfigureAwait(false);
    }


    /// <summary>
    /// Ejecuta un comando de forma asíncrona identificado por la clave proporcionada y devuelve un resultado tipado.
    /// </summary>
    /// <typeparam name="T">El tipo del resultado devuelto por la ejecución del comando.</typeparam>
    /// <param name="commandKeyor">La clave o identificador asociado con el comando específico a ejecutar.</param>
    /// <param name="arg">Un arreglo opcional de argumentos para pasar al comando. Por defecto es null.</param>
    /// <returns>Una tarea que representa la operación asíncrona y devuelve el resultado convertido al tipo <typeparamref name="T"/>.</returns>
    public async Task<T> RunCommandAsync<T>(string commandKeyor, object[]? arg = null)
    {
        try
        {
            commandKeyor = commandKeyor.Trim();

            // Recuperamos el comando del diccionario a partir de su clave
            if (!_commandList.ContainsKey(commandKeyor))
            {
                Log.Error($"Comando {commandKeyor} no encontrado.");
                throw new KeyNotFoundException($"Comando {commandKeyor} no encontrado.");
            }

            var rCommand = _commandList[commandKeyor];

            if (rCommand.CommandAction == null) throw new InvalidOperationException($"Comando {commandKeyor} no tiene una acción asociada.");

            object? result;

            // Revisar si el comando es una tarea asíncrona
            if (typeof(Task).IsAssignableFrom(rCommand.CommandAction.ReturnType))
            {
                // Invocamos el comando de forma asíncrona
                var task = (Task)rCommand.CommandAction.Invoke(rCommand.Instance, arg)!;

                // Esperamos si la tarea tiene resultado (Task<T>)
                await task.ConfigureAwait(false);

                if (rCommand.CommandAction.ReturnType.IsGenericType)
                {
                    // Obtenemos el resultado de la tarea genérica Task<T>
                    var resultProperty = rCommand.CommandAction.ReturnType.GetProperty("Result");
                    result = resultProperty?.GetValue(task);
                }
                else
                {
                    result = null; // Si es una Task void, no retornamos resultado
                }
            }
            else
            {
                // Si el comando no es asíncrono, invocarlo de forma normal
                result = rCommand.CommandAction.Invoke(rCommand.Instance, arg);
            }

            if (result == null) return default!;
            if (typeof(T) == typeof(object)) return (T)result;
            return (T)Convert.ChangeType(result, typeof(T));
        }
        catch (TargetInvocationException ex)
        {
            var inner = ex.InnerException ?? ex;
            Log.Error($"Excepción de invocación en RunCommandAsync para {commandKeyor}: {inner}");
            throw inner;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error($"Error al ejecutar RunCommandAsync para {commandKeyor}: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Ejecuta un comando identificado por la clave de comando especificada, opcionalmente con una lista de argumentos.
    /// </summary>
    /// <param name="commandKeyor">La clave que representa el comando a ser ejecutado.</param>
    /// <param name="arg">Matriz opcional de argumentos necesarios para la ejecución del comando.</param>
    /// <returns>
    /// El resultado de la ejecución del comando como un objeto, o null si el comando falla o no se encuentra.
    /// </returns>
    public object? RunCommand(string commandKeyor, object[]? arg = null)
    {
        return RunCommandAsync(commandKeyor, arg).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Ejecuta un comando basado en la clave de comando proporcionada y los argumentos, y devuelve el resultado.
    /// El valor devuelto puede convertirse al tipo genérico especificado <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">El tipo esperado del resultado del comando.</typeparam>
    /// <param name="commandKeyor">La clave utilizada para identificar el comando a ejecutar.</param>
    /// <param name="arg">Una matriz de argumentos para pasar al comando. Puede ser null si no se requieren argumentos.</param>
    /// <returns>
    /// El resultado del comando ejecutado, convertido al tipo especificado <typeparamref name="T"/>.
    /// Devuelve el valor predeterminado del tipo <typeparamref name="T"/> si el comando falla o no se encuentra.
    /// </returns>
    public T? RunCommand<T>(string commandKeyor, object[]? arg = null)
    {
        return RunCommandAsync<T>(commandKeyor, arg).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Recupera una instancia de un HCommand asociada con la clave de comando especificada.
    /// </summary>
    /// <param name="commandKey">
    /// El identificador único del comando que se desea recuperar.
    /// </param>
    /// <returns>
    /// Una instancia de HCommand asociada con la clave de comando proporcionada si se encuentra; de lo contrario, null.
    /// </returns>
    public HCommand? GetHCommand(string commandKey)
    {
        _commandList.TryGetValue(commandKey, out HCommand? rCommand);
        return rCommand;
    }

    /// <summary>
    /// Recupera la lista de todos los comandos registrados en la instancia actual.
    /// </summary>
    /// <returns>
    /// Una colección de objetos <see cref="HCommand"/> que representan los comandos disponibles.
    /// </returns>
    public IEnumerable<HCommand> GetCommandList()
    {
        return _commandList.Values.ToList();
    }

    /// <summary>
    /// Elimina de la lista de comandos aquellos que pertenezcan al dominio especificado.
    /// </summary>
    /// <param name="domainName">Nombre del dominio (ALC).</param>
    public void UnloadCommandsByDomain(string domainName)
    {
        try
        {
            var keysToRemove = _commandList.Where(x => x.Value.Domain == domainName).Select(x => x.Key).ToList();
            foreach (var key in keysToRemove)
            {
                if (_commandList.TryGetValue(key, out var command))
                {
                    // Si la instancia implementa IDisposable, la liberamos
                    if (command.Instance is IDisposable disposable)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Log.Warn($"Error al liberar instancia del comando {key} en el dominio {domainName}: {ex.Message}");
                        }
                    }
                    _commandList.Remove(key);
                }
            }
            Log.Info($"Desvinculados {keysToRemove.Count} comandos del dominio {domainName}");
        }
        catch (Exception e)
        {
            Log.Error($"Error en UnloadCommandsByDomain para {domainName}: {e.Message}");
        }
    }

    /// <summary>
    /// Cargamos los comandos de los ensamblados asociados a un dominio específico.
    /// </summary>
    /// <param name="domainName">El nombre del dominio (ALC).</param>
    public void LoadCommandsByDomain(string domainName)
    {
        try
        {
            var assemblyManager = _assemblyManager.Value;

            if (assemblyManager == null)
            {
                Log.Error("No se puede cargar comandos por dominio: IhAssemblyManager no disponible.");
                return;
            }

            assemblyManager.LoadCommandsByDomain(domainName);
        }
        catch (Exception e)
        {
            Log.Error($"Error en LoadCommandsByDomain para {domainName}: {e.Message}");
        }
    }
}