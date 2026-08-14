using System.Text;
using Gelf4Net;
using Horizonte;
using Horizonte.Extension.AiWorkFlows;
using Horizonte.Interfaces;
using Horizonte.WorkFlows.Widgets;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;

namespace Horizonte.WorkFlows;

/// <summary>
/// La clase PanelModulo pertenece al módulo Horizonte.WorkFlows y es responsable de manejar
/// la integración y funcionalidad del componente principal en la plataforma de workflows.
/// Esta clase incluye métodos para la inicialización del módulo y la gestión de widgets de visualización en el dashboard.
/// </summary>
[HorizonteModule("Horizonte.WorkFlows")]
public class PanelModulo
{
    /// <summary>
    /// AssemblyToPackageMap es un diccionario de solo lectura que mapea nombres de ensamblados a sus respectivos nombres de paquetes NuGet.
    /// Utiliza comparaciones de cadenas sin distinción de mayúsculas y minúsculas.
    /// Este mapeo es útil para resolver dependencias e identificar el paquete NuGet correspondiente a un ensamblado en particular,
    /// facilitando así la gestión de referencias en el módulo Horizonte.WorkFlows.
    /// </summary>
    [AssemblyMappingDefinition] public static readonly Dictionary<string, string> AssemblyToPackageMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Blazor.Diagrams"] = "Z.Blazor.Diagrams",
            ["Blazor.Diagrams.Core"] = "Z.Blazor.Diagrams.Core"
        };

    private readonly ILogger<PanelModulo> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IhContext _context;
    /// <summary>
    /// La clase PanelModulo pertenece al módulo Horizonte.WorkFlows y es responsable de manejar
    /// la integración y funcionalidad del componente principal en la plataforma de workflows.
    /// Esta clase incluye métodos para la inicialización del módulo y la gestión de widgets de visualización en el dashboard.
    /// </summary>
    public PanelModulo(ILogger<PanelModulo> logger, IhContext context,IServiceProvider serviceProvider)
    {
        _logger = logger;
        _context = context;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Inicializa el módulo Horizonte.WorkFlows. Este método es responsable de
    /// registrar el inicio del módulo y asegurar que esté listo para su operación
    /// en la plataforma de workflows.
    /// </summary>
    /// <returns>Devuelve un valor booleano que indica si el módulo se inició exitosamente.
    /// Siempre devuelve true, pues el proceso de inicialización se registra como exitoso
    /// mediante un mensaje de información en el logger.</returns>
    [HorizonteRole("init")]
    [HorizonteCommand("WorkFlows_Init")]
    public bool Init()
    {
        _logger.LogInformation("Módulo Horizonte.WorkFlows Iniciado");
        return true;
    }

    /// <summary>
    /// Define un widget que se utiliza para la visualización de scripts dentro del módulo
    /// Horizonte.WorkFlows. Este método devuelve una instancia de WidgetDef que está
    /// configurada para integrar el tipo de widget especificado en el panel de control
    /// de workflows.
    /// </summary>
    /// <returns>Devuelve un objeto de tipo WidgetDef que representa el widget de visor de
    /// scripts dentro del panel de control de workflows, estableciendo como tipo el
    /// componente Dashboard.</returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("WorkFlows_Dashboard", "")]
    public WidgetDef ScriptsViewerWidget() => new WidgetDef() { Type = typeof(Dashboard), Parameters = null };



    [HorizonteCommand("WorkFlows_ExecuteWorkFlow", "")]
    public async Task<object?> ExecuteWorkFlow(string workflowName, object message, CancellationToken ctoken, Action<WorkflowEvent>? callback)
    {
        var workflow =   _context.Get<WorkFlowConfig>().WorkFlows.FirstOrDefault(w => w.Name == workflowName);
        if(workflow == null) throw new Exception("Workflow not found");
        var result =await workflow.RunWorkFlow(_serviceProvider, message, ctoken, callback);
        return result;
    }

    
    [HorizonteCommand("WorkFlows_TestWorkFlow", "")]
    public async Task<string> TestWorkFlow()
    {
        try
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            string workflowName="SampleWorkFlow";
            var message = new Dictionary<string, string>();
            message.Add("entrada", "Hola Mundo");
            
            var res = await ExecuteWorkFlow(workflowName, message, cts.Token, null!);
            
            //var workflow =   _context.Get<WorkFlowConfig>().WorkFlows.FirstOrDefault(w => w.Name == workflowName);
            //if(workflow == null) throw new Exception("Workflow not found");
            //var result = await workflow.RunWorkFlow(_serviceProvider, message, cts.Token, null!);
            
            return  Newtonsoft.Json.JsonConvert.SerializeObject(res);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al ejecutar el workflow");
        }
        return string.Empty;
    }
}
