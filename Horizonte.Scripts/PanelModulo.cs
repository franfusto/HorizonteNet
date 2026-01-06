using Horizonte;
using Horizonte.Scripts.Widgets;
using Microsoft.Extensions.Logging;

namespace Horizonte.Scripts;

[HorizonteModule("Horizonte.Scripts")]
public class PanelModulo
{
    private ILogger<PanelModulo>? _logger;
    private Lazy<IHorizonteEnv> _env;
    
    public PanelModulo(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
    }
    
    [HorizonteRole("init")]
    [HorizonteCommand("Scripts_Init")]
    public bool Init()
    {
        _logger = _env.Value.GetService<ILogger<PanelModulo>>();
        var context = _env.Value.GetService<IHContext>();
        _logger?.LogInformation("Módulo Scripts Inciciado");
        
        
        return true;
    }
    
    [HorizonteRole("widget")]
    [HorizonteCommand("Scripts_JupyterNotebookViewer","pruebas iniciales")]
    public WidgetDef JupyterNotebookViewer()  => new WidgetDef() { Type = typeof(JupyterNotebookViewer), Parameters = null };    
    
    [HorizonteRole("widget")]
    [HorizonteCommand("Scripts_ScriptsViewer","pruebas iniciales")]
    public WidgetDef ScriptsViewer()  => new WidgetDef() { Type = typeof(ScriptsViewer), Parameters = null };
    
    [HorizonteRole("widget")]
    [HorizonteCommand("Scripts_JupyterNotebookWidget","pruebas iniciales")]
    public WidgetDef JupyterNotebookWidget()  => new WidgetDef() { Type = typeof(JupyterNotebookWidget), Parameters = null };    
    
    [HorizonteRole("widget")]
    [HorizonteCommand("Scripts_ScriptsViewerWidget","pruebas iniciales")]
    public WidgetDef ScriptsViewerWidget()  => new WidgetDef() { Type = typeof(ScriptsViewerWidget), Parameters = null };
}