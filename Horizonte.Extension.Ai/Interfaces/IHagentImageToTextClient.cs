using Horizonte.Extension.Ai.Runtime.Files;

namespace Horizonte.Extension.Ai.Interfaces;

/// <summary>
/// Define la interfaz para clientes de IA capaces de describir o extraer información de imágenes (visión).
/// </summary>
public interface IHagentImageToTextClient
{
    /// <summary>
    /// Describe o analiza el contenido de una o más imágenes a partir de una solicitud estructurada.
    /// </summary>
    /// <param name="request">Solicitud con las imágenes y el prompt de instrucción.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado con el texto descriptivo y metadatos asociados.</returns>
    Task<HagentImageToTextResult> DescribeAsync(
        HagentImageToTextRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Representa una solicitud de procesamiento de imagen a texto.
/// </summary>
public sealed class HagentImageToTextRequest
{
    /// <summary>
    /// Obtiene o establece la instrucción o pregunta sobre las imágenes.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene la colección de archivos de imagen a procesar.
    /// </summary>
    public List<HagentFile> Images { get; } = [];

    /// <summary>
    /// Obtiene el diccionario de opciones adicionales para el modelo de visión.
    /// </summary>
    public Dictionary<string, object?> Options { get; } = [];
}

/// <summary>
/// Representa el resultado obtenido del procesamiento de imagen a texto.
/// </summary>
public sealed class HagentImageToTextResult
{
    /// <summary>
    /// Obtiene o establece el texto de respuesta o descripción generada.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene los metadatos devueltos por el proveedor o modelo de visión.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; } = [];
}