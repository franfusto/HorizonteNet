using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Horizonte.Ai.MCPServer;

public static class Extensions
{
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