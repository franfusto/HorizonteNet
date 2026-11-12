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
    /// Inicializa una nueva instancia de la clase <see cref="PanelModulo"/>.
    /// </summary>
    /// <param name="logger">El servicio de registro de eventos para el módulo.</param>
    /// <param name="context">El contexto principal de Horizonte para acceso a estado y configuración.</param>
    /// <param name="serviceProvider">El proveedor de servicios de inyección de dependencias para la ejecución de flujos.</param>
    public PanelModulo(ILogger<PanelModulo> logger, IhContext context, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _context = context;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Inicializa el módulo Horizonte.WorkFlows.
    /// </summary>
    /// <returns>Devuelve <c>true</c> si el módulo se inicializó correctamente.</returns>
    [HorizonteRole("init")]
    [HorizonteCommand("WorkFlows_Init")]
    public bool Init()
    {
        _logger.LogInformation("Módulo Horizonte.WorkFlows Iniciado");
        return true;
    }

    /// <summary>
    /// Obtiene la definición del widget de panel de control (Dashboard) para la gestión y diseño de flujos de trabajo.
    /// </summary>
    /// <returns>Una instancia de <see cref="WidgetDef"/> configurada con el componente <see cref="Dashboard"/>.</returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("WorkFlows_Dashboard", "")]
    public WidgetDef ScriptsViewerWidget() => new WidgetDef() { Type = typeof(Dashboard), Parameters = null };

    /// <summary>
    /// Ejecuta un flujo de trabajo por su nombre pasando un mensaje de entrada y manejando eventos de callback.
    /// </summary>
    /// <param name="workflowName">Nombre del flujo de trabajo a ejecutar.</param>
    /// <param name="message">Mensaje u objeto de entrada para el flujo de trabajo.</param>
    /// <param name="ctoken">Token de cancelación para abortar la ejecución.</param>
    /// <param name="callback">Acción de callback para recibir eventos del flujo de trabajo durante su ejecución.</param>
    /// <returns>El resultado producido por el flujo de trabajo tras su ejecución.</returns>
    /// <exception cref="Exception">Se lanza si el flujo de trabajo especificado no existe en la configuración.</exception>
    [HorizonteCommand("WorkFlows_ExecuteWorkFlow", "")]
    public async Task<object?> ExecuteWorkFlow(string workflowName, object message, CancellationToken ctoken, Action<WorkflowEvent>? callback)
    {
        var workflow = _context.Get<WorkFlowConfig>()?.WorkFlows.FirstOrDefault(w => w.Name == workflowName);
        if (workflow == null) throw new Exception("Workflow not found");
        var result = await workflow.RunWorkFlow(_serviceProvider, message, ctoken, callback);
        return result;
    }

    /// <summary>
    /// Ejecuta una prueba de flujo de trabajo de ejemplo con un mensaje predeterminado.
    /// </summary>
    /// <returns>Una cadena JSON con el resultado serializado de la ejecución del flujo de trabajo.</returns>
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
