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
}

public class WorkFlowLink
{
    
}