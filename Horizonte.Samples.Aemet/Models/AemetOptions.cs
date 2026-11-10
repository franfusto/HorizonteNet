namespace OpenDataAemet.Models;

/// <summary>
/// Opciones de configuración para el cliente de AEMET OpenData.
/// </summary>
public class AemetOptions
{
    /// <summary>
    /// Clave de API (api_key) obtenida a través de https://opendata.aemet.es/centrodedescargas/altaUsuario
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// URL base de la API de AEMET OpenData (por defecto: https://opendata.aemet.es/opendata).
    /// </summary>
    public string BaseUrl { get; set; } = "https://opendata.aemet.es/opendata";

    /// <summary>
    /// Timeout para las peticiones HTTP (por defecto: 30 segundos).
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Ubicación donde enviar la API Key (Header por defecto según OpenAPI securityScheme, o QueryString).
    /// </summary>
    public ApiKeyLocation UbicacionApiKey { get; set; } = ApiKeyLocation.Header;
}

/// <summary>
/// Modo de envío de la clave de API.
/// </summary>
public enum ApiKeyLocation
{
    /// <summary>
    /// Se envía en la cabecera HTTP 'api_key'.
    /// </summary>
    Header,

    /// <summary>
    /// Se envía como parámetro en la query string 'api_key'.
    /// </summary>
    QueryString
}
