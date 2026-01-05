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
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //llenamos diccionario de commandos
            foreach (var assembly in assemblies)
            {
                //Console.WriteLine($"*** Processing assembly '{assembly.FullName}'");
                var modulostypes = (from type in assembly.GetTypes()
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
                            Log.Info($">>>> Loading modules from '{modtype.FullName}'");

                            modInstance = Activator.CreateInstance(modtype, enviorment);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Error al crear instancia: " + modtype.FullName, e);
                            continue;
                        }

                        var metodos = modtype.GetMethods().Where(t => t.IsDefined(typeof(HorizonteCommand)));
                        foreach (var method in metodos)
                        {
                            if (modInstance != null)
                            {
                                AddCommandToList(method, modInstance);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _log?.LogError(e.ToString());
                    }
                }
            }

            _log?.LogInformation($"Loaded {_commandList.Count} HCommands");
        }
        catch (Exception e)
        {
            _log?.LogError(e.ToString());
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
    private void AddCommandToList(MethodInfo method, object instance)
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
                       method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
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
            if (method == null) return null;

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

                    var serob = JsonSerializer.Deserialize(jsonValue,T);

                    tmpobjlst.Add(serob);

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
        catch (Exception e)
        {
            _log?.LogError("RunCommandJson: " + e.ToString());
            return null;
        }
    }


    public async Task<string?> RunCommandJsonAsync(string commandKey, string[]? jsonarglist)
    {
        try
        {
            object?[]? inobjparams = null;
            var method = GetHCommand(commandKey);
            if (method == null) return null;

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
        catch (Exception e)
        {
            _log?.LogError("RunCommandJson: " + e.ToString());
            return null;
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
                return null;
            }

            var rCommand = _commandList[commandKeyor];

            if (rCommand.CommandAction == null) return null;

            // Revisar si el comando es una tarea asíncrona
            if (typeof(Task).IsAssignableFrom(rCommand.CommandAction.ReturnType))
            {
                // Invocamos el comando de forma asíncrona
                var task = (Task)rCommand.CommandAction.Invoke(rCommand.Instance, arg)!;

                // Esperamos si la tarea tiene resultado (Task<T>)
                await task.ConfigureAwait(true);
                task.Wait();

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
        catch (Exception ex)
        {
            _log?.LogError($"Error al ejecutar RunCommandAsync para {commandKeyor}: {ex}");
            return null;
        }
    }

    public async Task<T> RunCommandAsync<T>(string commandKeyor, object[]? arg = null)
    {
        try
        {
            commandKeyor = commandKeyor.Trim();
    
            // Recuperamos el comando del diccionario a partir de su clave
            if (!_commandList.ContainsKey(commandKeyor))
            {
                _log?.LogError($"Comando {commandKeyor} no encontrado.");
                return default!;
            }
    
            var rCommand = _commandList[commandKeyor];
    
            if (rCommand.CommandAction == null) return default!;
    
            // Si el comando es async
            //if (typeof(Task).IsAssignableFrom(rCommand.CommandAction.ReturnType))
            if (rCommand.IsAsync)
            {
                var task = (Task)rCommand.CommandAction.Invoke(rCommand.Instance, arg)!;
    
                await task.ConfigureAwait(true);
    
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
            return default!;
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
                return default(T);
            }

            var rCommand = (HCommand)_commandList[commandKeyor];
            // invocamos el método de la clase Panel con los argumentos
            if (rCommand.CommandAction == null) return default(T);
            var resObject = rCommand.CommandAction.Invoke(rCommand.Instance, arg);
            if (typeof(T) != typeof(object))
                resObject = Convert.ChangeType(resObject, typeof(T));
            //devolvemos el objeto
            return (T)resObject!;
        }
        catch (Exception ex)
        {
            _log?.LogError($"**** {commandKeyor} ****" + ex.ToString());
            return default(T);
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
}