namespace Horizonte.Ai.MCPServer;

public class MCPServerConfig
{
    public List<MCPCommandDef> Commands { get; set; } = new();    
    
    public MCPServerConfig()
    {
        Commands.Add(new MCPCommandDef()
        {
            Command = "Horizonte_SytstemDateTime",
            Description = "Obtiene la fecha y hora del sistema"
        });
        Commands.Add(new MCPCommandDef()
        {
            Command = "Workers_GetAvailablesBackServices",
            Description = "Obtiene la lista de trabajadores disponibles en el sistema"
        });
        Commands.Add(new MCPCommandDef()
        {
            Command = "Sample_Encriptar",
            Description = ""
        });
        Commands.Add(new MCPCommandDef()
        {
            Command = "Sample_GetMeteoInfo",
            Description = ""
        });
    }
}

public class MCPCommandDef
{
    public string Command {get; set; }
    public string Description {get; set; }
}