namespace Horizonte.Extension.Ai.Contracts;

/// <summary>
/// Representa una consulta de chat enviada a un agente de IA, incluyendo texto y posibles archivos adjuntos.
/// </summary>
public class ChatQuery
{
    /// <summary>
    /// Obtiene o establece el texto de la consulta o mensaje del usuario.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece los archivos codificados o rutas de archivo adjuntas a la consulta.
    /// </summary>
    public string[]? Files { get; set; } = null;
}