using System.ComponentModel;
using Horizonte.Ai.Agent;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace Horizonte.Extension.AiWorkFlows.Tools;

public class HCommandToolSet
{
    private CommandVectorManager _commandVectorManager;
    private IHGesCom _gesCom;
    private ILogger _logger;
    public HCommandToolSet(IHorizonteEnv env, OpenAIClient embedderCliente, string emmbedderModel)
    {
        _gesCom = env.GetService<IHGesCom>();
        _logger = env.GetService<ILogger<HCommandToolSet>>();
        _commandVectorManager = new CommandVectorManager(env, embedderCliente, emmbedderModel);
        _commandVectorManager.InitializeManagerAsync();
        
    }


    public AITool RunCommand()
    {
        return AIFunctionFactory.Create(
            RunCommandAction,
            name: "RunCommand",
            description: "Ejecuta un comando en el sistema y devuelve su resultado"
        );
    }

    public AITool QueryVectorStore()
    {
        return AIFunctionFactory.Create(
            QueryVerctorStoreAction,
            name: "QueryVectorStore",
            description: "Consulta los comandos disponibles en el sistema y devuelve los resultados"
        );
    }

    public AITool GetSystemTime()
    {
        return AIFunctionFactory.Create(
            () => { return DateTime.Now.ToString("O"); },
            name: "GetSystemTime",
            description: "Devuelve la hora actual del sistema en formato ISO 8601"
        );
    }

    private async Task<object?> RunCommandAction(
        [Description("Nombre del comando")] string commandName,
        [Description("Parámetros necesarios para ejecutar el comando")]
        string[]? args)
    {
        try
        {
            // Registrar los parámetros de entrada
            _logger.LogInformation(
                "Ejecutando RunCommand con los siguientes parámetros: CommandName = {CommandName}, Args = {Args}",
                commandName, args != null ? string.Join(", ", args) : "null");

            //
            if(!_gesCom.ExistCommand(commandName)) throw new ArgumentException($"Nombre del comando: {commandName} >> not found");
            // Ejecutar la acción principal
            object? result = null;
            if (_gesCom.IsAsyncCommand(commandName))
            {
                //todo: gestionar el token de cancelación
                result = await _gesCom.RunCommandJsonAsync(commandName,  args);
            }
            else
            {
                result = _gesCom.RunCommandJson(commandName, args);

            }

            // Registrar el resultado
            _logger.LogTrace("Resultado de RunCommand: {Result}", result ?? "null");

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return null;
        }
    }

    private Task<string?> QueryVerctorStoreAction(
        [Description("Consulta a realizar en el vector store")]
        string query)
    {
        return _commandVectorManager.SearchCommandInVectorStoreAsync(query, CancellationToken.None);
    }
}
