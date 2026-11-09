namespace Horizonte.Ai.MCPServer.McpResources;



using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

[McpServerResourceType]
public class AppStateResource
{
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