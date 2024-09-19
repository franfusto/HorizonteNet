using Horizonte;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Modulo2;

[HorizonteModule("Modulo2")]
public class PanelModulo
{
    private IHorizonteEnv _env;
    private ILogger<PanelModulo>? _logger;
    public PanelModulo(IHorizonteEnv env)
    {
        _env = env;
    }
    
    [HorizonteCommand("Modulo2_Command2")]
    public DateTime Command1()
    {
       _logger?.LogInformation("desde el módulo 2");
       return  DateTime.Now;
    }
    
    [HorizonteRole("init")]
    [HorizonteCommand("Modulo2_Init")]
    public bool Init()
    {
        _logger =_env.GetService<ILogger<PanelModulo>>();
       _logger?.LogInformation("Módulo 2 Inciciado");
       return true;
       
    }

    [HorizonteRole("configpage")]
    [HorizonteCommand("Modulo2_ConfigPage","Módulo 2")]
    public string ConfigPage() => typeof(Component2).AssemblyQualifiedName!;
    
    //public string ConfigPage() => "Modulo2.Componente2,Modulo2";


}