using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Horizonte.Ai.MCPServer;

[McpServerToolType]
public class MCPTools
{
    [McpServerTool]
    [Description("Genera un saludo personalizado.")]
    public string GetGreeting([Description("El nombre de la persona a saludar")] string name)
    {
        return $"Hola, {name}! Bienvenido al servidor MCP de Horizonte.";
    }

    [McpServerTool]
    [Description("Suma dos números.")]
    public double AddNumbers(double a, double b)
    {
        return a + b;
    }

    [McpServerTool]
    [Description("Obtiene la hora actual del servidor.")]
    public string GetServerTime()
    {
        return DateTime.Now.ToString("F");
    }
}
