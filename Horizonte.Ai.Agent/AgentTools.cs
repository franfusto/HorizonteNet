using System.ComponentModel;
using Horizonte;
using Microsoft.SemanticKernel;

namespace Horizonte.Ai.Agent;

public class AgentTools
{
    
    private IHGesCom _gesCom;
    private ILogger _logger;
    private CommandVectorManager _commandVectorManager;
    public AgentTools(ILogger logger, IHGesCom gesCom,CommandVectorManager commandVectorManager)
    {
        _logger = logger;
        _gesCom = gesCom;
        _commandVectorManager = commandVectorManager;
    }
    
     
    ////
     /// <summary>
    /// Ejecuta un comando utilizando el nombre del comando y los argumentos proporcionados a través del sistema HGesCom.
    /// Registra el proceso de ejecución, el resultado o cualquier error encontrado.
    /// </summary>
    /// <param name="commandName">
    /// El nombre del comando a ejecutar. Esta cadena identifica el comando dentro del sistema HGesCom.
    /// </param>
    /// <param name="args">
    /// Una matriz opcional de argumentos necesarios para la ejecución del comando.
    /// Estos argumentos proporcionan información o parámetros adicionales para el comando.
    /// </param>
    /// <returns>
    /// El resultado de la ejecución del comando tal como lo devuelve el sistema HGesCom.
    /// Si ocurre un error durante la ejecución, se devuelve null.
    /// </returns>
    [KernelFunction("RunCommand")]
    public async Task<object?> RunCommand([Description("Nombre del comando")] string commandName,
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
                result = await _gesCom.RunCommandJsonAsync(commandName, CancellationToken.None, args);
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


    [KernelFunction("QueryVerctorStore")]
    [Description("Consulta la base de datos vectorial")]
    public  Task<string?> QueryVerctorStore([Description("Nombre del catálogo")]string catalog, [Description("Consulta")] string query)
    {
        _logger.LogInformation($"Consulta la base de datos vectorial: {catalog} =>  {query}");
        return _commandVectorManager.SearchCommandInVectorStoreAsync(catalog, query, CancellationToken.None);
    }


}