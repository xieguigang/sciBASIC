# 2D And 3D Scientific Graphics Rendering Engine

A driver-based drawing engine that renders charts, heatmaps, shapes, text and isometric 3D scenes through one `IGraphics` API to bitmap, SVG or PostScript output.

## Overview
- Pluggable output drivers (`Drivers.GDI`, `SVG`, `WMF`, `PostScript`) behind a single `IGraphics`/`IPlot` contract; `GraphicsData` is the driver-neutral rendering result.
- 2D primitives and shape models: arrow, box, circle, diamond, hexagon, line, pentacle, round rect and triangle, plus a polar graphics context.
- 2D geometry algorithms: convex and concave hulls, Delaunay/Voronoi diagrams, marching-squares contours, polyline simplification, splines and canvas scaling.
- Colour infrastructure: ColorBrewer and Office palettes, colour designers and expressions, categorical and value-scale colour mappers, heat-map rasters, hqx upscaling and legend rendering.
- 3D support: camera, lights, vectors/matrices, projection, marching cubes and an isometric model engine with shape and path libraries.
- Text and label layout: d3-style force-directed and simulated-annealing labelers, word wrap, ASCII art and HTML-ish markup rendering.

## Key Types
- `Microsoft.VisualBasic.Imaging.Driver.InternalCanvas` — multi-layer canvas that composes several `IPlot` delegates into one `GraphicsData`.
- `Microsoft.VisualBasic.Imaging.Driver.GraphicsData` / `ImageData` / `SVGData` / `PSData` — driver-neutral rendering results that can be saved to a stream or file.
- `Microsoft.VisualBasic.Imaging.Drawing2D.g` — entry module: `Allocate`, `GraphicsPlots`, `CreateGraphics` and background filling.
- `Microsoft.VisualBasic.Imaging.Drawing2D.GraphicsRegion` — the canvas size and padding handed to every plot callback.
- `Microsoft.VisualBasic.Imaging.SVG.GraphicsSVG` — the SVG back end implementing `IGraphics`.
- `Microsoft.VisualBasic.Imaging.PostScript.GraphicsPostScript` / `PostScript.Writer` — the PostScript back end and document writer.
- `Microsoft.VisualBasic.Imaging.Drawing3D.IsometricEngine` — the isometric 3D scene renderer.
- `Microsoft.VisualBasic.Imaging.d3js.labeler.Labeler` — simulated-annealing automatic label placement.

## Quick Start
```vbnet
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver

Dim canvas As InternalCanvas = g.Allocate(New Size(1600, 1200), bg:="white")

canvas += Sub(ByRef g As IGraphics, region As GraphicsRegion)
              Call g.DrawCircle(Pens.Black, 200, 200, 80)
              Call g.DrawString("sciBASIC#",
                                New Font(FontFace.MicrosoftYaHei, 32),
                                Brushes.Black, 320.0F, 180.0F)
          End Sub

Using image As GraphicsData = canvas.InvokePlot()
    Call image.Save("demo.png")
End Using
```

## Package
- Assembly: `Microsoft.VisualBasic.Imaging`
- TargetFramework: `net10.0-windows;net10.0`
- Tags: `scibasic;graphics;imaging;svg;postscript`

## License
GPL-3.0-or-later
