namespace Horizonte.WorkFlows.NodeModels;

/// <summary>
/// Define la interfaz para nodos que pueden ser ejecutados y mostrar mensajes o estados de ejecución en la interfaz.
/// </summary>
public interface IRunnableNode
{
    /// <summary>
    /// Obtiene o establece el mensaje o estado asociado a la ejecución del nodo.
    /// </summary>
    public string RunMessage { get; set; }
}