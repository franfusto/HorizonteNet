using System.Security.Claims;
using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;

namespace Horizonte.Ai.MCPServer.Services;

/// <summary>
/// Implementación del validador de claves de API que delega la comprobación a comandos de Horizonte.
/// </summary>
public sealed class ApiKeyValidator : IApiKeyValidator
{
    private readonly IHGesCom _gesCom;
    private readonly ILogger<ApiKeyValidator> logger;
    private readonly IhContext _context;
    private readonly string? _command;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ApiKeyValidator"/>.
    /// </summary>
    /// <param name="logger">El servicio de registro de eventos.</param>
    /// <param name="gesCom">El gestor de comandos de Horizonte para invocar el comando de autenticación configurado.</param>
    /// <param name="context">El contexto de la aplicación para obtener la configuración del servidor MCP.</param>
    public ApiKeyValidator(ILogger<ApiKeyValidator> logger, IHGesCom gesCom, IhContext context)
    {
        this.logger = logger;
        _context = context;
        _gesCom = gesCom;
        _command = _context.Get<MCPServerConfig>()?.BuiltInAuthCommand;
    }
    
    /// <summary>
    /// Valida de forma asíncrona una clave de API delegando en el comando de autenticación configurado en <see cref="MCPServerConfig"/>.
    /// </summary>
    /// <param name="apiKey">La clave de API a validar.</param>
    /// <param name="cancellationToken">Token de cancelación para abortar la operación.</param>
    /// <returns>El <see cref="ClaimsPrincipal"/> resultante de la validación, o <c>null</c> si no es válida o no está configurada.</returns>
    public Task<ClaimsPrincipal?> ValidateAsync(
        string apiKey,
        CancellationToken cancellationToken)
    {
        try
        {
            if (_command == null) throw new Exception("No se ha configurado el comando de validación");
            var isasync = _gesCom.IsAsyncCommand(_command);
            if (isasync)
            {
                var result = _gesCom.RunCommandAsync<ClaimsPrincipal?>(_command, [apiKey]);
                return result;
            }
            else
            {
                var result = _gesCom.RunCommand<ClaimsPrincipal?>(_command, [apiKey]);
                return Task.FromResult(result);
                
            }
        }
        catch (Exception e)
        {
            logger.LogError(e,"Error al ejecutar el comando de validación");
            return null!;
        }

    }


}