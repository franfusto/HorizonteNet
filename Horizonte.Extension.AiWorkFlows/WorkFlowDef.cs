namespace Horizonte.Extension.AiWorkFlows;

/// <summary>
/// Define la estructura y configuración de un flujo de trabajo ejecutable por el motor de workflows de IA.
/// </summary>
public class WorkFlowDef
{
    /// <summary>
    /// Obtiene o establece el identificador único del flujo de trabajo.
    /// </summary>
    public string Id { get; set; } = Guid.Empty.ToString();

    /// <summary>
    /// Obtiene o establece el nombre descriptivo del flujo de trabajo.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre calificado del tipo de mensaje de datos que procesa el flujo (por defecto "System.String").
    /// </summary>
    public string MessageType { get; set; } = "System.String";

    /// <summary>
    /// Obtiene o establece el nombre del comando o procesador que gestiona los errores que ocurren en el flujo.
    /// </summary>
    public string? FlowErrorProcessor { get; set; }

    /// <summary>
    /// Obtiene o establece el nombre del comando o procesador que gestiona las cancelaciones en el flujo.
    /// </summary>
    public string? FlowCancelProcessor { get; set; }

    /// <summary>
    /// Obtiene o establece la lista de nodos (<see cref="WorkFlowNode"/>) que componen el flujo de trabajo.
    /// </summary>
    public List<WorkFlowNode> Nodes { get; set; } = new List<WorkFlowNode>();

    /// <summary>
    /// Obtiene o establece la lista de enlaces (<see cref="WorkFlowLink"/>) que interconectan los nodos del flujo.
    /// </summary>
    public List<WorkFlowLink> Links { get; set; } = new List<WorkFlowLink>();
}

/// <summary>
/// Representa un nodo dentro de una definición de flujo de trabajo.
/// </summary>
public class WorkFlowNode
{
    /// <summary>
    /// Obtiene o establece el identificador único del nodo.
    /// </summary>
    public string Id { get; set; } = Guid.Empty.ToString();

    /// <summary>
    /// Obtiene o establece el nombre descriptivo del nodo.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el tipo de nodo (<see cref="WorkFlowNodeTypes"/>): entrada, nodo de operación o salida.
    /// </summary>
    public WorkFlowNodeTypes Type { get; set; } = WorkFlowNodeTypes.Node;

    /// <summary>
    /// Obtiene o establece la posición X del nodo en el lienzo visual del diseñador.
    /// </summary>
    public int PosX { get; set; } = 0;

    /// <summary>
    /// Obtiene o establece la posición Y del nodo en el lienzo visual del diseñador.
    /// </summary>
    public int PosY { get; set; } = 0;

    /// <summary>
    /// Obtiene o establece la colección de puertos de entrada del nodo.
    /// </summary>
    public NodePort[] InPorts { get; set; } = Array.Empty<NodePort>();

    /// <summary>
    /// Obtiene o establece la colección de puertos de salida del nodo.
    /// </summary>
    public NodePort[] OutPorts { get; set; } = Array.Empty<NodePort>();

    /// <summary>
    /// Obtiene o establece el nombre del comando de Horizonte que ejecuta la acción asociada al nodo.
    /// </summary>
    public string CommandAction { get; set; } = "";
}

/// <summary>
/// Representa un enlace de conexión entre dos nodos en una definición de flujo de trabajo.
/// </summary>
public class WorkFlowLink
{
    /// <summary>
    /// Obtiene o establece el identificador único del enlace.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Obtiene o establece el identificador del nodo origen de la conexión.
    /// </summary>
    public string SourceNodeId { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el identificador del puerto de salida origen.
    /// </summary>
    public string SourcePortId { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el identificador del nodo destino de la conexión.
    /// </summary>
    public string TargetNodeId { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el identificador del puerto de entrada destino.
    /// </summary>
    public string TargetPortId { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la condición evaluada (<see cref="LinkCondition"/>) para habilitar el tránsito por este enlace.
    /// </summary>
    public LinkCondition? Condition { get; set; }
}

/// <summary>
/// Define la condición asociada a un enlace de flujo de trabajo para controlar la ramificación condicional.
/// </summary>
public class LinkCondition
{
    /// <summary>
    /// Obtiene o establece el nombre descriptivo de la condición.
    /// </summary>
    public string ConditionName { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre del comando booleano de Horizonte que evalúa la condición.
    /// </summary>
    public string ConditionCommand { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el valor booleano esperado que debe devolver el comando para que la condición se cumpla.
    /// </summary>
    public bool ExpectedValue { get; set; } = false;
}

/// <summary>
/// Representa un puerto de entrada o salida en un nodo de flujo de trabajo.
/// </summary>
public class NodePort
{
    /// <summary>
    /// Obtiene o establece el identificador único del puerto.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Obtiene o establece el nombre del puerto.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la alineación o posición del puerto en el nodo (e.g., izquierda, derecha).
    /// </summary>
    public string Alignment { get; set; } = string.Empty;
}

/// <summary>
/// Especifica los roles o tipos funcionales de los nodos en un flujo de trabajo.
/// </summary>
public enum WorkFlowNodeTypes
{
    /// <summary>
    /// Nodo de entrada que recibe el mensaje inicial del flujo.
    /// </summary>
    In,

    /// <summary>
    /// Nodo intermedio de procesamiento u operación.
    /// </summary>
    Node,

    /// <summary>
    /// Nodo de salida o terminación que emite el resultado final del flujo.
    /// </summary>
    Out
}