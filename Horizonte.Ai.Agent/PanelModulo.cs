using Horizonte.Ai.Agent.Widgets;
using Horizonte;
using Microsoft.Extensions.Logging;

namespace Horizonte.Ai.Agent;

[HorizonteModule("Horizonte.Ai.Agent")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private AgentsDen? _agentsDen;
    private IServiceProvider _serviceProvider;

    public PanelModulo(ILogger<PanelModulo> logger)
    {
        _logger = logger;
    }

    [HorizonteRole("init")]
    [HorizonteCommand("AgentH_Init", "Agente")]
    public bool Init()
    {
        _logger.LogInformation("Horizonte.Ai.Agent Iniciado");
        _agentsDen = new AgentsDen(_serviceProvider);
        return true;
    }

    [HorizonteRole("widget")]
    [HorizonteCommand("AgentH_Agent", description: "AgenteH")]
    public WidgetDef Agent() => new WidgetDef() { Type = typeof(Chat), Parameters = null };

    [HorizonteRole("configpage")]
    [HorizonteCommand("AgentH_ConfigPage", "Configuración del AgenteH")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(AgentConfigWidget), Parameters = null };

    [HorizonteCommand("AgentH_GetCommandDefinition",
        "Recupera en una cadena de texto con formato JSON los comandos disponibles en el entorno actual.")]
    public string? GetCommandDefinition() => _agentsDen?.GetCommandDefinition();

    
    [HorizonteCommand("AgentH_GetEntityDefinition","Recupera en una cadenda de texto con formato JSON la entidad especificada, con los valores predeterminados.Si no es posible recuperar la entidad, se retorna null.")]
    public string? GetEntityDefinition(string entityName) => _agentsDen?.GetEntityDefinition(entityName);
    
    
    [HorizonteCommand("AgentH_GetAgents",
        "Devuelve una lista con los nombres de los agentes presentes en el pool.")]
    public List<string>? GetAgents()
    {
        return _agentsDen?.GetAgentsNames();
    }

    [HorizonteCommand("AgentH_AddAgent",
        "Añade un nuevo agente al pool.")]
    public bool? AddAgent(string agentName, AgentConfig? config)
    {
        if (_agentsDen == null) return null;
        if (config == null) config = new AgentConfig();
        return _agentsDen?.AddAgent(agentName, config);
    }

    [HorizonteCommand("AgentH_RemoveAgent",
        "Elimina un agente del pool según su nombre.")]
    public bool? RemoveAgent(string agentName)
    {
        if (_agentsDen == null) return null;
        _agentsDen.RemoveAgent(agentName);
        return true; // Se retorna true al remover el agente sin errores
    }

    [HorizonteCommand("AgentH_GetAgentStatics",
        "Obtiene una lista de estadísticas para un agente específico. Con información sobre la cantidad de tokens realizados y funciones utilizadas.")]
    public Dictionary<string, string>? GetAgentStatics(string agentName)
    {
        return _agentsDen?.GetStatics(agentName);
    }

    [HorizonteCommand("AgentH_QueryAgent",
        "Realiza una consulta a un agente específico.")]
    public object? QueryAgent(string agentName, string query)
    {
        return _agentsDen?.Query(agentName, query);
    }

    [HorizonteCommand("AgentH_GetHistory",
        "Obtiene el historial de consultas realizadas a un agente.")]
    public object? GetHistory(string agentName)
    {
        return _agentsDen?.GetHistory(agentName);
    }

    [HorizonteCommand("AgentH_ClearHistory",
        "Limpia el historial de un agente específico.")]
    public bool? ClearHistory(string agentName)
    {
        if (_agentsDen == null) return null;
        _agentsDen.ClearHistory(agentName);
        return true; // Se retorna true al limpiar el historial sin errores
    }
}
    
    
