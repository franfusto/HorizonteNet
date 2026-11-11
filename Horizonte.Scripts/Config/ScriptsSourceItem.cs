namespace Horizonte.Scripts.Config;

/// <summary>
/// Representa un elemento de origen o fuente de scripts en la configuración.
/// </summary>
/// <remarks>
/// Define la información de una fuente de scripts, incluyendo un nombre identificativo y la ruta del directorio correspondiente.
/// </remarks>
public class ScriptsSourceItem
{
    /// <summary>
    /// Obtiene o establece el nombre identificador de la fuente de scripts.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la ruta del archivo o directorio asociado a la fuente de scripts.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}
