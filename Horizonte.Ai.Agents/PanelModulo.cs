using Horizonte;
using Horizonte.Ai.Agents.Widgets;
using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;

namespace Horizonte.Ai.Agents;

/// <summary>
/// Proporciona la integración y los comandos del módulo Horizonte.Ai.Agents en la plataforma Horizonte.
/// </summary>
[HorizonteModule("Horizonte.Ai.Agents")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PanelModulo"/>.
    /// </summary>
    /// <param name="logger">Servicio de registro de eventos para el módulo.</param>
    /// <param name="context">Contexto global de ejecución y almacenamiento de Horizonte.</param>
    public PanelModulo(ILogger<PanelModulo> logger, IhContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Inicializa los servicios y el estado del módulo Horizonte.Ai.Agents.
    /// </summary>
    /// <returns><c>true</c> si la inicialización se completó satisfactoriamente.</returns>
    [HorizonteRole("init")]
    [HorizonteCommand("Horizonte.Ai.Agents_Init")]
    public bool Init()
    {
        _logger.LogInformation("Módulo Horizonte.Ai.Agents Iniciado");
        return true;
    }

    /// <summary>
    /// Obtiene la definición del widget de la página de configuración para la gestión de agentes, herramientas, clientes, servidores y RAG.
    /// </summary>
    /// <returns>Una instancia de <see cref="WidgetDef"/> configurada con <see cref="AgentsConfigWidget"/>.</returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("Horizonte.Ai.Agents_ConfigPage", "Widget de configuración del módulo Horizonte.Ai.Agents")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(AgentsConfigWidget), Parameters = null };

    /// <summary>
    /// Obtiene la definición del widget de interfaz de chat para interactuar con agentes inteligentes.
    /// </summary>
    /// <returns>Una instancia de <see cref="WidgetDef"/> configurada con <see cref="ChatWidget"/>.</returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("Horizonte.Ai.Agents_ChatWidget")]
    public WidgetDef ChatWidget() => new WidgetDef() { Type = typeof(ChatWidget), Parameters = null };
}