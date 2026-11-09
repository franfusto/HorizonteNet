using System.Security.Claims;
using Horizonte;
using Horizonte.Ai.MCPServer.Widgets;
using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;

namespace Horizonte.Ai.MCPServer;

[HorizonteModule("Horizonte.Ai.MCPServer")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;


    public PanelModulo(ILogger<PanelModulo> logger, IhContext context)
    {
        _logger = logger;
        _context = context;
    }
    [HorizonteRole("init")]
    [HorizonteCommand("MCPServer_Init")]
    public bool Init()
    {
        _logger.LogInformation("Módulo MCPServer Iniciado");
        return true;
    }
    [HorizonteRole("configpage")]
    [HorizonteCommand("MCPServer_ConfigPage", "Widget de configuración del módulo MCPServer")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(ConfigWidget), Parameters = null };

    /*
    [HorizonteCommand("Sample_Encriptar","Encripta la cadena de texto introducida")]
    public string HazAlgo(string args, ClaimsPrincipal? user)
    {
        string userjson = user.FindFirst("clienteid")?.Value ?? "sin datos";
        Console.WriteLine("===>" + userjson);
        Console.WriteLine(args+"-----");
        return "[" + args + "]";
    }
    */
}