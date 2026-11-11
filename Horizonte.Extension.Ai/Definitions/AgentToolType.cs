namespace Horizonte.Extension.Ai.Definitions;

/// <summary>
/// Especifica el tipo o propósito funcional de una herramienta de agente.
/// </summary>
public enum AgentToolType
{
    /// <summary>
    /// Búsqueda semántica en un almacén RAG.
    /// </summary>
    RagSearch,

    /// <summary>
    /// Procesamiento de imagen a texto (visión).
    /// </summary>
    ImageToText,

    /// <summary>
    /// Generación de imágenes a partir de descripciones textuales.
    /// </summary>
    TextToImage,

    /// <summary>
    /// Lectura de archivos locales o remotos.
    /// </summary>
    FileReader,

    /// <summary>
    /// Escritura y creación de archivos.
    /// </summary>
    FileWriter,

    /// <summary>
    /// Herramienta personalizada implementada por el usuario.
    /// </summary>
    Custom
}