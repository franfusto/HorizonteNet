using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Horizonte.Extension.WorkFlows;

namespace Horizonte.WorkFlows.NodeModels;

/// <summary>
/// 
/// </summary>
public class NodeOperationModel : NodeModel
{
    /// <summary>
    /// 
    /// </summary>
    public WorkFlowNode Node { get; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    public NodeOperationModel(WorkFlowNode node) : base(node.Id, new Point(node.PosX, node.PosY))
    {
        Node = node;  
    }
}