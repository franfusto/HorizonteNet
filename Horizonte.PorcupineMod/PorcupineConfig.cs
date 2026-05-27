namespace Horizonte.PorcupineMod;

/// La clase `PorcupineConfig` se utiliza para representar la configuración necesaria para trabajar con el módulo Porcupine en la aplicación.
/// La configuración controla el acceso a los servicios de Porcupine a través de una clave de acceso.
/// Propiedades:
/// - `AccessKey`: Cadena que representa la clave de acceso necesaria para la autenticación y uso de los servicios de Porcupine. Es una cadena vacía por defecto y debe ser configurada adecuadamente antes de usar el módulo Porcupine.
/// Esta clase es esencial para garantizar que la aplicación pueda comunicarse correctamente con el servicio de Porcupine, proporcionando la clave de acceso requerida.
/// /
public class PorcupineConfig
{
    /// <summary>
    /// Representa la clave de acceso necesaria para autenticar las operaciones realizadas por el módulo Porcupine.
    /// </summary>
    /// <remarks>
    /// Esta propiedad es utilizada para obtener el valor de la clave de acceso desde un gestor de credenciales,
    /// permitiendo la configuración y funcionamiento correcto del servicio de reconocimiento de voz Porcupine.
    /// </remarks>
    public string AccessKey { get; set; } = string.Empty;
}