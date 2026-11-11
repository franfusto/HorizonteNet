namespace Horizonte.Ai.MCPServer.McpResources;

using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

/// <summary>
/// Recurso MCP que expone información sobre el estado general de la aplicación.
/// </summary>
[McpServerResourceType]
public class AppStateResource
{
    /// <summary>
    /// Devuelve el estado operativo y versión actual de la aplicación en formato JSON.
    /// </summary>
    /// <returns>Una cadena en formato JSON con la información del estado de la aplicación.</returns>
    [McpServerResource(
        UriTemplate = "app://estado",
        Name = "Estado de la aplicación",
        MimeType = "application/json")]
    [Description("Devuelve el estado actual de la aplicación")]
    public static string GetEstado()
    {
        var estado = new
        {
            nombre = "MiAplicacion",
            version = "1.0.0",
            estado = "Operativa",
            fecha = DateTimeOffset.UtcNow
        };

        return JsonSerializer.Serialize(estado);
    }
}