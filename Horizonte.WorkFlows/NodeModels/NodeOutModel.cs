using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Horizonte.Extension.WorkFlows;
namespace Horizonte.WorkFlows.NodeModels;

public class NodeOutModel : NodeModel
{
    public NodeOutModel(WorkFlowNode node) : base(node.Id, new Point(node.PosX, node.PosY))
    {
        
    }
}