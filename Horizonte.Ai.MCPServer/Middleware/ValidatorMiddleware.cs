using System.Security.Claims;
using Horizonte.Ai.MCPServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Horizonte.Ai.MCPServer.Middleware;
using Microsoft.Extensions.Primitives;

/// <summary>
/// Middleware de ASP.NET Core que valida la autenticación mediante API Key en las peticiones del servidor MCP.
/// </summary>
public sealed class ValidatorMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ValidatorMiddleware"/>.
    /// </summary>
    /// <param name="next">El siguiente delegado en el pipeline de peticiones HTTP.</param>
    public ValidatorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Procesa la petición HTTP entrante validando la cabecera X-API-Key mediante <see cref="IApiKeyValidator"/>.
    /// </summary>
    /// <param name="context">El contexto HTTP de la petición actual.</param>
    /// <param name="validator">El validador de claves de API inyectado.</param>
    /// <param name="loger">El servicio de registro de eventos.</param>
    /// <returns>Una tarea que representa la operación asíncrona de procesamiento de la petición.</returns>
    public async Task InvokeAsync(
        HttpContext context,
        IApiKeyValidator validator, ILogger<ValidatorMiddleware> loger)
    {
        try
        {
            loger.LogInformation("Validando API key");
            loger.LogInformation(typeof(IApiKeyValidator).AssemblyQualifiedName);
            loger.LogInformation(typeof(ApiKeyValidator).AssemblyQualifiedName);
            if (!context.Request.Headers.TryGetValue(
                    "X-API-Key", out StringValues values) || string.IsNullOrWhiteSpace(values.ToString())  )
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync(
                    "API key requerida.");
                return;
            }

            var apiKey = values.ToString();
            var claim = await validator.ValidateAsync(apiKey, context.RequestAborted);
            if (claim == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("API key inválida.");
                return;
            }

            context.User = claim;
        }
        catch (Exception e)
        {
            loger.LogError(e, "Error interno en el middleware de validación");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Error interno");
            return;
        }
       
        
        await _next(context);
    }
    
    
    
}