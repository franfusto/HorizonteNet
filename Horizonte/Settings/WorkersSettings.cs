namespace Horizonte;

[HConfigOption]
public class WorkerSettings
{
    public WorkerSettings()
    {
        List.Add(new WorkerSettingsItem()
        {
            ServiceName = "HorizonteNet GUI",
            WorkerType = "HorizonteGui.Worker,HorizonteGui",
            RunOnStart = true
        });
        List.Add(new WorkerSettingsItem()
        {
            ServiceName = "Fake Worker Contador",
            WorkerType = "Horizonte.FakeWorker,Horizonte",
            RunOnStart = true
        });
        List.Add(new WorkerSettingsItem()
        {
            ServiceName = "Start/End Worker",
            WorkerType = "Horizonte.StartEndWorker,Horizonte",
            RunOnStart = true
        }); 
    }
    public List<WorkerSettingsItem> List { get; set; } = [];
}

public class WorkerSettingsItem
{
    public string WorkerType { get; set; } = string.Empty;
    public bool RunOnStart { get; set; } = false;
    public string ServiceName { get; set; } = string.Empty;
}