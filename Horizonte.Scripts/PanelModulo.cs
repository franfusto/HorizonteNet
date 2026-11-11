using Horizonte;
using Horizonte.AdminUi;
using Horizonte.AdminUi.Attributes;
using Horizonte.Extensions.Interactive;
using Horizonte.Interfaces;
using Horizonte.Scripts.Config;
using Horizonte.Scripts.Services;
using Horizonte.Scripts.Widgets;
using Microsoft.Extensions.Logging;

namespace Horizonte.Scripts;

/// <summary>
/// Representa el módulo de gestión de scripts y notebooks interactivos dentro del marco Horizonte.
/// </summary>
/// <remarks>
/// Identificado mediante el atributo <see cref="HorizonteModule"/>, este módulo inicializa los servicios de ejecución de scripts,
/// registra los servicios personalizados para autocompletado y expone widgets para la visualización y edición de scripts y cuadernos Jupyter.
/// </remarks>
[HorizonteModule("Horizonte.Scripts")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Definición de un servicio personalizado en la interfaz de usuario de administración para registrar <see cref="IScriptCompletionService"/>.
    /// </summary>
    [AdminUiCustomServiceDefinition]
    public static Horizonte.AdminUi.CustomServiceConfig ccc = new ()
    {
        ServiceType = "Horizonte.Scripts.Services.IScriptCompletionService, Horizonte.Scripts",
        ImplementationType = "Horizonte.Scripts.Services.ScriptCompletionService, Horizonte.Scripts",
        Lifetime = CustomServiceLifetime.Scoped
    };
    
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PanelModulo"/>.
    /// </summary>
    /// <param name="logger">Instancia de <see cref="ILogger{TCategoryName}"/> utilizada para registrar eventos y diagnósticos del módulo.</param>
    /// <param name="context">Instancia de <see cref="IhContext"/> que proporciona acceso al contexto y configuración de Horizonte.</param>
    /// <param name="serviceProvider">Proveedor de servicios para la resolución de dependencias requeridas en la carga de módulos.</param>
    public PanelModulo(ILogger<PanelModulo> logger, IhContext context, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _context = context;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Inicializa el módulo de Scripts en el marco Horizonte.
    /// </summary>
    /// <remarks>
    /// Carga y compila los módulos de script configurados en <see cref="ScriptsConfig"/> y registra un mensaje informativo en el log.
    /// </remarks>
    /// <returns><c>true</c> si el módulo se inicializa correctamente.</returns>
    [HorizonteRole("init")]
    [HorizonteCommand("Scripts_Init")]
    public bool Init()
    {
        var config = _context.Get<ScriptsConfig>() ?? new ScriptsConfig();
        LoadScriptModules(config);
        _logger.LogInformation("Módulo Scripts Iniciado");
        
        //referenciamos BlazorMonaco para cargar nuget con assets necesarios para la web.
        _ = new BlazorMonaco.MarkdownString();
        
        return true;
    }


    /// <summary>
    /// Proporciona la definición del widget para la visualización y ejecución de cuadernos Jupyter.
    /// </summary>
    /// <returns>
    /// Una instancia de <see cref="WidgetDef"/> que define el tipo de componente <see cref="JupyterNotebookWidget"/>.
    /// </returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("Scripts_JupyterNotebookWidget", "pruebas iniciales")]
    public WidgetDef JupyterNotebookWidget() =>
        new WidgetDef() { Type = typeof(JupyterNotebookWidget), Parameters = null };

    /// <summary>
    /// Proporciona la definición del widget para visualizar, gestionar y compilar scripts de C# en tiempo de ejecución.
    /// </summary>
    /// <returns>
    /// Una instancia de <see cref="WidgetDef"/> que define el tipo de componente <see cref="ScriptsViewerWidget"/>.
    /// </returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("Scripts_ScriptsViewerWidget", "pruebas iniciales")]
    public WidgetDef ScriptsViewerWidget() => new WidgetDef() { Type = typeof(ScriptsViewerWidget), Parameters = null };
    
    private void LoadScriptModules(ScriptsConfig config)
    {
        var scripts = config.Scripts;
        _ = scripts.LoadScriptModules(_serviceProvider);

    }
}