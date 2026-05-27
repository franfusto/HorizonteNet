using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;
using Horizonte.AdminUi.HorizonteSettings.Entidades;
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
    /// <summary>
    /// La clase <c>DiagramExtensions</c> proporciona métodos de extensión para el manejo
    /// y disposición de diagramas dentro de la interfaz Blazor.Diagrams.
    /// Permite aplicar diferentes configuraciones de disposición a diagramas.
    /// </summary>
    public static class DiagramExtensions
    {
        /// <summary>
        /// Establece el diseño de un diagrama utilizando el tipo de disposición especificado.
        /// Este método es un alias que utiliza por defecto el diseño en capas.
        /// </summary>
        /// <param name="diagram">
        /// El diagrama de Blazor.Diagrams al que se aplicará el nuevo diseño.
        /// </param>
        /// <param name="layout">
        /// El tipo de disposición a aplicar al diagrama. Se utiliza la enumeración <c>DiagramLayout</c>
        /// para especificar este valor, que puede ser <c>Layered</c>, <c>Mds</c>, o <c>Incremental</c>.
        /// </param>
        public static void SetLayOut(this BlazorDiagram diagram,  DiagramLayout layout)
        {
            diagram.SetLayout(DiagramLayout.Layered);
        }

        /// <summary>
        /// Establece el diseño de un diagrama según el tipo de disposición especificado,
        /// aplicando opciones adicionales si se proveen.
        /// </summary>
        /// <param name="diagram">
        /// El diagrama de Blazor.Diagrams al que se aplicará el diseño.
        /// </param>
        /// <param name="layout">
        /// El tipo de disposición a aplicar, representado por la enumeración <c>DiagramLayout</c>.
        /// Puede ser <c>Layered</c>, <c>Mds</c>, o <c>Incremental</c>.
        /// </param>
        /// <param name="options">
        /// Opciones adicionales de configuración para el diseño del diagrama.
        /// Si no se especifica, se utilizan las opciones predeterminadas.
        /// </param>
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




}