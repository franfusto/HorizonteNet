using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Horizonte.Extension.AiWorkFlows;
namespace Horizonte.WorkFlows.NodeModels;

/// <summary>
/// La clase NodeOutModel representa un nodo de salida en el contexto de un diagrama de flujo de trabajo.
/// </summary>
/// <remarks>
/// Esta clase hereda de NodeModel e implementa la interfaz IRunnableNode, lo cual sugiere que el nodo no solo es una entidad gráfica en el diagrama sino que también tiene capacidades de ejecución.
/// </remarks>
public class NodeOutModel : NodeModel,IRunnableNode
{
    /// <summary>
    /// Propiedad que representa un nodo de flujo de trabajo en el modelo.
    /// Tipicamente se utiliza para definir la configuración y características
    /// de un nodo específico dentro de un diagrama de flujo.
    /// </summary>
    public WorkFlowNode Node { get; }

    /// <summary>
    /// La propiedad <c>RunMessage</c> representa un mensaje asociado a la ejecución de un nodo dentro del modelo <c>NodeOutModel</c>.
    /// Esta propiedad es utilizada para almacenar y recuperar un mensaje que contiene información relevante acerca de la ejecución
    /// de un nodo en un flujo de trabajo. Esto puede incluir mensajes de error, instrucciones de ejecución, o cualquier otro tipo de
    /// información que necesite ser comunicada al usuario o al sistema.
    /// </summary>
    public string RunMessage {get; set;}


    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="NodeOutModel"/> a partir de la definición de un nodo.
    /// </summary>
    /// <param name="node">
    /// Una instancia de <c>WorkFlowNode</c> que provee la información necesaria para
    /// inicializar el modelo de nodo, incluyendo su identificador único y posición.
    /// </param>
    public NodeOutModel(WorkFlowNode node) : base(node.Id, new Point(node.PosX, node.PosY))
    {
        Node = node;  
    }
}