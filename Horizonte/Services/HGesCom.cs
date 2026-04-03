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
    private readonly Lazy<IHorizonteEnv> _env;

    /// <summary>
    /// Representa una instancia de logger que se utiliza para registrar mensajes, advertencias y errores dentro de la clase HGesCom.
    /// </summary>
    /// <remarks>
    /// El logger se instancia de forma perezosa durante la inicialización del módulo y utiliza Microsoft.Extensions.Logging.
    /// Se emplea para proporcionar información de diagnóstico durante la carga de módulos, ejecución de comandos,
    /// y manejo de errores a lo largo del ciclo de vida de HGesCom.
    /// </remarks>
    private ILogger? _log;

    /// <summary>
    /// Una clase que implementa la interfaz IHGesCom, responsable de gestionar y ejecutar comandos
    /// dentro del entorno de Horizonte. Carga módulos de comandos, gestiona los comandos disponibles
    /// y facilita la interacción con estos comandos usando entradas basadas en objetos o JSON.
    /// </summary>
    public HGesCom(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
        LoadModules();
    }

    /// <summary>
    /// Carga e inicializa los módulos decorados con el atributo <see cref="HorizonteModule"/> 
    /// desde los ensamblados del dominio de aplicación actual.
    /// Este método crea dinámicamente instancias de esas clases de módulo, identifica sus métodos
    /// marcados con el atributo <see cref="HorizonteCommand"/> y los agrega a un diccionario interno de comandos.
    /// Esto permite que las operaciones o comandos se descubran y ejecuten dinámicamente en tiempo de ejecución.
    /// Cualquier error encontrado durante la instanciación de módulos o la inicialización de comandos
    /// se registra para su diagnóstico y el proceso continúa sin fallar por completo.
    /// </summary>
    private void LoadModules()
    {
        try
        {
            var enviorment = _env.Value;
            var assemblyManager = _env.Value.AssemblyManager;
            var processedAssemblies = new HashSet<string>();

            // Si no tenemos assemblyManager (muy raro), usamos el comportamiento anterior como fallback seguro
            var assembliesByDomain = assemblyManager?.AssembliesByDomain;

            if (assembliesByDomain == null)
            {
                Log.Warn("IhAssemblyManager not found or AssembliesByDomain is null. Falling back to AppDomain.CurrentDomain.");
                ProcessAssemblies(AppDomain.CurrentDomain.GetAssemblies().ToList(), "Default", processedAssemblies, enviorment);
                return;
            }

            foreach (var domainEntry in assembliesByDomain)
            {
                var domainName = domainEntry.Key;
                var assemblies = domainEntry.Value;
                ProcessAssemblies(assemblies, domainName, processedAssemblies, enviorment);
            }

            _log?.LogInformation($"Loaded {_commandList.Count} HCommands");
        }
        catch (Exception e)
        {
            _log?.LogError(e.ToString());
        }
    }

    private void ProcessAssemblies(List<Assembly> assemblies, string domainName, HashSet<string> processedAssemblies, IHorizonteEnv enviorment)
    {
        var assembliesToProcess = new Queue<Assembly>(assemblies);

        while (assembliesToProcess.Count > 0)
        {
            var assembly = assembliesToProcess.Dequeue();
            if (processedAssemblies.Contains(assembly.FullName!)) continue;
            processedAssemblies.Add(assembly.FullName!);

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }
            catch (Exception e)
            {
                Log.Error($"Error al obtener tipos del ensamblado {assembly.FullName} en dominio {domainName}: {e.Message}");
                continue;
            }

            var modulostypes = (from type in types
                                where Attribute.IsDefined(type, typeof(HorizonteModule))
                                select type).ToList();

            foreach (var modtype in modulostypes)
            {
                try
                {
                    if (modtype == null) continue;
                    object? modInstance;
                    try
                    {
                        Log.Info($">>>> Loading modules from '{modtype.FullName}' in domain '{domainName}'");

                        modInstance = Activator.CreateInstance(modtype, enviorment);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error al crear instancia: {modtype.FullName} en dominio {domainName}", e);
                        continue;
                    }

                    var metodos = modtype.GetMethods().Where(t => t.IsDefined(typeof(HorizonteCommand)));
                    foreach (var method in metodos)
                    {
                        if (modInstance != null)
                        {
                            AddCommandToList(method, modInstance, domainName);
                        }
                    }
                }
                catch (Exception e)
                {
                    _log?.LogError(e.ToString());
                }
            }
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
        _log = _env.Value.HHost.Services.GetService<ILogger<HGesCom>>();
        foreach (var item in GetRoleCommands("init"))
        {
            _log?.LogInformation("Ejecutando Init: " + item.CommandName);
            RunCommand(item.CommandName);
        }

        _log?.LogInformation("Módulos iniciados");
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
    /// Añade un método como comando al diccionario interno asociándolo con sus metadatos y propiedades.
    /// </summary>
    /// <param name="method">La información del método que será añadido como comando.</param>
    /// <param name="instance">La instancia del objeto que contiene el método que se va a agregar.</param>
    /// <param name="domain">El dominio (ALC) al que pertenece el comando.</param>
    private void AddCommandToList(MethodInfo method, object instance, string domain = "Default")
    {
        var hAttrib = method.CustomAttributes.First(t =>
            t.AttributeType == typeof(HorizonteCommand));
        var roleAttrib = (method.GetCustomAttributes(typeof(HorizonteRole), false)
            as HorizonteRole[] ?? []).ToList().Select(x => x.Role).ToList();

        var miCmd = new HCommand
        {
            CommandKey = hAttrib.ConstructorArguments[0].Value!.ToString()!,
            Description = hAttrib.ConstructorArguments[1].Value?.ToString() ?? string.Empty,
            CommandAction = method,
            Instance = instance,
            InTypes = method.GetParameters().Select(p => p.ParameterType).ToList(),
            InNames = GetParameterNames(method),
            OutType = method.ReturnType,
            Roles = roleAttrib,
            IsAsync = method.ReturnType == typeof(Task) ||
                      (method.ReturnType.IsGenericType &&
                       method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)),
            Domain = domain
        };
        _commandList.Add(miCmd.CommandKey, miCmd);
    }

    /// <summary>
    /// Recupera los nombres de los parámetros de un método dado.
    /// </summary>
    /// <param name="methodInfo">La información del método del cual se extraen los nombres de los parámetros.</param>
    /// <returns>
    /// Una lista de nombres de los parámetros para el método especificado o una lista vacía si no se encuentran parámetros.
    /// </returns>
    private List<string>? GetParameterNames(MethodInfo methodInfo)
    {
        return methodInfo.GetParameters()
            .Where(x => x.Name != null)
            .Select(parameter => parameter.Name!)
            .ToList();
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
            object?[]? inobjparams = null;
            var method = GetHCommand(commandKey);
            if (method == null) throw new KeyNotFoundException($"Comando {commandKey} no encontrado.");

            // Si el método requiere tipos de entrada y jsonarglist no es null
            var intypes = method.InTypes?.ToArray();
            var parameters = method.CommandAction.GetParameters();

            if (intypes != null && intypes.Length != 0)
            {
                var tmpobjlst = new List<object?>();
                int jsonIdx = 0;
                for (var i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    if (param.ParameterType == typeof(CancellationToken))
                    {
                        tmpobjlst.Add(CancellationToken.None);
                        continue;
                    }

                    if (jsonarglist != null && jsonIdx < jsonarglist.Length)
                    {
                        var T = param.ParameterType;
                        string jsonValue = jsonarglist[jsonIdx++];

                        // Comprobar si el tipo de entrada es string
                        if ((T == typeof(string) || T == typeof(DateTime)) && !jsonValue.StartsWith("\"") && !jsonValue.EndsWith("\""))
                        {
                            jsonValue = $"\"{jsonValue}\""; // Agregar comillas si faltan
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

                inobjparams = tmpobjlst.ToArray();
            }

            // Ejecutar el comando con o sin parámetros
            object? resobj = null;
            //if (method.IsAsync) // comprobamos sí el método está maracado cómo async
            //{
            //    resobj =  RunCommandAsync(method.CommandKey, inobjparams!).Result;
            //}
            //else
            //{
                resobj = RunCommand(method.CommandKey, inobjparams!);
            //}

            return JsonSerializer.Serialize(resobj);
        }
        catch (TargetInvocationException ex)
        {
            var inner = ex.InnerException ?? ex;
            _log?.LogError("RunCommandJson: Invocación fallida: " + inner.ToString());
            throw inner;
        }
        catch (Exception e)
        {
            _log?.LogError("RunCommandJson: " + e.ToString());
            throw;
        }
    }


    public async Task<string?> RunCommandJsonAsync(string commandKey, string[]? jsonarglist = null)
    {
        try
        {
            object?[]? inobjparams = null;
            var method = GetHCommand(commandKey);
            if (method == null) throw new KeyNotFoundException($"Comando {commandKey} no encontrado.");

            // Si el método requiere tipos de entrada y jsonarglist no es null
            var intypes = method.InTypes?.ToArray();
            if (intypes != null && intypes.Length != 0 && jsonarglist != null)
            {
                var tmpobjlst = new List<object?>();
                for (var i = 0; i < intypes.Length; i++)
                {
                    var T = intypes[i];
                    string jsonValue = jsonarglist[i];

                    // Comprobar si el tipo de entrada es string
                    if ((T == typeof(string) || T==typeof(DateTime) ) && !jsonValue.StartsWith("\"") && !jsonValue.EndsWith("\""))
                    {
                        jsonValue = $"\"{jsonValue}\""; // Agregar comillas si faltan
                    }

                    var serob = JsonSerializer.Deserialize(jsonValue, T);
                    tmpobjlst.Add(serob);

                }

                inobjparams = tmpobjlst.ToArray();
            }

            // Ejecutar el comando con o sin parámetros
            object? resobj = null;
            //if (method.IsAsync) // comprobamos sí el método está maracado cómo async
            //{
            
                resobj = await  RunCommandAsync(method.CommandKey, inobjparams!);
            
                //}
            //else
            //{
            //    resobj = RunCommand(method.CommandKey, inobjparams!);
            //}

            return JsonSerializer.Serialize(resobj);
        }
        catch (TargetInvocationException ex)
        {
            var inner = ex.InnerException ?? ex;
            _log?.LogError("RunCommandJsonAsync: Invocación fallida: " + inner.ToString());
            throw inner;
        }
        catch (Exception e)
        {
            _log?.LogError("RunCommandJsonAsync: " + e.ToString());
            throw;
        }
    }

    public bool IsAsyncCommand(string commandKey)
    {
        var method = GetHCommand(commandKey);
        if (method == null) return false;
        return method.IsAsync;
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

    public async Task<object?> RunCommandAsync(string commandKeyor, object[]? arg = null)
    {
        try
        {
            commandKeyor = commandKeyor.Trim();

            // Recuperamos el comando del diccionario a partir de su clave
            if (!_commandList.ContainsKey(commandKeyor))
            {
                _log?.LogError($"Comando {commandKeyor} no encontrado.");
                throw new KeyNotFoundException($"Comando {commandKeyor} no encontrado.");
            }

            var rCommand = _commandList[commandKeyor];

            if (rCommand.CommandAction == null) throw new InvalidOperationException($"Comando {commandKeyor} no tiene una acción asociada.");

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
                    return resultProperty?.GetValue(task);
                }

                return null; // Si es una Task void, no retornamos resultado
            }

            else
            {
                // Si el comando no es asíncrono, invocarlo de forma normal
                return rCommand.CommandAction.Invoke(rCommand.Instance, arg);
            }
        }
        catch (TargetInvocationException ex)
        {
            var inner = ex.InnerException ?? ex;
            _log?.LogError($"Excepción de invocación en RunCommandAsync para {commandKeyor}: {inner}");
            throw inner;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _log?.LogError($"Error al ejecutar RunCommandAsync para {commandKeyor}: {ex}");
            throw;
        }
    }


    public async Task<T> RunCommandAsync<T>(string commandKeyor,object[]? arg = null)
    {
        try
        {
            commandKeyor = commandKeyor.Trim();
    
            // Recuperamos el comando del diccionario a partir de su clave
            if (!_commandList.ContainsKey(commandKeyor))
            {
                _log?.LogError($"Comando {commandKeyor} no encontrado.");
                throw new KeyNotFoundException($"Comando {commandKeyor} no encontrado.");
            }
    
            var rCommand = _commandList[commandKeyor];
    
            if (rCommand.CommandAction == null) throw new InvalidOperationException($"Comando {commandKeyor} no tiene una acción asociada.");

            // Si el comando es async
            //if (typeof(Task).IsAssignableFrom(rCommand.CommandAction.ReturnType))
            if (rCommand.IsAsync)
            {
                var task = (Task)rCommand.CommandAction.Invoke(rCommand.Instance, arg)!;
    
                await task.ConfigureAwait(false);
    
                if (task.GetType().IsGenericType)
                {
                    var resultProperty = task.GetType().GetProperty("Result");
                    return (T)resultProperty?.GetValue(task)!;
                }
    
                return default!;
            }
            else
            {
                // Si no es async, se ejecuta normalmente
                var result = rCommand.CommandAction.Invoke(rCommand.Instance, arg);
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }

        catch (Exception ex)
        {
            _log?.LogError($"Error al ejecutar RunCommandAsync<T> para {commandKeyor}: {ex}");
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
        return RunCommand<object>(commandKeyor, arg);
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
        try
        {
            commandKeyor = commandKeyor.Trim();

            // recuperamos del diccionario el comando correspondiente al CommandKey
            if (!_commandList.ContainsKey(commandKeyor))
            {
                _log?.LogError($"Comando {commandKeyor} no encontrado.");
                throw new KeyNotFoundException($"Comando {commandKeyor} no encontrado.");
            }

            var rCommand = (HCommand)_commandList[commandKeyor];
            // invocamos el método de la clase Panel con los argumentos
            if (rCommand.CommandAction == null) throw new InvalidOperationException($"Comando {commandKeyor} no tiene una acción asociada.");
            var resObject = rCommand.CommandAction.Invoke(rCommand.Instance, arg);
            if (typeof(T) != typeof(object))
                resObject = Convert.ChangeType(resObject, typeof(T));
            //devolvemos el objeto
            return (T)resObject!;
        }

        catch (Exception ex)
        {
            _log?.LogError($"Error al ejecutar RunCommand<T> para {commandKeyor}: {ex}");
            throw;
        }
    }

    /// Recupera una instancia de un HCommand asociada con la clave de comando especificada.
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
            var enviorment = _env.Value;
            var assemblyManager = _env.Value.AssemblyManager;
            var processedAssemblies = new HashSet<string>();

            if (assemblyManager == null)
            {
                Log.Error("No se puede cargar comandos por dominio: IhAssemblyManager no disponible.");
                return;
            }

            var assembliesByDomain = assemblyManager.AssembliesByDomain;
            if (assembliesByDomain.TryGetValue(domainName, out var assemblies))
            {
                Log.Info($"Cargando comandos para el dominio: {domainName}");
                ProcessAssemblies(assemblies, domainName, processedAssemblies, enviorment);
            }
            else
            {
                Log.Warn($"No se encontraron ensamblados para el dominio {domainName}");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error en LoadCommandsByDomain para {domainName}: {e.Message}");
        }
    }
}