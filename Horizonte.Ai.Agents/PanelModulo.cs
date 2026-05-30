using Horizonte;
using Horizonte.Ai.Agents.Widgets;
using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;

namespace Horizonte.Ai.Agents;

[HorizonteModule("Horizonte.Ai.Agents")]
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
    [HorizonteCommand("Horizonte.Ai.Agents_Init")]
    public bool Init()
    {
        _logger.LogInformation("Módulo Horizonte.Ai.Agents Iniciado");
        return true;
    }
    
    [HorizonteRole("widget")]
    [HorizonteCommand("Horizonte.Ai.Agents_ConfigPage", "Widget de configuración del módulo Horizonte.Ai.Agents")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(AgentsConfigWidget), Parameters = null };

    [HorizonteRole("widget")]
    [HorizonteCommand("Horizonte.Ai.Agents_ChatWidget")]
    public WidgetDef ChatWidget() => new WidgetDef() { Type = typeof(ChatWidget), Parameters = null };


}