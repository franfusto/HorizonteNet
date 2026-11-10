using System.Security.Claims;
using Horizonte.Ai.MCPServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Horizonte.Ai.MCPServer.Middleware;
using Microsoft.Extensions.Primitives;

public sealed class ValidatorMiddleware
{
    private readonly RequestDelegate _next;

    public ValidatorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

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