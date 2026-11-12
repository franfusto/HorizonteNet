namespace Horizonte.AdminUi.HorizonteSettings.Entidades;

/// <summary>
/// Enumeración que define los diferentes tipos de disposición disponibles para un diagrama.
/// </summary>
public enum DiagramLayout
{
    /// <summary>
    /// Disposición jerárquica o en capas (algoritmo Sugiyama).
    /// </summary>
    Layered,

    /// <summary>
    /// Disposición basada en escalado multidimensional (MDS).
    /// </summary>
    Mds,

    /// <summary>
    /// Disposición incremental rápida evitando solapamiento entre nodos.
    /// </summary>
    Incremental
}