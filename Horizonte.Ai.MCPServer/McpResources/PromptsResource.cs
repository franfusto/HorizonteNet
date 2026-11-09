namespace Horizonte.Ai.MCPServer.McpResources;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerPromptType]
public class PromptsResource
{
    [McpServerPrompt, Description("Analiza una incidencia")]
    public static string AnalizarIncidencia(
        [Description("Descripción de la incidencia")]
        string incidencia)
    {
        return $"Analiza la causa y propone una solución: {incidencia}";
    }
}

[McpServerToolType]
public class Tools
{
    [McpServerTool, Description("Consulta el estado de un pedido")]
    public static string ConsultarPedido(int id)
    {
        // Consultar la base de datos o un servicio.
        return $"Estado del pedido {id}: En preparación";
    }
}