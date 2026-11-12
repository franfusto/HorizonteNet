namespace OpenDataAemet.Models;

/// <summary>
/// Excepción lanzada cuando ocurre un error al comunicarse con la API de AEMET OpenData.
/// </summary>
public class AemetException : Exception
{
    /// <summary>
    /// Código de estado devuelto por el servicio o código HTTP.
    /// </summary>
    public int Estado { get; }

    /// <summary>
    /// Descripción del error proporcionada por AEMET.
    /// </summary>
    public string Descripcion { get; }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="AemetException"/> con el mensaje de error, código de estado y descripción especificados.
    /// </summary>
    /// <param name="mensaje">Mensaje explicativo del error.</param>
    /// <param name="estado">Código de estado devuelto por el servicio o código de estado HTTP.</param>
    /// <param name="descripcion">Descripción detallada del error proporcionada por AEMET.</param>
    public AemetException(string mensaje, int estado = 0, string descripcion = "")
        : base(mensaje)
    {
        Estado = estado;
        Descripcion = descripcion;
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="AemetException"/> con el mensaje de error, la excepción interna que causó este error, el código de estado y la descripción especificados.
    /// </summary>
    /// <param name="mensaje">Mensaje explicativo del error.</param>
    /// <param name="innerException">Excepción interna que originó el error actual.</param>
    /// <param name="estado">Código de estado devuelto por el servicio o código de estado HTTP.</param>
    /// <param name="descripcion">Descripción detallada del error proporcionada por AEMET.</param>
    public AemetException(string mensaje, Exception innerException, int estado = 0, string descripcion = "")
        : base(mensaje, innerException)
    {
        Estado = estado;
        Descripcion = descripcion;
    }
}
