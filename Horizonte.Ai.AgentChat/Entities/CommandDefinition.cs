namespace Horizonte.Ai.Agent;

/// <summary>
/// Representa la definición y metadatos de un comando ejecutable por un agente.
/// </summary>
public class CommandDefinition
{
    /// <summary>
    /// Nombre identificador del comando.
    /// </summary>
    public string CommandName = string.Empty;

    /// <summary>
    /// Descripción funcional del comando.
    /// </summary>
    public string CommandDescription = string.Empty;

    /// <summary>
    /// Roles o permisos asociados al comando.
    /// </summary>
    public string[] CommandRoles = Array.Empty<string>();

    /// <summary>
    /// Lista de parámetros requeridos u opcionales del comando.
    /// </summary>
    public List<CommandParameterDefinition> Parameters = new List<CommandParameterDefinition>(); 
}

/// <summary>
/// Define los metadatos de un parámetro para un comando.
/// </summary>
public class CommandParameterDefinition
{
    /// <summary>
    /// Nombre del parámetro.
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    /// Tipo de dato del parámetro (ej: string, int, boolean).
    /// </summary>
    public string Type = string.Empty;
}