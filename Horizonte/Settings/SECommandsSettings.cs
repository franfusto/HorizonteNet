namespace Horizonte.Settings;

/// La clase SeCommandsSettings representa la configuración de comandos de inicio y fin para un servicio o aplicación.
/// Esta clase permite definir y gestionar listas de comandos que deben ejecutarse al inicio y al final de un proceso.
/// Los comandos se almacenan en dos listas separadas: StartCommands para los comandos de inicio, y EndCommands para los comandos de finalización.
/// Cada comando se representa mediante un objeto de tipo SeCommandsSettingsItem, que incluye el nombre del comando y su estado de activación.
/// Propiedades:
/// - StartCommands: Lista de comandos que se deben ejecutar al inicio de un servicio o aplicación.
/// - EndCommands: Lista de comandos que se deben ejecutar al finalizar un servicio o aplicación.
/// /
public class SeCommandsSettings
{
    /// /
    public List<SeCommandsSettingsItem> StartCommands { get; set; } =
        [new SeCommandsSettingsItem() { CommandName = "cmd", Active = false }];

    /// La propiedad `EndCommands` representa una lista de comandos que se ejecutan al finalizar un proceso o tarea específica.
    /// Cada elemento de la lista es una instancia de `SeCommandsSettingsItem`, que contiene las propiedades `CommandName`
    /// para especificar el nombre del comando y `Active` para indicar si el comando está habilitado para ser ejecutado.
    /// Esta propiedad se utiliza para gestionar y almacenar los comandos de finalización, permitiendo que solo aquellos
    /// comandos marcados como activos sean ejecutados. Es parte de la configuración de comandos del sistema dentro de
    /// la aplicación.
    /// /
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
    /// Propiedad que representa el nombre del comando que se desea ejecutar.
    /// CommandName es una cadena de texto que identifica concretamente un comando
    /// que puede ser ejecutado en el contexto de una configuración específica.
    /// Los comandos se pueden utilizar dentro de una lista de comandos iniciales
    /// o finales, y cada uno está asociado a un estado activo o inactivo definido
    /// por otra propiedad.
    /// Esta propiedad es esencial para reconocer y vincular comandos específicos
    /// con la lógica de negocio que permite su ejecución. El valor predeterminado
    /// de esta propiedad es "cmd".
    /// /
    public string CommandName { get; set; } = "cmd";

    /// Propiedad que indica si un comando está activo o no.
    /// Utiliza un valor booleano donde `true` significa que el comando está activo y debe ejecutarse,
    /// mientras que `false` indica que el comando está inactivo y no debe ejecutarse.
    /// /
    public bool Active { get; set; } = false;
    
}