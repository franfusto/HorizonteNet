using Horizonte;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Modulo1;

[HorizonteModule("Modulo1")]
public class PanelModulo
{
    private Lazy<IHorizonteEnv> _env;
    private ILogger<PanelModulo>? _logger;
    public PanelModulo(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
    }
    
    [HorizonteCommand("Modulo1_Command1")]
    public DateTime Command1()
    {
       _logger?.LogInformation("desde el módulo 1 - command1");
       return  DateTime.Now;
    }   
    [HorizonteCommand("Modulo1_Command2")]
    public string Command2(string value)
    {
       _logger?.LogInformation("desde el módulo 1 - command2");
       return  value+"-2.0.0";
    }
    
    [HorizonteRole("init")]
    [HorizonteCommand("Modulo1_Init")]
    public bool Init()
    {
       _logger =_env.Value.GetService<ILogger<PanelModulo>>();
       _logger?.LogInformation("Módulo 1 Inciciado");
       return true;
    }

    [HorizonteRole("configpage")]
    [HorizonteCommand("Modulo1_ConfigPage","Módulo 1")]
    public string ConfigPage() => typeof(Component1).AssemblyQualifiedName!;

}