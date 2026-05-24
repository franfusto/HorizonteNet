namespace Horizonte
{
    /// <summary>
    /// Define un contrato para acceder y modificar datos de contexto tipados.
    /// </summary>
    /// <remarks>
    /// Las implementaciones de esta interfaz permiten recuperar y actualizar secciones de configuración
    /// o estado usando el nombre del tipo <typeparamref name="T"/> como identificador de sección.
    /// También pueden soportar múltiples contextos mediante un nombre opcional y priorizar orígenes
    /// alternativos, como sobrescrituras locales.
    /// </remarks>

    public interface IHContext
    {

        /// <summary>
        /// Obtiene la ruta raíz asociada al contexto actual.
        /// </summary>
        /// <remarks>
        /// Esta propiedad identifica la ubicación base utilizada por la implementación para resolver
        /// archivos, recursos o rutas relacionadas con el contexto.
        /// </remarks>
        public string RootPath { get; }
        
        /// <summary>
        /// Obtiene el nombre del contexto actual.
        /// </summary>
        /// <remarks>
        /// Este valor representa el identificador lógico del contexto predeterminado que utilizará
        /// la implementación cuando no se especifique uno explícitamente en las operaciones.
        /// </remarks>
        public string ContextName {get;}
        
        /// <summary>
        /// Recupera la sección asociada al tipo especificado.
        /// </summary>
        /// <typeparam name="T">Tipo de la sección que se desea recuperar.</typeparam>
        /// <param name="contextname">
        /// Nombre opcional del contexto. Si no se especifica, la implementación utilizará el contexto predeterminado.
        /// </param>
        /// <returns>
        /// La instancia deserializada de <typeparamref name="T"/> si existe; en caso contrario, <see langword="default"/>.
        /// </returns>
        /// <remarks>
        /// La implementación puede decidir el origen de los datos, por ejemplo priorizando una sobrescritura local
        /// frente al archivo de contexto principal.
        /// </remarks>
        T? Get<T>(string? contextname = null);

        /// <summary>
        /// Reemplaza o crea la sección asociada al tipo especificado con el valor proporcionado.
        /// </summary>
        /// <typeparam name="T">Tipo de los datos que se desean guardar.</typeparam>
        /// <param name="value">Valor que se almacenará en la sección correspondiente.</param>
        /// <param name="contextname">
        /// Nombre opcional del contexto. Si no se especifica, la implementación utilizará el contexto predeterminado.
        /// </param>
        /// <remarks>
        /// La sección a actualizar se identifica normalmente mediante <c>typeof(T).Name</c>.
        /// El destino de escritura depende de la implementación y del origen efectivo de la sección.
        /// </remarks>
        void Update<T>(T value, string? contextname = null);

        /// <summary>
        /// Actualiza la sección asociada al tipo especificado aplicando una acción sobre su valor actual.
        /// </summary>
        /// <typeparam name="T">Tipo de la sección que se desea modificar.</typeparam>
        /// <param name="update">Acción que modifica el valor actual de la sección.</param>
        /// <param name="contextname">
        /// Nombre opcional del contexto. Si no se especifica, la implementación utilizará el contexto predeterminado.
        /// </param>
        /// <remarks>
        /// Este método es útil cuando se desea modificar parcialmente una sección existente sin reemplazar
        /// explícitamente el objeto desde el código consumidor.
        /// </remarks>
        void Update<T>(Action<T> update, string? contextname = null);

        /// <summary>
        /// Obtiene el origen efectivo de la sección asociada al tipo especificado.
        /// </summary>
        /// <typeparam name="T">Tipo de la sección cuyo origen se desea consultar.</typeparam>
        /// <param name="contextname">
        /// Nombre opcional del contexto. Si no se especifica, la implementación utilizará el contexto predeterminado.
        /// </param>
        /// <returns>
        /// Un valor de <see cref="SectionSource"/> que indica desde dónde se resolverá la sección.
        /// </returns>
        /// <remarks>
        /// Este método permite conocer si una sección se obtiene desde el contexto principal o desde una
        /// sobrescritura local, lo que resulta útil para depuración y trazabilidad.
        /// </remarks>
        SectionSource Source<T>(string? contextname = null);
    }
}