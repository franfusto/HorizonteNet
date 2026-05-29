namespace Horizonte.Extensions.Interactive;

/// <summary>
/// La clase <c>KernelOptions</c> representa un conjunto de opciones de configuración para un núcleo interactivo en el contexto de las extensiones de Horizonte.
/// </summary>
/// <remarks>
/// Esta clase permite definir y modificar los parámetros configurables que afectan el funcionamiento de un núcleo interactivo.
/// Es parte del espacio de nombres <c>Horizonte.Extensions.Interactive</c>.
/// </remarks>
public class KernelOptions
{
    /// <summary>
    /// Propiedad que representa el nombre asociado a las opciones del núcleo.
    /// </summary>
    /// <remarks>
    /// Esta propiedad puede ser nula, pero se inicializa de manera predeterminada como una cadena vacía.
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Propiedad que indica si el núcleo está aislado o no.
    /// Si no es aislado, se incluye el ServiceProvider en el contexto del núcleo.
    /// </summary>
    /// <remarks>
    /// Cuando está establecida en <c>true</c>, el núcleo opera en un modo aislado, lo que puede afectar su interacción con otros componentes.
    /// El valor predeterminado de esta propiedad es <c>false</c>.
    /// </remarks>
    public bool Isolated { get; set; } = false;
}