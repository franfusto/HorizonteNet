using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Horizonte.Extension.WorkFlows;
namespace Horizonte.WorkFlows.NodeModels;

/// <summary>
/// 
/// </summary>
public class NodeOutModel : NodeModel,IRunnableNode
{
    /// <summary>
    /// 
    /// </summary>
    public WorkFlowNode Node { get; }

    public string RunMessage {get; set;}
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    public NodeOutModel(WorkFlowNode node) : base(node.Id, new Point(node.PosX, node.PosY))
    {
        Node = node;  
    }
}