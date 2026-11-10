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

    public AemetException(string mensaje, int estado = 0, string descripcion = "")
        : base(mensaje)
    {
        Estado = estado;
        Descripcion = descripcion;
    }

    public AemetException(string mensaje, Exception innerException, int estado = 0, string descripcion = "")
        : base(mensaje, innerException)
    {
        Estado = estado;
        Descripcion = descripcion;
    }
}
