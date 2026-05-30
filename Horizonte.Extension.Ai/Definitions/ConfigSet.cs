namespace Horizonte.Extension.Ai.Definitions;

public class ConfigSet()
{
    public AgentDef[] AgentDefs { get; set; } = []; 
    public RaGdef[] RaGdefs {get;set;}= [];
    public ClientDef[] ClientDefs {get;set;}= [];
    public ServerDef[] ServerDefs { get; set; }= [];
    public AgentToolDef[] AgentToolDefs { get; set; } = [];
}