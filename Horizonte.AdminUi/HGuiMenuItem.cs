namespace Horizonte.AdminUi;

/// <summary>
/// Representa un menú gráfico de usuario (GUI) que contiene una colección de elementos de menú.
/// </summary>
/// <remarks>
/// Proporciona las propiedades necesarias para definir un menú, incluyendo un título descriptivo y la lista de sus elementos subordinados.
/// </remarks>
public class HGuiMenu
{
    /// <summary>
    /// Representa el texto descriptivo o título de un elemento en la interfaz gráfica de usuario.
    /// </summary>
    public string Caption { get; set; } = string.Empty;

    /// <summary>
    /// Representa una colección de elementos de tipo <see cref="HGuiMenuItem"/> asociados al menú gráfico
    /// de usuario (<see cref="HGuiMenu"/>).
    /// </summary>
    public List<HGuiMenuItem> Items { get; set; } = new List<HGuiMenuItem>();
}

/// <summary>
/// Representa un elemento individual o nodo dentro de la estructura de menús en la interfaz gráfica de usuario.
/// </summary>
/// <remarks>
/// Define los identificadores jerárquicos (Id, ParentId), icono visual, etiqueta de texto y comando o componente asociado a ejecutar al seleccionarlo.
/// </remarks>
public class HGuiMenuItem
{
    /// <summary>
    /// Representa el identificador único de un elemento del menú gráfico de usuario (<see cref="HGuiMenuItem"/>).
    /// </summary>
    public int Id { get; set; } = 0;

    /// <summary>
    /// Representa el identificador del elemento padre dentro de una jerarquía de elementos del menú.
    /// </summary>
    /// <remarks>
    /// <para>Esta propiedad se utiliza para estructurar los elementos del menú de manera jerárquica, permitiendo definir qué elemento actúa como un contenedor o categoría para otros elementos.</para>
    /// <para>Por defecto, su valor inicial es 0, lo que generalmente indica que el elemento es un nodo primario y no tiene un padre.</para>
    /// </remarks>
    public int ParentId { get; set; } = 0;

    /// <summary>
    /// Obtiene o establece el nombre del icono asociado visualmente al elemento del menú.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Representa el texto que se muestra como título o nombre de un elemento del menú en la interfaz gráfica.
    /// </summary>
    public string Caption { get; set; } = string.Empty;

    /// <summary>
    /// Representa el nombre del componente o comando que se asociará con un elemento de menú en la interfaz gráfica.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se utiliza para determinar el componente específico que se debe renderizar o asociar cuando se selecciona un elemento de menú.
    /// Es importante establecer un valor significativo para esta propiedad para asegurar la correcta funcionalidad del menú.
    /// </remarks>
    public string Component { get; set; } = string.Empty;
}