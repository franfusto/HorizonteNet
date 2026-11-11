using Horizonte.Extension.Ai.Definitions;

namespace Horizonte.Extension.Ai.Runtime.Chat;

/// <summary>
/// Describe una herramienta y su esquema de parámetros para que el modelo de lenguaje pueda invocarla.
/// </summary>
public sealed class HagentToolDescriptor
{
    /// <summary>
    /// Obtiene el identificador único de la herramienta.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene el nombre de la herramienta.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene el tipo funcional de la herramienta (<see cref="AgentToolType"/>).
    /// </summary>
    public AgentToolType ToolType { get; init; }

    /// <summary>
    /// Obtiene la descripción de la función de la herramienta para el modelo.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene el esquema JSON de los parámetros aceptados por la herramienta.
    /// </summary>
    public Dictionary<string, object?> Schema { get; init; } = new();
}