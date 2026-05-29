namespace Horizonte.Scripts.Config;

/// <summary>
/// Representa un elemento fuente para los scripts en la configuración.
/// </summary>
/// <remarks>
/// Esta clase se utiliza para definir la información de origen de scripts,
/// incluyendo el nombre del origen y la ruta donde se encuentran los scripts.
/// </remarks>
public class ScriptsSourceItem
{
    /// <summary>
    /// La propiedad Name representa el nombre de la fuente de scripts dentro de un objeto ScriptsSourceItem.
    /// Este nombre identifica de manera única cada origen de scripts configurado.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la ruta del archivo o directorio relacionado con un elemento de origen de scripts.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}
