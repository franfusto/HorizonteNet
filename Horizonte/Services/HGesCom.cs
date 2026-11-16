using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using log4net;
using Horizonte.Interfaces;


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
    /// <param name="CommandName">Nombre o clave del comando.</param>
    /// <param name="Description">Descripción de la funcionalidad del comando.</param>
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


    private readonly Lazy<IEnvCoordinator> _envCoordinator;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="HGesCom"/>.
    /// </summary>
    /// <param name="serviceProvider">Proveedor de servicios para inyección de dependencias.</param>
    public HGesCom(IServiceProvider serviceProvider)
    {
        _envCoordinator = new Lazy<IEnvCoordinator>(() => serviceProvider.GetRequiredService<IEnvCoordinator>());
    }

    /// <inheritdoc/>
    public void LoadModules()
    {
        try
        {
            var envCoordinator = _envCoordinator.Value;
            if (envCoordinator == null)
            {
                Log.Error("IEnvCoordinator not found. Cannot load modules.");
                return;
            }

            // Limpiamos comandos previos si los hubiera (útil para re-carga)
            _commandList.Clear();

            var assembliesByDomain = envCoordinator.AssembliesByDomain;
            foreach (var domainName in assembliesByDomain.Keys)
            {
                envCoordinator.LoadCommandsByDomain(domainName);
            }

            Log.Info($"Loaded {_commandList.Count} HCommands");
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
    }

    /// <inheritdoc/>
    public void InitzializeModules()
    {
        foreach (var item in GetRoleCommands("init"))
        {
            Log.Info("Ejecutando Init: " + item.CommandName);
            RunCommand(item.CommandName);
        }

        Log.Info("Módulos iniciados");
    }

    /// <inheritdoc/>
    public List<RoleListItem> GetRoleCommands(string role)
    {
        return _commandList
            .Where(x => x.Value.Roles.Contains(role))
            .Select(item => new RoleListItem(item.Value.CommandKey, item.Value.Description))
            .ToList();
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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


    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public bool IsAsyncCommand(string commandKey)
    {
        var method = GetHCommand(commandKey);
        return method is { IsAsync: true };
    }

    /// <inheritdoc/>
    public bool ExistCommand(string commandKey)
    {
        return _commandList.ContainsKey(commandKey);
    }

    /// <inheritdoc/>
    public async Task<object?> RunCommandAsync(string commandKeyor, object[]? arg = null)
    {
        return await RunCommandAsync<object?>(commandKeyor, arg).ConfigureAwait(false);
    }


    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public object? RunCommand(string commandKeyor, object[]? arg = null)
    {
        return RunCommandAsync(commandKeyor, arg).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public T? RunCommand<T>(string commandKeyor, object[]? arg = null)
    {
        return RunCommandAsync<T>(commandKeyor, arg).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public HCommand? GetHCommand(string commandKey)
    {
        _commandList.TryGetValue(commandKey, out HCommand? rCommand);
        return rCommand;
    }

    /// <inheritdoc/>
    public IEnumerable<HCommand> GetCommandList()
    {
        return _commandList.Values.ToList();
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void LoadCommandsByDomain(string domainName)
    {
        try
        {
            var envCoordinator = _envCoordinator.Value;

            if (envCoordinator == null)
            {
                Log.Error("No se puede cargar comandos por dominio: IEnvCoordinator no disponible.");
                return;
            }

            envCoordinator.LoadCommandsByDomain(domainName);
        }
        catch (Exception e)
        {
            Log.Error($"Error en LoadCommandsByDomain para {domainName}: {e.Message}");
        }
    }
}