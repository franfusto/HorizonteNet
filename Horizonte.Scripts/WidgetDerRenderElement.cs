using Microsoft.DotNet.Interactive.Documents;

namespace Horizonte.Scripts;

/// <summary>
/// Elemento de salida de documento interactivo que encapsula y renderiza una definición de widget (<see cref="WidgetDef"/>).
/// </summary>
public sealed class WidgetDerRenderElement : InteractiveDocumentOutputElement
{
    /// <summary>
    /// Obtiene la definición del widget a renderizar como salida interactiva.
    /// </summary>
    public WidgetDef WidgetDef { get; }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="WidgetDerRenderElement"/>.
    /// </summary>
    /// <param name="widgetDef">Definición del widget a renderizar.</param>
    public WidgetDerRenderElement(WidgetDef widgetDef)
    {
        WidgetDef = widgetDef;
    }
}