namespace Horizonte.Ai.MCPServer;

public sealed class ApiKeyValidator : IApiKeyValidator
{
    public Task<bool> ValidateAsync(
        string apiKey,
        CancellationToken cancellationToken)
    {
        // Sustituir por tu lógica real de validación.
        bool valid = ValidateAgainstMySystem(apiKey);

        return Task.FromResult(valid);
    }

    private static bool ValidateAgainstMySystem(string apiKey)
    {
        // Ejemplo: invocar tu servicio de autenticación.
        return apiKey.StartsWith("demo_", StringComparison.Ordinal);
    }
}