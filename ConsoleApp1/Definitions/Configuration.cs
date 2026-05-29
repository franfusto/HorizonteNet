namespace ConsoleApp1.Definitions;

public class Configuration()
{
    public AgentDef[] AgentDefs { get; set; } = []; 
    public RaGdef[] RaGdefs {get;set;}= [];
    public ClientDef[] ClientDefs {get;set;}= [];
    public ServerDef[] ServerDefs { get; set; }= [];
}