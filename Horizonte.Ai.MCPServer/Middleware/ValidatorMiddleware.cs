using System.Security.Claims;
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
        loger.LogInformation("Validando API key");
        loger.LogInformation(typeof(IApiKeyValidator).AssemblyQualifiedName);
        loger.LogInformation(typeof(ApiKeyValidator).AssemblyQualifiedName);
        if (!context.Request.Headers.TryGetValue(
                "X-API-Key", out StringValues values))
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(
                "API key requerida.");
            return;
        }

        var apiKey = values.ToString();

        if (string.IsNullOrWhiteSpace(apiKey) ||
            !await validator.ValidateAsync(
                apiKey, context.RequestAborted))
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(
                "API key inválida.");
            return;
        }
        // claims

        var claims = new List<Claim>
        {
            new("client_id", "clienteid"),
            new(ClaimTypes.Name, "claimtypes.name")
        };

        //claims.AddRange(client.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(claims, authenticationType: "ApiKey"));
        await _next(context);
    }
    
    
    
}