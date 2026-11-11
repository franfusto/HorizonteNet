namespace Horizonte.Ai.MCPServer.McpResources;
using ModelContextProtocol.Server;
using System.ComponentModel;

/// <summary>
/// Provee plantillas de prompts MCP para el servidor.
/// </summary>
[McpServerPromptType]
public class PromptsResource
{
    /// <summary>
    /// Genera un prompt estructurado para analizar una incidencia y proponer soluciones.
    /// </summary>
    /// <param name="incidencia">Descripción detallada de la incidencia a analizar.</param>
    /// <returns>Texto estructurado con la instrucción para el modelo de lenguaje.</returns>
    [McpServerPrompt, Description("Analiza una incidencia")]
    public static string AnalizarIncidencia(
        [Description("Descripción de la incidencia")]
        string incidencia)
    {
        return $"Analiza la causa y propone una solución: {incidencia}";
    }
}

/// <summary>
/// Provee herramientas adicionales expuestas a través del protocolo MCP.
/// </summary>
[McpServerToolType]
public class Tools
{
    /// <summary>
    /// Consulta el estado actual de preparación o entrega de un pedido.
    /// </summary>
    /// <param name="id">Identificador único del pedido a consultar.</param>
    /// <returns>Descripción textual del estado del pedido.</returns>
    [McpServerTool, Description("Consulta el estado de un pedido")]
    public static string ConsultarPedido(int id)
    {
        // Consultar la base de datos o un servicio.
        return $"Estado del pedido {id}: En preparación";
    }
}