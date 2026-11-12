namespace Horizonte;

/// <summary>
/// Representa la configuración básica de un módulo en términos de su nombre identificador y versión.
/// </summary>
public class ModuleConfig
{
    /// <summary>
    /// Obtiene o establece el nombre del módulo.
    /// </summary>
    /// <remarks>
    /// Esta propiedad es utilizada para identificar el módulo dentro de la aplicación.
    /// Se debe asignar un valor representativo del módulo al que pertenece.
    /// </remarks>
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la versión semántica del módulo (por defecto "0.0.0").
    /// </summary>
    public string ModuleVersion { get; set; } = "0.0.0";
}