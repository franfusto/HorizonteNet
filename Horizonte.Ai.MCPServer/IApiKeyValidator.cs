namespace Horizonte.Ai.MCPServer;

public interface IApiKeyValidator
{
    Task<bool> ValidateAsync(
        string apiKey,
        CancellationToken cancellationToken);
}