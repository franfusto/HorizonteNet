using System.Security.Claims;
using Horizonte;
using Horizonte.Ai.MCPServer.Widgets;
using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;

namespace Horizonte.Ai.MCPServer;

/// <summary>
/// Panel de control y punto de entrada para el módulo Horizonte.Ai.MCPServer.
/// </summary>
/// <remarks>
/// Gestiona la inicialización del servidor MCP, el suministro de la página de configuración y
/// la validación de API Keys mediante comandos registrados en Horizonte.
/// </remarks>
[HorizonteModule("Horizonte.Ai.MCPServer")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PanelModulo"/>.
    /// </summary>
    /// <param name="logger">El servicio de registro de eventos para el módulo.</param>
    /// <param name="context">El contexto principal de Horizonte para acceso a estado y configuración.</param>
    public PanelModulo(ILogger<PanelModulo> logger, IhContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Inicializa el módulo Horizonte.Ai.MCPServer.
    /// </summary>
    /// <returns>Devuelve <c>true</c> si el módulo se inicializó correctamente.</returns>
    [HorizonteRole("init")]
    [HorizonteCommand("MCPServer_Init")]
    public bool Init()
    {
        _logger.LogInformation("Módulo MCPServer Iniciado");
        return true;
    }

    /// <summary>
    /// Obtiene la definición del widget de la página de configuración del servidor MCP.
    /// </summary>
    /// <returns>Una instancia de <see cref="WidgetDef"/> configurada con el componente <see cref="ConfigWidget"/>.</returns>
    [HorizonteRole("configpage")]
    [HorizonteCommand("MCPServer_ConfigPage", "Widget de configuración del módulo MCPServer")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(ConfigWidget), Parameters = null };
    
    /// <summary>
    /// Valida una clave de API proporcionada y devuelve un objeto <see cref="ClaimsPrincipal"/> asociado con los claims correspondientes.
    /// </summary>
    /// <param name="apiKey">La clave de API recibida.</param>
    /// <returns>El <see cref="ClaimsPrincipal"/> autenticado con los claims de la API Key.</returns>
    [HorizonteCommand("MCPServer_GetClaimFromApiKey", "Built-in Fake validator")]
    public ClaimsPrincipal? GetClaimFromApiKey(string apiKey)
    {
        var claims = new List<Claim>
        {
            new("api-key", apiKey)
        };
        var result =  new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "ApiKey"));
        return result;
    }

}