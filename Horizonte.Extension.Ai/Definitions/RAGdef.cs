namespace Horizonte.Extension.Ai.Definitions;


/// <summary>
/// Define la configuración de un almacén o índice RAG (Retrieval-Augmented Generation) para búsqueda semántica.
/// </summary>
public class RaGdef
{
    /// <summary>
    /// Obtiene o establece el identificador único de la configuración RAG.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre descriptivo del índice RAG.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el tipo de backend de almacenamiento vectorial (<see cref="RagType"/>).
    /// </summary>
    public RagType RagType { get; set; } = RagType.InMemory;

    /// <summary>
    /// Obtiene o establece el identificador del cliente de embeddings asociado (<see cref="ClientDef"/>).
    /// </summary>
    public string EmbedderClient { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre de la colección o tabla vectorial subyacente.
    /// </summary>
    public string CollectionName { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la dimensión o tamaño del vector de embeddings.
    /// </summary>
    public int Size { get; set; } = 0;

    /// <summary>
    /// Obtiene o establece el número de resultados más cercanos a recuperar (Top-K).
    /// </summary>
    public int TopK { get; set; } = 5;

    /// <summary>
    /// Obtiene o establece el umbral mínimo de similitud requerido para aceptar un resultado.
    /// </summary>
    public float MinScore { get; set; } = 0.0f;

    /// <summary>
    /// Obtiene o establece el tamaño en caracteres o tokens de cada fragmento (chunk) de documento.
    /// </summary>
    public int ChunkSize { get; set; } = 1000;

    /// <summary>
    /// Obtiene o establece el solapamiento entre fragmentos consecutivos.
    /// </summary>
    public int ChunkOverlap { get; set; } = 150;

    /// <summary>
    /// Obtiene o establece el número máximo de elementos de contexto a inyectar en el prompt.
    /// </summary>
    public int MaxContextItems { get; set; } = 8;

    /// <summary>
    /// Obtiene o establece si este almacén RAG está habilitado como herramienta invocable por agentes.
    /// </summary>
    public bool EnabledAsTool { get; set; } = true;

    /// <summary>
    /// Obtiene o establece configuraciones adicionales del motor de búsqueda vectorial.
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = new();
}