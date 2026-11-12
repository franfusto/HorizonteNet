namespace Horizonte;

/// <summary>
/// Representa la definición y configuración de un enlace simbólico en el sistema de archivos.
/// </summary>
/// <remarks>
/// Encapsula la ruta origen, el destino, si se trata de un directorio y si está configurado de forma estática en la aplicación.
/// </remarks>
public class SymLinkDef
{
    /// <summary>
    /// Obtiene o establece la ruta de origen en el sistema de archivos a la que apunta el enlace simbólico.
    /// </summary>
    public string  Source { get; set; } = string.Empty;

    /// <summary>
    /// Propiedad que define el destino de un enlace simbólico.
    /// Indica la ruta relativa a la que el enlace simbólico apuntará en el sistema de archivos.
    /// Esta propiedad debe contener una ruta que sea relativa al directorio de trabajo actual.
    /// Si se especifica una ruta absoluta, se generará un error.
    /// </summary>
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece si el enlace simbólico corresponde a un directorio (<c>true</c>) o a un archivo (<c>false</c>).
    /// </summary>
    public bool IsDirecory { get; set; } = false;

    /// <summary>
    /// Obtiene o establece si la definición del enlace simbólico está predefinida estáticamente en el código (<c>true</c> por defecto).
    /// </summary>
    public bool HardCoded { get; set; } = true;
}