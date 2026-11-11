using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Horizonte.Ai.MCPServer;

/// <summary>
/// Métodos de extensión para configurar y registrar herramientas y comandos en el servidor MCP.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registra los comandos de Horizonte definidos en la configuración como herramientas ejecutables (Tools) en el servidor MCP.
    /// </summary>
    /// <param name="builder">El constructor de servidor MCP (<see cref="IMcpServerBuilder"/>).</param>
    /// <param name="config">La configuración del servidor MCP que contiene los comandos a exponer.</param>
    /// <param name="gesCom">El gestor de comandos de Horizonte para resolver las acciones de los comandos.</param>
    /// <param name="log">El servicio de registro de eventos para advertencias y trazabilidad.</param>
    /// <param name="serializerOptions">Opciones opcionales de serialización JSON para las herramientas registradas.</param>
    /// <returns>El constructor <see cref="IMcpServerBuilder"/> con las herramientas registradas.</returns>
    public static IMcpServerBuilder WithCommands(
        this IMcpServerBuilder builder,
        MCPServerConfig config,
        IHGesCom gesCom,
        ILogger log,
        JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(gesCom);

        var tools = new List<McpServerTool>();

        foreach (var command in config.Commands)
        {
            var hCommand = gesCom.GetHCommand(command.Command);
            if (hCommand is null)
            {
                log?.LogWarning($"Command {command.Command} not found");
                continue;
            }

            if (hCommand.CommandAction is null)
            {
                log?.LogWarning($"Command {command.Command} has no associated CommandAction");
                continue;
            }

            var description = !string.IsNullOrWhiteSpace(command.Description)
                ? command.Description
                : hCommand.Description;

            var options = new McpServerToolCreateOptions
            {
                Name = command.Command,
                Description = description,
                SerializerOptions = serializerOptions
            };

            var tool = McpServerTool.Create(hCommand.CommandAction, hCommand.Instance, options);
            tools.Add(tool);
        }

        if (tools.Count > 0)
        {
            builder.WithTools(tools);
        }

        return builder;
    }
}