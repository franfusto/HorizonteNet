using System.Security.Claims;

namespace Horizonte.Ai.MCPServer.Services;

public interface IApiKeyValidator
{
    Task<ClaimsPrincipal?> ValidateAsync(
        string apiKey,
        CancellationToken cancellationToken);
}