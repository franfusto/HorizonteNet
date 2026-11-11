using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Horizonte.Extension.AiWorkFlows;
namespace Horizonte.WorkFlows.NodeModels;

/// <summary>
/// La clase <c>NodeInModel</c> representa un modelo de nodo dentro de un flujo de trabajo.
/// Esta clase hereda de <c>NodeModel</c> y implementa la interfaz <c>IRunnableNode</c>.
/// Es utilizada para encapsular la lógica y datos necesarios para un nodo específico en un modelo de diagrama.
/// </summary>
public class NodeInModel : NodeModel,IRunnableNode
{
    /// <summary>
    /// Representa un nodo individual dentro de un flujo de trabajo de AI.
    /// Este nodo contiene información relevante del flujo de trabajo específica del nodo y está asociado con un modelo subyacente de ejecución.
    /// </summary>
    public WorkFlowNode Node { get; }

    /// <summary>
    /// Obtiene o establece el mensaje o estado asociado a la ejecución del nodo de entrada.
    /// </summary>
    public required string RunMessage { get; set; }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="NodeInModel"/> a partir de la definición de un nodo.
    /// </summary>
    /// <param name="node">Definición del nodo (<see cref="WorkFlowNode"/>) que contiene la configuración y posición inicial.</param>
    public NodeInModel(WorkFlowNode node) : base(node.Id, new Point(node.PosX, node.PosY))
    {
        Node = node;    
    } 
}