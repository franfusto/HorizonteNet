using Horizonte;
using Horizonte.Interfaces;
using Horizonte.PorcupineMod.Widgets;
using Microsoft.Extensions.Logging;

namespace Horizonte.PorcupineMod;

/// <summary>
/// La clase PanelModulo representa un módulo en el espacio de nombres Horizonte.PorcupineMod.
/// Proporciona métodos para inicializar el módulo y obtener la configuración del widget asociado.
/// </summary>
[HorizonteModule("Horizonte.PorcupineMod")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;

    /// <summary>
    /// La clase PanelModulo representa un módulo en el espacio de nombres Horizonte.PorcupineMod.
    /// Proporciona métodos para inicializar el módulo y obtener la configuración del widget asociado.
    /// </summary>
    public PanelModulo(ILogger<PanelModulo> logger, IhContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Método responsable de iniciar el módulo Porcupine.
    /// </summary>
    /// <remarks>
    /// Este método utiliza un logger para registrar un mensaje informativo indicando que el módulo Porcupine ha sido iniciado.
    /// </remarks>
    /// <horizonterole>
    /// Esta función requiere el rol "init" para ser ejecutada.
    /// </horizonterole>
    /// <horizontecommand>
    /// Este método está asociado al comando "Porcupine_Init".
    /// </horizontecommand>
    /// <returns>
    /// Devuelve un booleano que indica si el inicio del módulo fue exitoso.
    /// </returns>
    [HorizonteRole("init")]
    [HorizonteCommand("Porcupine_Init")]
    public bool Init()
    {
        _logger.LogInformation("Módulo Porcupine Iniciado");
        return true;
    }

    /// <summary>
    /// Método que proporciona el widget de configuración para el módulo Porcupine.
    /// Utiliza la clase <c>WidgetDef</c> para definir el tipo de widget y sus parámetros
    /// de configuración. La implementación específica del widget se encuentra en
    /// <c>PorcupineCfgWidget</c>.
    /// </summary>
    /// <returns>Una instancia de <c>WidgetDef</c> que describe el widget de configuración para este módulo.</returns>
    [HorizonteRole("configpage")]
    [HorizonteCommand("Porcupine_ConfigPage", "Widget de configuración del módulo Porcupine")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(PorcupineCfgWidget), Parameters = null };

}