using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Horizonte.Extension.AiWorkFlows;

namespace Horizonte.WorkFlows.NodeModels;

/// <summary>
/// La clase <c>NodeOperationModel</c> representa un nodo operativo en un flujo de trabajo dentro del sistema Horizonte.
/// Hereda de <c>NodeModel</c> e implementa la interfaz <c>IRunnableNode</c>.
/// </summary>
/// <remarks>
/// Esta clase es parte del espacio de nombres <c>Horizonte.WorkFlows.NodeModels</c> y está diseñada para integrarse en la lógica de diagramas y flujos de trabajo.
/// Ofrece un mecanismo para manejar el cambio de puertos a través del evento <c>OnPortsChanged</c>.
/// </remarks>
public class NodeOperationModel : NodeModel,IRunnableNode
{
    /// <summary>
    /// Propiedad que representa un nodo de flujo de trabajo en el modelo de operación de nodo.
    /// Proporciona acceso a la configuración y estado del nodo asociado, incluyendo su posición y puertos.
    /// </summary>
    public WorkFlowNode Node { get; }

    /// <summary>
    /// Mensaje de ejecución asociado al nodo. Se utiliza para proporcionar detalles o registros sobre el proceso de ejecución de un nodo individual.
    /// </summary>
    public string RunMessage {get; set;}

    /// <summary>
    /// Evento disparado cuando ocurre un cambio en los puertos.
    /// Se proporcionan el modelo del nodo, el alineamiento del puerto y el número total de puertos como parámetros.
    /// </summary>
    public event Action<NodeOperationModel, PortAlignment, int>? OnPortsChanged;

    /// <summary>
    /// Notifica el cambio en los puertos del nodo, activando el evento asociado a dicho cambio.
    /// </summary>
    /// <param name="alignment">Indica la alineación de los puertos que han cambiado, puede ser izquierda, derecha u otra alineación definida en <c>PortAlignment</c>.</param>
    /// <param name="count">Especifica la cantidad de puertos implicados en el cambio.</param>
    public void NotifyPortsChanged(PortAlignment alignment, int count)
    {
        OnPortsChanged?.Invoke(this, alignment, count);
    }

    /// <summary>
    /// Representa un modelo de operación de nodo dentro de un flujo de trabajo.
    /// </summary>
    /// <remarks>
    /// Esta clase hereda de <c>NodeModel</c> e implementa la interfaz <c>IRunnableNode</c>,
    /// proporcionando la funcionalidad para representar nodos que pueden ser ejecutados
    /// dentro de un flujo de trabajo.
    /// </remarks>
    public NodeOperationModel(WorkFlowNode node) : base(node.Id, new Point(node.PosX, node.PosY))
    {
        Node = node;  
    }
}