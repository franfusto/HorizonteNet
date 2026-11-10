using System.Text.Json.Serialization;

namespace OpenDataAemet.Models;

/// <summary>
/// Modelo de respuesta devuelto por los endpoints de AEMET OpenData.
/// </summary>
public class RespuestaAemet
{
    /// <summary>
    /// Descripción del resultado de la operación (ej: "Éxito", "Bad Request", "Unauthorized", etc.).
    /// </summary>
    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Código de estado HTTP / estado de la respuesta.
    /// </summary>
    [JsonPropertyName("estado")]
    public int Estado { get; set; }

    /// <summary>
    /// URL donde se encuentran los datos del resultado en formato final (JSON, XML, imagen, etc.).
    /// </summary>
    [JsonPropertyName("datos")]
    public string? Datos { get; set; }

    /// <summary>
    /// URL donde se encuentran los metadatos de la consulta.
    /// </summary>
    [JsonPropertyName("metadatos")]
    public string? Metadatos { get; set; }

    /// <summary>
    /// Indica si la petición fue exitosa (código 200).
    /// </summary>
    [JsonIgnore]
    public bool EsExitosa => Estado == 200;
}
