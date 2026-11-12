using Horizonte;
using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;

namespace Horizonte.Ai.AgentChat;

/// <summary>
/// Módulo principal para la integración de Horizonte.Ai.AgentChat en la plataforma Horizonte.
/// </summary>
[HorizonteModule("Horizonte.Ai.AgentChat")]
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
    /// Inicializa los servicios y el estado del módulo Horizonte.Ai.AgentChat.
    /// </summary>
    /// <returns><c>true</c> si la inicialización se completó con éxito.</returns>
    [HorizonteRole("init")]
    [HorizonteCommand("Horizonte.Ai.AgentChat_Init")]
    public bool Init()
    {
        _logger.LogInformation("Módulo Horizonte.Ai.AgentChat Iniciado");
        return true;
    }

    /// <summary>
    /// Obtiene la definición del widget de la página de configuración para los parámetros del chat de agente.
    /// </summary>
    /// <returns>Una instancia de <see cref="WidgetDef"/> configurada con <see cref="AgentChatConfigWidget"/>.</returns>
    [HorizonteRole("configpage")]
    [HorizonteCommand("Horizonte.Ai.AgentChat_ConfigPage", "Widget de configuración del módulo Horizonte.Ai.AgentChat")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(AgentChatConfigWidget), Parameters = null };

    /// <summary>
    /// Obtiene la definición del widget de interfaz de chat para interactuar en tiempo real con el agente.
    /// </summary>
    /// <returns>Una instancia de <see cref="WidgetDef"/> configurada con <see cref="ChatWidget"/>.</returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("Horizonte.Ai.AgentChat_ChatWidget")]
    public WidgetDef ChatWidget() => new WidgetDef() { Type = typeof(ChatWidget), Parameters = null };
}