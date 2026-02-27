using Horizonte.Extension.WorkFlows;

namespace Horizonte.WorkFlows;

public class WorkFlowConfig
{
    WorkFlowConfig()
    {
        // add samples
        WorkFlows.Add(new WorkFlowDef());
        WorkFlows[0].Name = "Sample Workflow";
    }
    List<WorkFlowDef> WorkFlows { get; set; } = new List<WorkFlowDef>();
}