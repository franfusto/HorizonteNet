namespace Horizonte.WorkFlows;

/// <summary>
/// Clase estática que proporciona un mapeo entre nombres de ensamblado y nombres de paquete NuGet.
/// </summary>
/// <remarks>
/// Utiliza un <see cref="Dictionary{TKey, TValue}"/> como estructura de datos para establecer relaciones entre
/// el nombre del ensamblado como clave y el nombre del paquete como valor, asegurando la correspondencia
/// entre ensamblados y sus respectivos paquetes de NuGet. Ignora las diferencias entre mayúsculas y minúsculas
/// al realizar búsquedas.
/// </remarks>
public static class Mapping
{
    /// <summary>
    /// AssemblyToPackageMap es un diccionario estático de solo lectura que proporciona un mapeo
    /// entre nombres de ensamblados y sus correspondientes nombres de paquetes NuGet.
    /// Este mapeo se define de manera que se puede tratar de forma insensible al caso.
    /// </summary>
    [AssemblyMappingDefinition]
    public static readonly Dictionary<string, string> AssemblyToPackageMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Blazor.Diagrams"] = "Z.Blazor.Diagrams",
            ["Blazor.Diagrams.Core"] = "Z.Blazor.Diagrams.Core"
        };
}