namespace Horizonte;

/// <summary>
/// Define las operaciones para la gestión de credenciales en el sistema Horizonte.
/// </summary>
public interface IHCredManager
{
    /// <summary>
    /// Obtiene una lista de todas las claves de credenciales disponibles.
    /// </summary>
    public IEnumerable<string> KeyList {get; }

    /// <summary>
    /// Obtiene el valor de una credencial específica.
    /// Si la credencial no existe, se crea una nueva entrada con valor vacío.
    /// </summary>
    /// <param name="key">La clave de la credencial.</param>
    /// <returns>El valor de la credencial asociada a la clave.</returns>
    public string GetCredential(string key);

    /// <summary>
    /// Establece o actualiza el valor de una credencial.
    /// </summary>
    /// <param name="key">La clave de la credencial.</param>
    /// <param name="value">El valor a asignar.</param>
    public void SetCredential(string key, string value);
}