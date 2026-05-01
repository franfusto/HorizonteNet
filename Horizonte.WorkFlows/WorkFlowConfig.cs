using Horizonte.Extension.AiWorkFlows;

namespace Horizonte.WorkFlows;

public class WorkFlowConfig
{
    public WorkFlowConfig()
    {
        // add samples
        WorkFlows.Add(new WorkFlowDef());
        WorkFlows[0].Name = "Sample Workflow";
    }
    public List<WorkFlowDef> WorkFlows { get; set; } = new List<WorkFlowDef>();
}