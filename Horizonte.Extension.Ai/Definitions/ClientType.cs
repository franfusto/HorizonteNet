namespace Horizonte.Extension.Ai.Definitions;

/// <summary>
/// Especifica la modalidad o propósito funcional de un cliente de IA.
/// </summary>
public enum ClientType
{
    /// <summary>
    /// Modelo de chat o generación conversacional de texto.
    /// </summary>
    Chat,

    /// <summary>
    /// Modelo para cálculo y generación de embeddings vectoriales.
    /// </summary>
    Embedder,

    /// <summary>
    /// Modelo para síntesis y generación de imágenes a partir de texto.
    /// </summary>
    TextToImage,

    /// <summary>
    /// Modelo de visión para análisis y descripción de imágenes.
    /// </summary>
    ImageToText,

    /// <summary>
    /// Modelo de transcripción de audio a texto (speech-to-text).
    /// </summary>
    AudioToText,

    /// <summary>
    /// Modelo de síntesis de voz a partir de texto (text-to-speech).
    /// </summary>
    TextToAudio,

    /// <summary>
    /// Modelo de reordenamiento de relevancia semántica (reranking).
    /// </summary>
    Reranker,

    /// <summary>
    /// Reconocimiento óptico de caracteres (OCR).
    /// </summary>
    Ocr,

    /// <summary>
    /// Extractor estructurado de documentos.
    /// </summary>
    DocumentExtractor,

    /// <summary>
    /// Modelo de moderación y filtro de contenidos.
    /// </summary>
    Moderation,

    /// <summary>
    /// Cliente de IA personalizado.
    /// </summary>
    Custom
}