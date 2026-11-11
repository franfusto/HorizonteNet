namespace Horizonte.Extension.Ai.Definitions;

/// <summary>
/// Define la configuración de permisos, límites y procesamiento de archivos para un agente de IA.
/// </summary>
public class AgentFileOptions
{
    /// <summary>
    /// Obtiene o establece si se permite el envío de archivos de entrada al agente.
    /// </summary>
    public bool AllowInputFiles { get; set; } = true;

    /// <summary>
    /// Obtiene o establece si se permite la generación o devolución de archivos de salida por parte del agente.
    /// </summary>
    public bool AllowOutputFiles { get; set; } = true;

    /// <summary>
    /// Obtiene o establece el esquema de codificación de transporte de archivos (por defecto "base64").
    /// </summary>
    public string TransportEncoding { get; set; } = "base64";

    /// <summary>
    /// Obtiene o establece el tamaño máximo en bytes permitido para archivos de entrada.
    /// </summary>
    public int MaxInputFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Obtiene o establece el tamaño máximo en bytes permitido para archivos de salida generados.
    /// </summary>
    public int MaxOutputFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Obtiene o establece la lista de tipos MIME permitidos para los archivos de entrada.
    /// </summary>
    public string[] AllowedInputMimeTypes { get; set; } = [];

    /// <summary>
    /// Obtiene o establece la lista de tipos MIME permitidos para los archivos de salida.
    /// </summary>
    public string[] AllowedOutputMimeTypes { get; set; } = [];

    /// <summary>
    /// Obtiene o establece si las imágenes adjuntas deben procesarse automáticamente mediante herramientas de visión.
    /// </summary>
    public bool AutoProcessImages { get; set; } = true;

    /// <summary>
    /// Obtiene o establece si los documentos adjuntos deben procesarse automáticamente.
    /// </summary>
    public bool AutoProcessDocuments { get; set; } = false;

    /// <summary>
    /// Obtiene o establece configuraciones adicionales para el manejo de archivos.
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = new();
}