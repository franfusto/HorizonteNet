namespace Horizonte.AdminUi.HorizonteSettings.Entidades;

/// <summary>
/// La clase <c>DiagramLayoutOptions</c> configura las opciones de diseño para la disposición de un diagrama.
/// Es un objeto de configuración que permite personalizar diversos aspectos del diseño de nodos y enlaces dentro de un diagrama.
/// </summary>
/// <remarks>
/// Esta clase es inmutable después de su inicialización, lo que significa que todas sus propiedades son de solo lectura una vez que se ha creado una instancia.
/// </remarks>
public sealed class DiagramLayoutOptions
{
    /// <summary>
    /// Representa el ancho del nodo dentro de un diagrama.
    /// Esta propiedad se utiliza para definir el tamaño horizontal de un nodo al establecer el diseño del diagrama.
    /// Dependiendo de la implementación del diagrama, el ancho del nodo puede afectar la distribución y apariencia general del mismo.
    /// </summary>
    public double NodeWidth { get; init; } = 180;

    /// <summary>
    /// Representa la altura de un nodo en el layout del diagrama.
    /// </summary>
    /// <remarks>
    /// Esta propiedad define la dimensión vertical de los nodos dentro de un diagrama,
    /// influyendo en el espacio ocupado y en la disposición visual general de los elementos.
    /// Es utilizada, por ejemplo, al crear rectángulos en el contexto de un gráfico geométrico
    /// donde se especifican las dimensiones del nodo.
    /// </remarks>
    public double NodeHeight { get; init; } = 60;

    /// Propiedad que define la separación entre nodos en un diagrama.
    /// Se utiliza para establecer la distancia horizontal o vertical mínima entre nodos adyacentes en la disposición del diagrama.
    /// Un valor mayor incrementará el espacio entre nodos, mientras que un valor menor los acercará.
    /// /
    public double NodeSeparation { get; init; } = 50;

    /// <summary>
    /// Propiedad que define la separación entre capas en un diagrama cuando se utiliza un diseño de disposición por capas (Layered).
    /// Este valor determina la distancia vertical entre las capas de nodos, lo que permite ajustar el espacio del diagrama para
    /// adaptarse mejor a las necesidades de visualización y presentación.
    /// </summary>
    public double LayerSeparation { get; init; } = 80;

    /// Representa la longitud ideal de una arista dentro de un diagrama en el contexto de las opciones de diseño de diagramas.
    /// Esta propiedad se utiliza para determinar el espaciamiento óptimo entre nodos cuando se dibujan aristas que los conectan,
    /// con el fin de mejorar la claridad y organización del diagrama.
    /// La longitud ideal de una arista puede influir en el aspecto visual del diagrama, afectando cómo se distribuyen los nodos
    /// y cómo se visualizan las conexiones entre ellos. Un valor adecuado para esta propiedad puede ayudar a minimizar el solapamiento
    /// de aristas y mejorar la legibilidad del diagrama.
    /// /
    public double IdealEdgeLength { get; init; } = 120;

    /// <summary>
    /// Propiedad que especifica el desplazamiento horizontal (en el eje X) aplicado
    /// a todas las posiciones de los nodos en un diagrama. Se utiliza para ajustar
    /// la posición base del contenido visualizado, permitiendo un control adicional
    /// sobre el diseño del diagrama.
    /// </summary>
    public double OffsetX { get; init; } = 50;

    /// <summary>
    /// Propiedad que define el desplazamiento vertical en el sistema de coordenadas de un diagrama.
    /// </summary>
    /// <remarks>
    /// El valor de OffsetY se utiliza para ajustar la posición vertical inicial de los nodos en el diagrama.
    /// Es útil al configurar la disposición gráfica de nodos, permitiendo un espaciado personalizado en el eje Y.
    /// </remarks>
    public double OffsetY { get; init; } = 50;

    /// Indica si se deben incluir enlaces a sí mismos (self-links) en el diseño del diagrama.
    /// Los enlaces a sí mismos son conexiones en las que el nodo de origen y el nodo de destino
    /// son el mismo. Esta propiedad permite controlar si se deben considerar tales conexiones al
    /// establecer el diseño del diagrama.
    /// Tipo de dato: booleano.
    /// Valor por defecto: depende del contexto de uso donde se inicialice la propiedad.
    /// /
    public bool IncludeSelfLinks { get; init; }
}