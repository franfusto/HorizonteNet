using System.Security.Claims;
using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;

namespace Horizonte.Ai.MCPServer.Services;

public sealed class ApiKeyValidator : IApiKeyValidator
{
    private readonly IHGesCom _gesCom;
    private readonly ILogger<ApiKeyValidator> logger;
    private readonly IhContext _context;
    private readonly string? _command;

    public ApiKeyValidator(ILogger<ApiKeyValidator> logger,IHGesCom gesCom, IhContext context)
    {
        this.logger = logger;
        _context = context;
        _gesCom = gesCom;
        _command = _context.Get<MCPServerConfig>()?.BuiltInAuthCommand;
    }
    
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