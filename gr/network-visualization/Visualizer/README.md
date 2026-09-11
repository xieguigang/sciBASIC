# Network Graph Rendering Engine With CSS Style Mapping

Rendering layer of the sciBASIC# network visualization stack: turns a laid-out `NetworkGraph` into a bitmap or vector image, drawing nodes, edges, arrowheads, labels and convex-hull group polygons through a CSS-like map-expression styling language.

## Overview
- `NetworkVisualizer.DrawImage` renders a graph to `GraphicsData` with configurable canvas size, padding, background, node radius, link width, edge dash types, shadows and labels.
- `NetworkPlot` / `NetworkRenderConfig` expose the same pipeline as an object, with one property per rendering option.
- Styling through `StyleMapper` / `StyleCreator`: node, edge and label properties are mapped to colours, sizes and shapes with `MapExpression` (unify, passthrough, discrete, continuous, category, image) and the `Styling.FillBrushes` / `Styling.Numeric` providers.
- Label placement by simulated annealing (`LabelRendering`), convex-hull group polygons with legend (`HullPolygonRendering`), and bundled-edge rendering (`MingleRender`).

## Key Types
- `Microsoft.VisualBasic.Data.visualize.Network.NetworkVisualizer` — main `DrawImage` entry point.
- `Microsoft.VisualBasic.Data.visualize.Network.NetworkPlot` — object-oriented renderer driven by `NetworkRenderConfig`.
- `Microsoft.VisualBasic.Data.visualize.Network.Styling.CSS.StyleMapper` — parsed network CSS style model (`FromCSS`).
- `Microsoft.VisualBasic.Data.visualize.Network.Styling.MapExpression` — colour, size and shape map expression parser.
- `Microsoft.VisualBasic.Data.visualize.Network.Styling.MapperProcessor` — applies a style mapping onto a graph.
- `Microsoft.VisualBasic.Data.visualize.Network.Styling.FillBrushes.UnifyColorBrush` — brush providers (unify, passthrough, discrete, category, image).
- `Microsoft.VisualBasic.Data.visualize.Network.LabelRendering` — annealing-based node label placement.
- `Microsoft.VisualBasic.Data.visualize.Network.HullPolygonRendering` — convex hull group polygons and legend.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.visualize.Network

Dim img As GraphicsData = NetworkVisualizer.DrawImage(
    net:=graph,
    canvasSize:="1600,1200",
    background:="white",
    defaultColor:="skyblue",
    displayId:=True)
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.visualize.Network.Visualizer`
- TargetFramework: `net10.0`
- Tags: `scibasic;network-rendering;data-visualization;style-mapping;convex-hull`

## License
GPL-3.0-or-later
