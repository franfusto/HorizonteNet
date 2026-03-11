using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Horizonte.Extension.WorkFlows;

namespace Horizonte.WorkFlows.NodeModels;

/// <summary>
/// 
/// </summary>
public class NodeOperationModel : NodeModel,IRunnableNode
{
    /// <summary>
    /// 
    /// </summary>
    public WorkFlowNode Node { get; }

    public string RunMessage {get; set;}
    
    /// <summary>
    /// Evento emitido cuando cambia el número de puertos.
    /// (Alignment, Count)
    /// </summary>
    public event Action<PortAlignment, int>? OnPortsChanged;

    public void NotifyPortsChanged(PortAlignment alignment, int count)
    {
        OnPortsChanged?.Invoke(alignment, count);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    public NodeOperationModel(WorkFlowNode node) : base(node.Id, new Point(node.PosX, node.PosY))
    {
        Node = node;  
    }
}