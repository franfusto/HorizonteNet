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

    /// La propiedad `RunMessage` es de tipo `string` y forma parte de la clase `NodeInModel` en el espacio de nombres `Horizonte.WorkFlows.NodeModels`.
    /// Esta propiedad se utiliza para almacenar un mensaje asociado con la ejecución de un nodo que implementa la interfaz `IRunnableNode`.
    /// El valor de `RunMessage` puede ser utilizado para proporcionar retroalimentación sobre el proceso de ejecución del nodo o para indicar su estado actual.
    /// /
    public required string RunMessage {get; set;}

    /// <summary>
    /// La clase <c>NodeInModel</c> representa un modelo de nodo dentro de un flujo de trabajo.
    /// Hereda de la clase <c>NodeModel</c> e implementa la interfaz <c>IRunnableNode</c>.
    /// </summary>
    /// <remarks>
    /// La clase está diseñada para ser utilizada en entornos de diagramación, específicamente dentro de un modelo de flujo de trabajo.
    /// Se inicializa con un objeto <c>WorkFlowNode</c> que proporciona el identificador único y las coordenadas de posición del nodo.
    /// </remarks>
    public  NodeInModel(WorkFlowNode node) : base(node.Id, new Point(node.PosX, node.PosY))
    {
        Node = node;    
    } 
}