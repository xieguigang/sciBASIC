# Interactive WinForms Network Graph Viewer Control

A Windows Forms control that animates a `NetworkGraph` with the spring-force layout engine, renders it in 2D or 3D, and exposes mouse hit-testing, node tooltips and viewport zoom/pan for sciBASIC# desktop applications.

## Overview
- `Canvas` user control hosting a physics thread and a render thread, switchable between the 2D `Renderer` and the 3D `Renderer3D` engine.
- Mouse interaction: left-button node dragging, `HitTest` / `GetTargetNode` picking accelerated by `SpatialGrid`, node tooltips, wheel zoom and panning via `Viewport` / `CanvasViewState`.
- Static output: `GetSnapshot` bitmap capture, SVG export through `SVGExtensions`, and AVI animation recording through `AVI.DoRenderVideo`.
- `IGraphicsEngine` contract lets custom renderers be plugged into the canvas.

## Key Types
- `Microsoft.VisualBasic.Data.visualize.Network.Canvas.Canvas` — the WinForms network viewer control.
- `Microsoft.VisualBasic.Data.visualize.Network.Canvas.Renderer` — 2D GDI+ node and edge renderer.
- `Microsoft.VisualBasic.Data.visualize.Network.Canvas.Renderer3D` — 3D projection renderer with rotation and view distance.
- `Microsoft.VisualBasic.Data.visualize.Network.Canvas.Viewport` — zoom and pan transform between graph and screen coordinates.
- `Microsoft.VisualBasic.Data.visualize.Network.Canvas.SpatialGrid` — uniform screen grid for fast hit-testing.
- `Microsoft.VisualBasic.Data.visualize.Network.Canvas.AVI` — records the live layout animation into a video.
- `Microsoft.VisualBasic.Data.visualize.Network.Canvas.SVGExtensions` — exports a `NetworkGraph` into an SVG document.
- `Microsoft.VisualBasic.Data.visualize.Network.Canvas.IGraphicsEngine` — renderer contract consumed by the canvas.

## Quick Start
```vbnet
Dim viewer As New Canvas()

viewer.Graph = networkGraph
viewer.ShowLabel = True

Call viewer.Run()

Dim picked As Node = viewer.HitTest(mousePosition)
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.visualize.Network.Canvas`
- TargetFramework: `net10.0-windows`
- Tags: `scibasic;network-canvas;winforms;interactive-viewer;data-visualization`

## License
GPL-3.0-or-later
