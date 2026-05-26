using Horizonte;
using Horizonte.Extensions.Interactive;
using Horizonte.Interfaces;
using Horizonte.Scripts.Widgets;
using Microsoft.Extensions.Logging;

namespace Horizonte.Scripts;

[HorizonteModule("Horizonte.Scripts")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Clase que representa un módulo denominado "PanelModulo" dentro de la aplicación Horizonte.
    /// Este módulo está diseñado para gestionar y configurar funcionalidades específicas dentro del entorno
    /// modular de Horizonte, tales como inicializar servicios y registrar widgets personalizados.
    /// </summary>
    public PanelModulo(ILogger<PanelModulo> logger, IhContext context, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _context = context;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Método encargado de inicializar el módulo "Scripts" en la aplicación Horizonte.
    /// Este método configura el logger y otros servicios necesarios para el correcto funcionamiento
    /// del módulo y registra un mensaje de información en los logs.
    /// </summary>
    /// <returns>
    /// Devuelve un valor booleano que indica si el módulo fue iniciado correctamente.
    /// </returns>
    [HorizonteRole("init")]
    [HorizonteCommand("Scripts_Init")]
    public bool Init()
    {
        var config = _context.Get<ScriptsConfig>() ?? new ScriptsConfig();
        LoadScriptModules(config);
        _logger.LogInformation("Módulo Scripts Iniciado");
        return true;
    }


    /// <summary>
    /// Método que devuelve la configuración del widget de Jupyter Notebook
    /// para su uso en el módulo "Scripts" de la aplicación Horizonte.
    /// Este método configura los parámetros iniciales y define el tipo del widget
    /// asociado al entorno de ejecución de Jupyter Notebook.
    /// </summary>
    /// <returns>
    /// Devuelve una instancia de `WidgetDef` que especifica la configuración del widget
    /// para la integración con la funcionalidad de Jupyter Notebook.
    /// </returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("Scripts_JupyterNotebookWidget", "pruebas iniciales")]
    public WidgetDef JupyterNotebookWidget() =>
        new WidgetDef() { Type = typeof(JupyterNotebookWidget), Parameters = null };

    /// <summary>
    /// Método encargado de proporcionar la definición del widget "ScriptsViewerWidget" en el contexto del módulo "Scripts".
    /// Este widget permite visualizar, gestionar y modificar scripts configurados dentro de la aplicación Horizonte.
    /// </summary>
    /// <returns>
    /// Devuelve una instancia de <c>WidgetDef</c> que define el widget asociado, incluyendo su tipo y parámetros de inicialización.
    /// </returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("Scripts_ScriptsViewerWidget", "pruebas iniciales")]
    public WidgetDef ScriptsViewerWidget() => new WidgetDef() { Type = typeof(ScriptsViewerWidget), Parameters = null };
    
    private void LoadScriptModules(ScriptsConfig config)
    {
        var scripts = config.Scripts;
        scripts.LoadScriptModules(_serviceProvider);

    }
}