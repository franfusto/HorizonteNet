namespace Horizonte
{
    /// <summary>
    /// Representa una interfaz de contexto utilizada para gestionar y recuperar datos con nombres contextuales opcionales.
    /// Proporciona funcionalidades para obtener y actualizar valores, admitiendo tanto actualizaciones directas 
    /// como actualizaciones mediante acciones específicas.
    /// </summary>
    public interface IHContext
    {
        T? Get<T>(string? contextname = null);
       void Update<T>(T value, string? contextname = null);
        public void Update<T>(Action<T> update, string? contextname = null);
    }
}