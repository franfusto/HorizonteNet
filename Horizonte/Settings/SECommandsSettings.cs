namespace Horizonte.Settings;

/// <summary>
/// Define la configuración de comandos de inicio y fin para el ciclo de vida de un servicio o aplicación Horizonte.
/// </summary>
public class SeCommandsSettings
{
    /// <summary>
    /// Obtiene o establece la lista de comandos a ejecutar durante el inicio del servicio.
    /// </summary>
    public List<SeCommandsSettingsItem> StartCommands { get; set; } =
        [new SeCommandsSettingsItem() { CommandName = "cmd", Active = false }];

    /// <summary>
    /// Obtiene o establece la lista de comandos a ejecutar durante la finalización o detención del servicio.
    /// </summary>
    public List<SeCommandsSettingsItem> EndCommands { get; set; } =
        [new SeCommandsSettingsItem() { CommandName = "cmd", Active = false }];
}

/// <summary>
/// Representa un elemento de configuración para comandos dentro del sistema.
/// </summary>
/// <remarks>
/// Esta clase define la estructura básica de un comando que puede ser utilizado en configuraciones específicas
/// dentro del sistema, permitiendo especificar el nombre del comando y su estado de activación.
/// </remarks>
public class SeCommandsSettingsItem
{
    /// <summary>
    /// Obtiene o establece el nombre o clave del comando de Horizonte a ejecutar.
    /// </summary>
    public string CommandName { get; set; } = "cmd";

    /// <summary>
    /// Obtiene o establece si el comando está activo y habilitado para ejecutarse.
    /// </summary>
    public bool Active { get; set; } = false;
    
}