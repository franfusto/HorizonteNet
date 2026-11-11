namespace Horizonte.Extension.Ai.Definitions;

/// <summary>
/// Especifica el motor de base de datos o almacenamiento vectorial utilizado para RAG.
/// </summary>
public enum RagType
{
    /// <summary>
    /// Almacén vectorial en memoria volátil.
    /// </summary>
    InMemory,

    /// <summary>
    /// Base de datos MongoDB con soporte de búsqueda vectorial.
    /// </summary>
    MongoDb,

    /// <summary>
    /// Base de datos SQLite con extensiones vectoriales.
    /// </summary>
    SqliteDB,

    /// <summary>
    /// Base de datos PostgreSQL con extensión pgvector.
    /// </summary>
    Postgres,

    /// <summary>
    /// Motor de base de datos vectorial Qdrant.
    /// </summary>
    Qdrant,

    /// <summary>
    /// Almacén en memoria Redis con búsqueda vectorial.
    /// </summary>
    Redis,

    /// <summary>
    /// Servicio en la nube Azure AI Search.
    /// </summary>
    AzureAISearch,

    /// <summary>
    /// Motor de búsqueda distribuida Elasticsearch con vectores densos.
    /// </summary>
    Elasticsearch,

    /// <summary>
    /// Motor de búsqueda vectorial personalizado.
    /// </summary>
    Custom
}