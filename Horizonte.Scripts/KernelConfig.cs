namespace Horizonte.Scripts;

/// La clase `KernelConfig` es responsable de mantener configuraciones de kernel dentro del sistema.
/// Esta clase almacena una lista de elementos de kernel, cada uno representado por una instancia de `KernelItem`.
/// Los elementos de kernel inicialmente incluyen dos configuraciones predeterminadas: "Default" y "Aislado", ambos con valores vacíos.
/// La lista de `KernelItems` es accesible y puede ser modificada externamente, permitiendo agregar o modificar configuraciones adicionales según sea necesario.
/// /
public class KernelConfig
{
    /// <summary>
    /// La clase <c>KernelConfig</c> se encarga de configurar los elementos del núcleo
    /// en una colección predeterminada. Esta configuración incluye elementos con nombres
    /// como "Default" y "Aislado", a los cuales se les asigna un valor inicial vacío.
    /// </summary>
    public KernelConfig()
    {
        KernelItems.Add(new KernelItem()
        {
            Name = "Default",
            Value = string.Empty,
        });
        KernelItems.Add(new KernelItem()
        {
            Name = "Aislado",
            Value = string.Empty,
        });
    }

    /// La propiedad `KernelItems` representa una lista de objetos de tipo `KernelItem` dentro de la clase `KernelConfig`.
    /// Se utiliza para almacenar y gestionar una colección de configuraciones de núcleo, cada una de las cuales está representada por un objeto `KernelItem`.
    /// Este objeto contiene atributos como `Name` y `Value` que definen las características de cada configuración específica del núcleo.
    /// La propiedad está inicializada por defecto con una lista que incluye al menos dos elementos: uno con el nombre "Default" y otro con el nombre "Aislado", ambos con valores vacíos inicialmente.
    /// `KernelItems` es fundamental para proporcionar datos utilizados en componentes que visualizan las configuraciones disponibles, como un menú desplegable en la interfaz de usuario.
    /// /
    public   List<KernelItem> KernelItems {get; set;} = new List<KernelItem>();
}

/// <summary>
/// La clase KernelItem representa un elemento individual dentro de una configuración de kernel.
/// </summary>
/// <remarks>
/// Cada instancia de KernelItem contiene un nombre y un valor asociados,
/// los cuales son utilizados para categorizar y almacenar información relevante
/// a su contexto de configuración.
/// </remarks>
public class KernelItem
{
    /// /
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Propiedad que representa un valor asignado a un objeto de tipo <c>KernelItem</c>.
    /// Este valor se utiliza para definir o configurar ciertas características del objeto.
    /// Es una cadena de texto que por defecto está inicializada como una cadena vacía.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}