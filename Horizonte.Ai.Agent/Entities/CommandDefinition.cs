namespace Horizonte.Ai.Agent;

public class CommandDefinition
{
    public string CommandName = string.Empty;
    public string CommandDescription = string.Empty;
    public string[] CommandRoles = Array.Empty<string>();
    public List<CommandParameterDefinition> Parameters = new List<CommandParameterDefinition>(); 
    
}

public class CommandParameterDefinition
{
    public string Name = string.Empty;
    public string Type = string.Empty;
}