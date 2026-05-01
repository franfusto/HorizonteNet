using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Controls;
using Blazor.Diagrams.Core.Behaviors;
using Blazor.Diagrams.Core.Events;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Core.Positions;
using System;
using System.Threading.Tasks;
using Horizonte.Extension.AiWorkFlows;

namespace Horizonte.WorkFlows.NodeModels;

public class LinkSettingsControl : ExecutableControl
{

    public WorkFlowDef WorkFlowDef { get; private set; }
    public WorkFlowLink WorkFlowLink { get; private set; }
    public Rectangle Bounds { get; private set; } = Rectangle.Zero;
    public Diagram Diagram { get; private set; }

    public LinkSettingsControl(WorkFlowLink LinkDef)
    {
        WorkFlowLink = LinkDef;
    }
    
    public override Point? GetPosition(Model model)
    {
        if (model is not IHasBounds hb)
            return null;

        var bounds = hb.GetBounds();
        if (bounds == null)
            return null;

        Bounds = bounds;
        return Bounds.West;
    }

    public override ValueTask OnPointerDown(Diagram diagram, Model model, PointerEventArgs e)
    {
        return new ValueTask();
    }

    public Action OnConditionChanged { get; set; }

    public void EditCondition(WorkFlowLink updLink)
    {
        WorkFlowLink = updLink;
        OnConditionChanged?.Invoke();
    }

    public void DeleteCondition()
    {
        WorkFlowLink.Condition = null;
        OnConditionChanged?.Invoke();
    }
}