namespace Horizonte.Extension.Ai.Definitions;

/// <summary>
/// Define la configuración de un cliente de modelo de IA (chat, embeddings, visión, etc.).
/// </summary>
public class ClientDef
{
    /// <summary>
    /// Obtiene o establece el identificador único del cliente.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre descriptivo del cliente.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la modalidad o tipo funcional del cliente (<see cref="Definitions.ClientType"/>).
    /// </summary>
    public ClientType ClientType { get; set; } = ClientType.Chat;

    /// <summary>
    /// Obtiene o establece el identificador del servidor o endpoint de inferencia asociado (<see cref="ServerDef"/>).
    /// </summary>
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre del modelo de IA a utilizar en las llamadas.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el valor de temperatura para el muestreo del modelo (opcional).
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Obtiene o establece el número máximo de tokens de salida generados (opcional).
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// Obtiene o establece si se habilita la transmisión de respuestas en streaming.
    /// </summary>
    public bool EnableStreaming { get; set; } = false;

    /// <summary>
    /// Obtiene o establece si se habilita el soporte para llamada a funciones y herramientas.
    /// </summary>
    public bool EnableTools { get; set; } = true;

    /// <summary>
    /// Obtiene o establece configuraciones adicionales específicas del proveedor o modelo.
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = new();
}