namespace Horizonte;

/// <summary>
/// Define la estructura y configuración de un widget visual dentro de los módulos del marco Horizonte.
/// </summary>
/// <remarks>
/// Permite especificar el tipo CLR del componente o widget y los parámetros personalizados necesarios para su inicialización.
/// </remarks>
public class WidgetDef
{
    /// <summary>
    /// Obtiene o establece el <see cref="System.Type"/> del componente que implementa el widget.
    /// </summary>
    public required Type Type { get; set; }

    /// <summary>
    /// Propiedad que permite establecer o recuperar un diccionario de parámetros personalizados para el widget.
    /// Este diccionario contiene pares clave-valor donde la clave es un string que representa el nombre del parámetro
    /// y el valor es un objeto que representa el valor asociado a dicho parámetro.
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }
}