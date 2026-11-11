namespace Horizonte.Extension.Ai.Definitions;

/// <summary>
/// Contenedor de configuración integral que agrupa agentes, almacenes RAG, clientes, servidores y herramientas de IA.
/// </summary>
public class ConfigSet()
{
    /// <summary>
    /// Obtiene o establece la colección de definiciones de agentes (<see cref="AgentDef"/>).
    /// </summary>
    public AgentDef[] AgentDefs { get; set; } = [];

    /// <summary>
    /// Obtiene o establece la colección de definiciones de almacenes RAG (<see cref="RaGdef"/>).
    /// </summary>
    public RaGdef[] RaGdefs { get; set; } = [];

    /// <summary>
    /// Obtiene o establece la colección de definiciones de clientes de IA (<see cref="ClientDef"/>).
    /// </summary>
    public ClientDef[] ClientDefs { get; set; } = [];

    /// <summary>
    /// Obtiene o establece la colección de definiciones de servidores y proveedores (<see cref="ServerDef"/>).
    /// </summary>
    public ServerDef[] ServerDefs { get; set; } = [];

    /// <summary>
    /// Obtiene o establece la colección de definiciones de herramientas de agentes (<see cref="AgentToolDef"/>).
    /// </summary>
    public AgentToolDef[] AgentToolDefs { get; set; } = [];
}