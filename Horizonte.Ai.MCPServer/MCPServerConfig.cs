using Microsoft.Extensions.DependencyInjection;

namespace Horizonte.Ai.MCPServer;

/// <summary>
/// Gestiona la configuración del servidor Model Context Protocol (MCP) de Horizonte.
/// </summary>
/// <remarks>
/// Define la dirección URL base, la ruta del endpoint, los tipos de middleware de validación,
/// los comandos de Horizonte expuestos como herramientas MCP, los recursos y los prompts disponibles.
/// </remarks>
public class MCPServerConfig
{
    /// <summary>
    /// Obtiene o establece la ruta relativa para el endpoint SSE/HTTP del protocolo MCP (por defecto "mcp").
    /// </summary>
    public string Path { get; set; } = "mcp";
    
    /// <summary>
    /// Obtiene o establece la dirección URL base donde escucha el servidor MCP (por defecto "http://localhost:5005").
    /// </summary>
    public string Url { get; set; } = "http://localhost:5005";
    
    /// <summary>
    /// Obtiene o establece el nombre completo del tipo del middleware para la validación de peticiones MCP.
    /// </summary>
    public string? ValidatorMiddlewareType { get; set; } = "Horizonte.Ai.MCPServer.Middleware.ValidatorMiddleware, Horizonte.Ai.MCPServer";

    /// <summary>
    /// Obtiene o establece el nombre del comando de autenticación integrado que valida y resuelve claims a partir de la API Key.
    /// </summary>
    public string BuiltInAuthCommand { get; set; } = "MCPServer_GetClaimFromApiKey";
    
    /// <summary>
    /// Obtiene o establece la lista de nombres completos de tipos de recursos MCP registrados.
    /// </summary>
    public List<string> ResourcesTypes { get; set; } = new();
    
    /// <summary>
    /// Obtiene o establece la lista de nombres completos de tipos de prompts MCP registrados.
    /// </summary>
    public List<string> PromptsTypes { get; set; } = new();
    
    /// <summary>
    /// Obtiene o establece la lista de comandos de Horizonte expuestos como herramientas MCP (<see cref="MCPCommandDef"/>).
    /// </summary>
    public List<MCPCommandDef> Commands { get; set; } = new();    
    
    /// <summary>
    /// Obtiene o establece la lista de servicios de inyección de dependencias personalizados (<see cref="MCPServiceDef"/>) para el servidor MCP.
    /// </summary>
    public List<MCPServiceDef> Services { get; set; } = new();
    
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="MCPServerConfig"/> con comandos, recursos, prompts y servicios predeterminados.
    /// </summary>
    public MCPServerConfig()
    {
        Commands.Add(new MCPCommandDef()
        {
            Command = "Horizonte_SytstemDateTime",
            Description = "Obtiene la fecha y hora del sistema"
        });
        Commands.Add(new MCPCommandDef()
        {
            Command = "Workers_GetAvailablesBackServices",
            Description = "Obtiene la lista de trabajadores disponibles en el sistema"
        });
        Commands.Add(new MCPCommandDef()
        {
            Command = "Sample_Encriptar",
            Description = ""
        });
        Commands.Add(new MCPCommandDef()
        {
            Command = "Sample_GetMeteoInfo",
            Description = ""
        });
        Services.Add(new MCPServiceDef
        {
            Scope = ServiceLifetime.Singleton,
            InterfaceType = "Horizonte.Ai.MCPServer.Services.IApiKeyValidator, Horizonte.Ai.MCPServer",
            ImplementationType = "Horizonte.Ai.MCPServer.Services.ApiKeyValidator, Horizonte.Ai.MCPServer"
        });
        ResourcesTypes.Add("Horizonte.Ai.MCPServer.McpResources.AppStateResource, Horizonte.Ai.MCPServer");
        PromptsTypes.Add("Horizonte.Ai.MCPServer.McpResources.PromptsResource, Horizonte.Ai.MCPServer");
    }
}

/// <summary>
/// Define la configuración de un comando de Horizonte expuesto como herramienta MCP.
/// </summary>
public class MCPCommandDef
{
    /// <summary>
    /// Obtiene o establece el nombre o clave identificadora del comando de Horizonte.
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la descripción de la herramienta para los clientes MCP.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Define la configuración de un servicio que se registra en el contenedor de dependencias del servidor MCP.
/// </summary>
public class MCPServiceDef
{
    /// <summary>
    /// Obtiene o establece el ciclo de vida del servicio (<see cref="ServiceLifetime"/>).
    /// </summary>
    public ServiceLifetime Scope { get; set; } = ServiceLifetime.Singleton;

    /// <summary>
    /// Obtiene o establece el nombre calificado del tipo de interfaz del servicio (opcional).
    /// </summary>
    public string InterfaceType { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre calificado del tipo de implementación del servicio.
    /// </summary>
    public string ImplementationType { get; set; } = string.Empty;
}