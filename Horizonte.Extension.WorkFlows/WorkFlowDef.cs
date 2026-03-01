namespace Horizonte.Extension.WorkFlows;

public class WorkFlowDef
{
    public string Id { get; set; } = Guid.Empty.ToString();
    public string Name { get; set; }
    public List<WorkFlowNode> Nodes { get; set; } = new List<WorkFlowNode>();
    public List<WorkFlowLink> Links { get; set; } = new List<WorkFlowLink>();
    
}

public class WorkFlowNode
{
    public string Id { get; set; } = Guid.Empty.ToString();
    public string Name { get; set; }
    public WorkFlowNodeTypes Type { get; set; } = WorkFlowNodeTypes.Node;
    public int PosX { get; set; } = 0;
    public int PosY { get; set; } = 0;
    public NodePort[] InPorts { get; set; } = Array.Empty<NodePort>();
    public NodePort[] OutPorts { get; set; } = Array.Empty<NodePort>();
    public string CommandAction { get; set; } = "";
    
}

public class WorkFlowLink
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SourceNodeId { get; set; }
    public string SourcePortId { get; set; }
    public string TargetNodeId { get; set; }
    public string TargetPortId { get; set; }
}

public class NodePort
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Alignment { get; set; }
}

public enum WorkFlowNodeTypes
{
    In,
    Node,
    Out
}