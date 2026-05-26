using Horizonte;
using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;

namespace Horizonte.Ai.AgentChat;

[HorizonteModule("Horizonte.Ai.AgentChat")]
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
    [HorizonteCommand("Horizonte.Ai.AgentChat_Init")]
    public bool Init()
    {
        _logger.LogInformation("Módulo Horizonte.Ai.AgentChat Iniciado");
        return true;
    }
    [HorizonteRole("configpage")]
    [HorizonteCommand("Horizonte.Ai.AgentChat_ConfigPage", "Widget de configuración del módulo Horizonte.Ai.AgentChat")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(AgentChatConfigWidget), Parameters = null };
    
    [HorizonteRole("widget")]
    [HorizonteCommand("Horizonte.Ai.AgentChat_ChatWidget")]
    public WidgetDef ChatWidget() => new WidgetDef() { Type = typeof(ChatWidget), Parameters = null };
    

}