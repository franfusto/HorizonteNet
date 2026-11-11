using System.Security.Claims;

namespace Horizonte.Ai.MCPServer.Services;

/// <summary>
/// Define el contrato para la validación asíncrona de claves de API (API Keys) en las solicitudes del servidor MCP.
/// </summary>
public interface IApiKeyValidator
{
    /// <summary>
    /// Valida de forma asíncrona una clave de API dada y devuelve el <see cref="ClaimsPrincipal"/> asociado si es válida.
    /// </summary>
    /// <param name="apiKey">La clave de API a validar.</param>
    /// <param name="cancellationToken">Token de cancelación para abortar la operación.</param>
    /// <returns>Una tarea que devuelve el <see cref="ClaimsPrincipal"/> autenticado, o <c>null</c> si la clave no es válida.</returns>
    Task<ClaimsPrincipal?> ValidateAsync(
        string apiKey,
        CancellationToken cancellationToken);
}