namespace Horizonte
{
    /// <summary>
    /// Indica el origen de una sección de configuración.
    /// </summary>
    public enum SectionSource
    {
        /// <summary>
        /// La sección proviene del archivo de contexto principal.
        /// </summary>
        Context,

        /// <summary>
        /// La sección proviene de un archivo de sobrescritura local (local override).
        /// </summary>
        LocalOverride
    }
}
