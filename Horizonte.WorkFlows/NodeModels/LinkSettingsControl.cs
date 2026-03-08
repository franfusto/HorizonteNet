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

namespace Horizonte.WorkFlows.NodeModels;




public class LinkSettingsControl : ExecutableControl
{
    public Rectangle Bounds { get; private set; } = Rectangle.Zero;
    
    public override Point? GetPosition(Model model)
    {
        if (model is not IHasBounds hb)
            return null;

        var bounds = hb.GetBounds();
        if (bounds == null)
            return null;

        Bounds = bounds.Inflate(10, 10);
        return Bounds.NorthWest;
    }

    public override ValueTask OnPointerDown(Diagram diagram, Model model, PointerEventArgs e)
    {
        if(model.Locked) return new ValueTask();
        return new ValueTask();
        //throw new NotImplementedException();
    }
}