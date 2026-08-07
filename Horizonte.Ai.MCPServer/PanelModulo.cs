using Horizonte;
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
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(ConfigMCPServerWidget), Parameters = null };

}