namespace Horizonte.Scripts;

/// <summary>
/// Gestiona la configuración de los kernels interactivos disponibles en el sistema.
/// </summary>
/// <remarks>
/// Almacena una lista de elementos de kernel (<see cref="KernelItem"/>) que definen las características
/// y modos de ejecución (como el kernel predeterminado y el kernel aislado).
/// </remarks>
public class KernelConfig
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="KernelConfig"/> con las configuraciones de kernel predeterminadas ("Default" y "Aislado").
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

    /// <summary>
    /// Obtiene o establece la lista de configuraciones de kernel (<see cref="KernelItem"/>) disponibles.
    /// </summary>
    public List<KernelItem> KernelItems { get; set; } = new List<KernelItem>();
}

/// <summary>
/// Representa un elemento individual de configuración de kernel.
/// </summary>
/// <remarks>
/// Cada instancia define un nombre descriptivo y valores o parámetros de configuración asociados.
/// </remarks>
public class KernelItem
{
    /// <summary>
    /// Obtiene o establece el nombre identificador del elemento de kernel.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el valor o parámetros de configuración asociados al elemento de kernel.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}