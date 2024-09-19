namespace Horizonte;

public interface IHservice
{
    public string ServiceName { get; set; }
    public bool IsRunning { get; set; } 
    public bool RunOnStart { get; set; } 
}