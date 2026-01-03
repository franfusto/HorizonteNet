using Horizonte.Ai.Widgets;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
//using OpenAI;
using OpenAI.Chat;

namespace Horizonte.Ai;

[HorizonteModule("Horizonte.Ai")]
public class PanelModulo
{
    private Lazy<IHorizonteEnv> _env;
    private ILogger<PanelModulo>? _logger;
  //  private IHCredManager? _credManager;
    private AgentService _agentService;
    
    public PanelModulo(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
    }
    
    [HorizonteRole("init")]
    [HorizonteCommand("Horizonte.Ai_Init")]
    public bool Init()
    {
        _logger = _env.Value.GetService<ILogger<PanelModulo>>();
//        _credManager = _env.Value.GetService<IHCredManager>();
//        var context = _env.Value.GetService<IHContext>();
        _agentService = new AgentService(_env.Value);
        return true;
    }

    
    [HorizonteRole("configpage")]
    [HorizonteCommand("Horizonte.Ai_ConfigPage", "Widget de configuración del módulo Horizonte.Ai")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(ConfigWidget), Parameters = null };

    [HorizonteCommand("Horizonte.Ai_GetAgent")]
    public AIAgent GetAgent(string agentid)
    {
        return _agentService.BuildAgent(agentid);
    }
    

    
    

}