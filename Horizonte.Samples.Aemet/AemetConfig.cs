namespace Horizonte.Samples.Aemet;

/// <summary>
/// Representa la configuración necesaria para interactuar con la API de AEMET.
/// </summary>
/// <remarks>
/// La clase se utiliza para almacenar y gestionar las configuraciones básicas necesarias para realizar peticiones a la API de AEMET, incluyendo la clave API y la URL base del servicio.
/// Estas configuraciones son esenciales para garantizar el acceso adecuado y seguro a los datos proporcionados por el servicio de AEMET.
/// </remarks>
public class AemetConfig
{
    /// <summary>
    /// Representa la clave API utilizada para autenticar solicitudes a los servicios de AEMET.
    /// </summary>
    /// <remarks>
    /// Es fundamental proporcionar una clave API válida para acceder correctamente a la información meteorológica
    /// ofrecida por los servicios de AEMET. Esta propiedad debe ser configurada con los credenciales adecuados
    /// antes de realizar solicitudes.
    /// </remarks>
    public string ApiKey { get; set; } = "your_api_key";

    /// Representa la URL base para acceder a los datos abiertos de AEMET.
    /// La propiedad `BaseUrl` se utiliza para configurar la ruta base en las solicitudes
    /// a la API de AEMET. Este valor es crítico para establecer la conexión correcta
    /// con los servicios ofrecidos por la Agencia Estatal de Meteorología (AEMET) en España.
    /// Valor por defecto: "https://opendata.aemet.es/opendata"
    /// Esta propiedad es utilizada en la configuración de la instancia de API, asegurando
    /// que las solicitudes se envíen al endpoint adecuado.
    /// /
    public string BaseUrl { get; set; } ="https://opendata.aemet.es/opendata";
}