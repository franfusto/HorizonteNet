using Horizonte.Extension.Ai.Runtime.Files;

namespace Horizonte.Extension.Ai.Interfaces;

/// <summary>
/// Define la interfaz para clientes de IA capaces de sintetizar o generar imágenes a partir de descripciones de texto.
/// </summary>
public interface IHagentTextToImageClient
{
    /// <summary>
    /// Genera una o más imágenes a partir de una solicitud de texto de forma asincrónica.
    /// </summary>
    /// <param name="request">Solicitud con el prompt, formato y dimensiones requeridas.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado que contiene las imágenes generadas y metadatos.</returns>
    Task<HagentTextToImageResult> GenerateAsync(
        HagentTextToImageRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Representa una solicitud para generar imágenes a partir de un texto instructivo.
/// </summary>
public sealed class HagentTextToImageRequest
{
    /// <summary>
    /// Obtiene o establece la descripción o prompt visual para la generación.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el tipo MIME de la imagen generada (por defecto "image/png").
    /// </summary>
    public string MimeType { get; set; } = "image/png";

    /// <summary>
    /// Obtiene o establece las dimensiones de la imagen (por defecto "1024x1024").
    /// </summary>
    public string Size { get; set; } = "1024x1024";

    /// <summary>
    /// Obtiene opciones adicionales para el generador de imágenes.
    /// </summary>
    public Dictionary<string, object?> Options { get; } = [];
}

/// <summary>
/// Representa el resultado de una solicitud de generación de imágenes.
/// </summary>
public sealed class HagentTextToImageResult
{
    /// <summary>
    /// Obtiene la lista de imágenes generadas (<see cref="HagentGeneratedImage"/>).
    /// </summary>
    public List<HagentGeneratedImage> Images { get; } = [];

    /// <summary>
    /// Obtiene metadatos devueltos por el proveedor de generación.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; } = [];
}

/// <summary>
/// Representa una imagen binaria generada por un modelo de IA.
/// </summary>
public sealed class HagentGeneratedImage
{
    /// <summary>
    /// Obtiene o establece el nombre sugerido para el archivo de imagen generado.
    /// </summary>
    public string FileName { get; set; } = "generated-image.png";

    /// <summary>
    /// Obtiene o establece el tipo MIME de la imagen generada.
    /// </summary>
    public string MimeType { get; set; } = "image/png";

    /// <summary>
    /// Obtiene o establece los bytes del contenido de la imagen.
    /// </summary>
    public byte[] Content { get; set; } = [];

    /// <summary>
    /// Obtiene metadatos adicionales asociados a la imagen.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; } = [];
}