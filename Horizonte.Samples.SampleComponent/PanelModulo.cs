using Horizonte;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Modulo2;

/// <summary>
/// La clase PanelModulo representa un componente modular dentro del marco Horizonte.
/// Este módulo está destinado a ser utilizado como parte de la estructura de gestión de componentes de Horizonte,
/// proporcionando funcionalidad específica a través de comandos y roles.
/// </summary>
[HorizonteModule("Horizonte.Samples.SampleComponent")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;

    /// <summary>
    /// La clase PanelModulo representa un componente modular en el marco Horizonte.
    /// Está equipada con varios comandos y roles que permiten su inicialización, configuración y ejecución de comandos específicos.
    /// </summary>
    public PanelModulo(ILogger<PanelModulo> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Método asociado al comando "Modulo2_Command2" en el contexto del módulo Horizonte.
    /// Registra un mensaje de información en el log y devuelve la fecha y hora actual.
    /// </summary>
    /// <returns>
    /// La fecha y hora actuales como un objeto DateTime.
    /// </returns>
    [HorizonteCommand("Modulo2_Command2")]
    public DateTime Command1()
    {
       _logger.LogInformation("desde el módulo 2");
       return  DateTime.Now;
    }

    /// <summary>
    /// Método que inicia el Módulo 2, registrando en el log un mensaje informativo sobre el inicio del módulo.
    /// </summary>
    /// <returns>Retorna un valor booleano <c>true</c> indicando que el módulo ha sido iniciado exitosamente.</returns>
    [HorizonteRole("init")]
    [HorizonteCommand("Modulo2_Init")]
    public bool Init()
    {
       _logger.LogInformation("Módulo 2 Iniciado");
       return true;
    }

    /// <summary>
    /// Método que devuelve el nombre calificado del ensamblado del componente <see cref="Component2"/>.
    /// Este método está asociado al rol "configpage" y se define como un comando en Horizonte bajo la clave "Modulo2_ConfigPage" con la descripción "Módulo 2".
    /// </summary>
    /// <returns>El nombre calificado del ensamblado del componente <see cref="Component2"/>.</returns>
    [HorizonteRole("configpage")]
    [HorizonteCommand("Modulo2_ConfigPage","Módulo 2")]
    public string ConfigPage() => typeof(Component2).AssemblyQualifiedName!;
    
    //public string ConfigPage() => "Horizonte.Samples.SampleComponent.Componente2,Horizonte.Samples.SampleComponent";


}