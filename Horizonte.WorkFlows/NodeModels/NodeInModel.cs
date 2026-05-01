using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Horizonte.Extension.AiWorkFlows;
namespace Horizonte.WorkFlows.NodeModels;

/// <summary>
/// 
/// </summary>
public class NodeInModel : NodeModel,IRunnableNode
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
    public NodeInModel(WorkFlowNode node) : base(node.Id, new Point(node.PosX, node.PosY))
    {
        Node = node;    
    }
}