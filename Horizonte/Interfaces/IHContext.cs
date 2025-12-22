namespace Horizonte
{
    /// <summary>
    /// Representa una interfaz de contexto utilizada para gestionar y recuperar datos con nombres contextuales opcionales.
    /// Proporciona funcionalidades para obtener y actualizar valores, admitiendo tanto actualizaciones directas 
    /// como actualizaciones mediante acciones específicas.
    /// </summary>
    public interface IHContext
    {
        /// <summary>
        /// Recupera un objeto del tipo especificado desde el archivo de contexto, utilizando el
        /// nombre del contexto proporcionado o el nombre de contexto predeterminado si no se especifica.
        /// </summary>
        /// <typeparam name="T">El tipo de objeto a recuperar.</typeparam>
        /// <param name="contextname">Opcional. El nombre específico del contexto.</param>
        /// <returns>El objeto deserializado o el valor predeterminado si no se encuentra.</returns>
        T? Get<T>(string? contextname = null);

        /// <summary>
        /// Actualiza el archivo de contexto con un nuevo valor para el tipo especificado.
        /// </summary>
        /// <typeparam name="T">El tipo de los datos a actualizar.</typeparam>
        /// <param name="value">El nuevo valor a guardar.</param>
        /// <param name="contextname">Opcional. El nombre específico del contexto.</param>
        void Update<T>(T value, string? contextname = null);

        /// <summary>
        /// Actualiza una sección específica del archivo de contexto mediante una acción de modificación.
        /// </summary>
        /// <typeparam name="T">El tipo de la sección a actualizar.</typeparam>
        /// <param name="update">Acción que modifica los datos existentes.</param>
        /// <param name="contextname">Opcional. El nombre específico del contexto.</param>
        public void Update<T>(Action<T> update, string? contextname = null);

        /// <summary>
        /// Determina el origen de los datos para la sección del tipo especificado.
        /// </summary>
        /// <typeparam name="T">El tipo de la sección a consultar.</typeparam>
        /// <param name="contextname">Opcional. El nombre específico del contexto.</param>
        /// <returns>El origen de la sección (Contexto o LocalOverride).</returns>
        public SectionSource Source<T>(string? contextname = null);
    }
}