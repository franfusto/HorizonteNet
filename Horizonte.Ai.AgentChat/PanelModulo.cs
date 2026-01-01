using Horizonte;
using Microsoft.Extensions.Logging;

namespace Horizonte.Ai.AgentChat;

[HorizonteModule("Horizonte.Ai.AgentChat")]
public class PanelModulo
{
    private Lazy<IHorizonteEnv> _env;
    private ILogger<PanelModulo>? _logger;
    public PanelModulo(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
    }
    [HorizonteRole("init")]
    [HorizonteCommand("Horizonte.Ai.AgentChat_Init")]
    public bool Init()
    {
        _logger = _env.Value.GetService<ILogger<PanelModulo>>();
        var context = _env.Value.GetService<IHContext>();
        _logger?.LogInformation("Módulo Horizonte.Ai.AgentChat Iniciciado");
        return true;
    }
    [HorizonteRole("configpage")]
    [HorizonteCommand("Horizonte.Ai.AgentChat_ConfigPage", "Widget de configuración del módulo Horizonte.Ai.AgentChat")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(AgentChatConfigWidget), Parameters = null };
    
    [HorizonteRole("widget")]
    [HorizonteCommand("Horizonte.Ai.AgentChat_ChatWidget")]
    public WidgetDef ChatWidget() => new WidgetDef() { Type = typeof(ChatWidget), Parameters = null };
    

}