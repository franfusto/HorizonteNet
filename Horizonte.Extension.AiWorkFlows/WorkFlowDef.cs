namespace Horizonte.Extension.AiWorkFlows;

public class WorkFlowDef
{
    public string Id { get; set; } = Guid.Empty.ToString();
    public string Name { get; set; }= string.Empty;
    public List<WorkFlowNode> Nodes { get; set; } = new List<WorkFlowNode>();
    public List<WorkFlowLink> Links { get; set; } = new List<WorkFlowLink>();
    
}

public class WorkFlowNode
{
    public string Id { get; set; } = Guid.Empty.ToString();
    public string Name { get; set; }= string.Empty;
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
    public string SourceNodeId { get; set; }= string.Empty;
    public string SourcePortId { get; set; }= string.Empty;
    public string TargetNodeId { get; set; }= string.Empty;
    public string TargetPortId { get; set; }= string.Empty;
    public LinkCondition? Condition { get; set; }
}

public class LinkCondition
{
    public string ConditionName { get; set; }= string.Empty;
    public string ConditionCommand { get; set; }= string.Empty;
}

public class NodePort
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }= string.Empty;
    public string Alignment { get; set; }= string.Empty;
}

public enum WorkFlowNodeTypes
{
    In,
    Node,
    Out
}