namespace Horizonte.AdminUi;

/// Representa un menú gráfico de usuario (GUI) que contiene una colección de elementos de menú.
/// La clase `HGuiMenu` proporciona las propiedades necesarias para definir un menú, incluyendo un título (caption) y una lista de sus elementos (items).
/// Cada elemento de este menú está definido por la clase `HGuiMenuItem`, la cual especifica las características y comportamiento individual de los elementos, incluyendo su identificador, ícono, y componente asociado.
/// Propiedades:
/// - `Caption`: Define el título del menú.
/// - `Items`: Una lista de objetos `HGuiMenuItem` que representan los elementos contenidos en el menú.
/// /
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

/// La clase HGuiMenuItem representa un elemento de menú en la interfaz gráfica de usuario.
/// Esta clase es parte del espacio de nombres Horizonte.AdminUi y se utiliza para definir las propiedades de un elemento de menú.
/// Un HGuiMenuItem contiene la siguiente información:
/// - Id: un identificador único para el elemento del menú.
/// - ParentId: el identificador del elemento superior en el menú, que establece una relación jerárquica entre los elementos.
/// - Icon: la representación del ícono asociado al elemento del menú.
/// - Caption: el texto que se muestra para este elemento de menú.
/// - Component: el nombre del componente asociado que se activa cuando se selecciona este elemento del menú.
/// Esta clase se utiliza en conjunto con otras clases, como HGuiMenu, para estructurar y manipular los menús dentro de la aplicación de usuario.
/// /
public class HGuiMenuItem
{
    /// Representa el identificador único de un elemento del menú gráfico de usuario (HGuiMenuItem).
    /// Este identificador se utiliza para diferenciar y manipular cada elemento de manera individual dentro de la estructura del menú.
    /// Es un valor de tipo entero que, por defecto, se inicializa a 0.
    public int Id { get; set; } = 0;

    /// <summary>
    /// Representa el identificador del elemento padre dentro de una jerarquía de elementos del menú.
    /// </summary>
    /// <remarks>
    /// <para>Esta propiedad se utiliza para estructurar los elementos del menú de manera jerárquica, permitiendo definir qué elemento actúa como un contenedor o categoría para otros elementos.</para>
    /// <para>Por defecto, su valor inicial es 0, lo que generalmente indica que el elemento es un nodo primario y no tiene un padre.</para>
    /// </remarks>
    public int ParentId { get; set; } = 0;

    /// Propiedad que representa el icono asociado a un elemento del menú gráfico.
    /// El valor de esta propiedad es una cadena que típicamente corresponde al nombre
    /// de un icono, el cual puede ser utilizado para representar visualmente el elemento en la interfaz de usuario.
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Representa el texto que se muestra como título o nombre de un elemento del menú en la interfaz gráfica.
    /// </summary>
    public string Caption { get; set; } = string.Empty;

    /// <summary>
    /// Representa el nombre del componente que se asociará con un elemento de menú en la interfaz gráfica.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se utiliza para determinar el componente específico que se debe renderizar o asociar cuando se selecciona un elemento de menú.
    /// Es importante establecer un valor significativo para esta propiedad para asegurar la correcta funcionalidad del menú.
    /// </remarks>
    public string Component { get; set; } = string.Empty;
        
}