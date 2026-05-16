namespace Horizonte.Interfaces;

/// <summary>
/// Define las operaciones necesarias para administrar un andamio de enlaces simbólicos
/// dentro del entorno de ejecución actual.
/// </summary>
/// <remarks>
/// Un andamio de enlaces simbólicos permite crear, consultar y limpiar un conjunto
/// de definiciones de enlaces simbólicos utilizadas por la aplicación.
/// </remarks>
public interface ISymLinkScafolder
{
    /// <summary>
    /// Elimina los enlaces simbólicos gestionados por el andamio actual.
    /// </summary>
    /// <remarks>
    /// La implementación puede realizar la limpieza de forma recursiva dentro
    /// del directorio de trabajo y omitir la operación si el andamio está siendo
    /// utilizado por otra instancia.
    /// </remarks>
    public void CleanScafolder();

    /// <summary>
    /// Construye el andamio de enlaces simbólicos a partir de una colección de definiciones.
    /// </summary>
    /// <param name="symlinklist">
    /// Colección de definiciones que describen los enlaces simbólicos que deben crearse.
    /// </param>
    /// <remarks>
    /// La implementación puede validar las rutas de destino, crear directorios
    /// intermedios necesarios y registrar únicamente los enlaces construidos correctamente.
    /// </remarks>
    public void BuildScafolder(IEnumerable<SymLinkDef> symlinklist);

    /// <summary>
    /// Obtiene las definiciones de enlaces simbólicos actualmente registradas en el andamio.
    /// </summary>
    /// <returns>
    /// Una secuencia con las definiciones de enlaces simbólicos conocidas por la implementación.
    /// </returns>
    public IEnumerable<SymLinkDef> GetScafolder();
}