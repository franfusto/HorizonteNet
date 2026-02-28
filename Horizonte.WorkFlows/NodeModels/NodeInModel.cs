using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Horizonte.Extension.WorkFlows;
namespace Horizonte.WorkFlows.NodeModels;

public class NodeInModel : NodeModel
{
    public NodeInModel(WorkFlowNode node) : base(node.Id, new Point(node.PosX, node.PosY))
    {
        
    }
}