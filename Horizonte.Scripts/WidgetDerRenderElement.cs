using Microsoft.DotNet.Interactive.Documents;

namespace Horizonte.Scripts;


public sealed class WidgetDerRenderElement : InteractiveDocumentOutputElement
{
    public WidgetDef WidgetDef { get; }

    public WidgetDerRenderElement(WidgetDef widgetDef)
    {
        WidgetDef = widgetDef;
    }

}