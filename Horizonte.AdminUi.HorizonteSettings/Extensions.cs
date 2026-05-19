using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Layout.MDS;
using Microsoft.Msagl.Miscellaneous;
using MsaglPoint = Microsoft.Msagl.Core.Geometry.Point;
using MsaglNode = Microsoft.Msagl.Core.Layout.Node;
using DiagramAnchor = Blazor.Diagrams.Core.Anchors.Anchor;

namespace Horizonte.AdminUi.HorizonteSettings
{
    public static class DiagramExtensions
    {
        public static void SetLayOut(this BlazorDiagram diagram,  DiagramLayout layout)
        {
            diagram.SetLayout(DiagramLayout.Layered);
        }

        public static void SetLayout(
            this BlazorDiagram diagram,
            DiagramLayout layout,
            DiagramLayoutOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(diagram);

            options ??= new DiagramLayoutOptions();

            if (diagram.Nodes.Count == 0)
                return;

            var graph = new GeometryGraph();
            var nodeMap = new Dictionary<NodeModel, MsaglNode>();

            foreach (var diagramNode in diagram.Nodes)
            {
                var msaglNode = new MsaglNode(
                    CurveFactory.CreateRectangle(
                        options.NodeWidth,
                        options.NodeHeight,
                        new MsaglPoint(0, 0)),
                    diagramNode.Id);

                graph.Nodes.Add(msaglNode);
                nodeMap[diagramNode] = msaglNode;
            }

            foreach (var link in diagram.Links)
            {
                var sourceNode = ResolveNode(link.Source);
                var targetNode = ResolveNode(link.Target);

                if (sourceNode is null || targetNode is null)
                    continue;

                if (!nodeMap.TryGetValue(sourceNode, out var msaglSource))
                    continue;

                if (!nodeMap.TryGetValue(targetNode, out var msaglTarget))
                    continue;

                if (!options.IncludeSelfLinks && ReferenceEquals(sourceNode, targetNode))
                    continue;

                graph.Edges.Add(new Edge(msaglSource, msaglTarget));
            }

            var layoutSettings = CreateLayoutSettings(layout, options);

            LayoutHelpers.CalculateLayout(graph, layoutSettings, null);

            diagram.Batch(() =>
            {
                foreach (var pair in nodeMap)
                {
                    var diagramNode = pair.Key;
                    var msaglNode = pair.Value;

                    var x = msaglNode.Center.X - options.NodeWidth / 2d + options.OffsetX;

                    // MSAGL usa un sistema de coordenadas cartesiano.
                    // En canvas/HTML normalmente Y crece hacia abajo, por eso invertimos Y.
                    var y = -msaglNode.Center.Y - options.NodeHeight / 2d + options.OffsetY;

                    diagramNode.SetPosition(x, y);
                }

                foreach (var link in diagram.Links)
                {
                    link.Refresh();
                }
            });
        }

        private static LayoutAlgorithmSettings CreateLayoutSettings(
            DiagramLayout layout,
            DiagramLayoutOptions options)
        {
            return layout switch
            {
                DiagramLayout.Layered => new SugiyamaLayoutSettings
                {
                    NodeSeparation = options.NodeSeparation,
                    LayerSeparation = options.LayerSeparation
                },

                DiagramLayout.Mds => new MdsLayoutSettings
                {
                    NodeSeparation = options.NodeSeparation
                },

                DiagramLayout.Incremental => new FastIncrementalLayoutSettings
                {
                    AvoidOverlaps = true,
                    //IdealEdgeLength = options.IdealEdgeLength
                },

                _ => new SugiyamaLayoutSettings()
            };
        }

        private static NodeModel? ResolveNode(DiagramAnchor anchor)
        {
            return anchor.Model switch
            {
                NodeModel node => node,
                PortModel port => port.Parent,
                _ => null
            };
        }
    }

    public enum DiagramLayout
    {
        Layered,
        Mds,
        Incremental
    }

    public sealed class DiagramLayoutOptions
    {
        public double NodeWidth { get; init; } = 180;
        public double NodeHeight { get; init; } = 60;
        public double NodeSeparation { get; init; } = 50;
        public double LayerSeparation { get; init; } = 80;
        public double IdealEdgeLength { get; init; } = 120;
        public double OffsetX { get; init; } = 50;
        public double OffsetY { get; init; } = 50;
        public bool IncludeSelfLinks { get; init; }
    }
}