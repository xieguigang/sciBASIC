# DataPlot: GDI+ Chart Engine with ggplot2-Style Theme Units

A standalone GDI+ charting engine for sciBASIC# that draws basic and advanced statistical charts onto an in-memory bitmap and applies ggplot2-style theme units with automatic pixel layout.

## Overview
- Basic charts: scatter, line, area, bar, stacked bar, stacked area and histogram, all driven by a shared `PlotEngine` that handles coordinate transforms, axes, grid, legend and markers.
- Advanced charts: box, violin, jitter, bubble, pie/donut, radar, rose, heatmap (viridis and other colour maps), chord, Sankey and treemap.
- ggplot2-compatible theming: `Theme` element trees (`ElementLine`, `ElementText`, `ElementRect`, `ElementPoint`), CSS rule mapping via `CssThemeMapper`, and a `UnitConverter` supporting cm, mm, pt, lines, char, npc, inch and px.
- Output: renders through the sciBASIC imaging driver abstraction and exports via the `SavePng` extension; the raw `IGraphics` device is available for custom drawing.

## Key Types
- `Microsoft.VisualBasic.Data.Plots.PlotEngine` — base canvas engine: data-to-pixel transforms, axis/grid, legend and marker drawing, PNG export.
- `Microsoft.VisualBasic.Data.Plots.ScatterPlot` — scatter/line renderer consuming a list of `Series`.
- `Microsoft.VisualBasic.Data.Plots.Series` — one data series with `X`, `Y`, name, colour, marker shape and line style.
- `Microsoft.VisualBasic.Data.Plots.PlotTheme` — visual style parameters (palette, fonts, line width, margins).
- `Microsoft.VisualBasic.Data.Plots.HeatmapPlot` — matrix heatmap with configurable colour map and value labels.
- `Microsoft.VisualBasic.Data.Plots.SankeyPlot` — Sankey flow diagram built from `SankeyNode` / `SankeyLink`.
- `Microsoft.VisualBasic.Data.Plots.GgplotTheme.Theme` — ggplot2-style theme element resolution and inheritance.
- `Microsoft.VisualBasic.Data.Plots.GgplotTheme.LayoutCalculator` — converts theme units into pixel-level plot layout.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.Plots

Using plot As New ScatterPlot(1200, 800)
    plot.Title = "Sample"
    plot.XLabel = "x"
    plot.YLabel = "y"

    Call plot.Plot({
        New Series With {.Name = "A", .X = x, .Y = y, .MarkerShape = MarkerShape.Circle},
        New Series With {.Name = "B", .X = x2, .Y = y2}
    })
    Call plot.SavePng("scatter.png")
End Using
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.DataPlot`
- TargetFramework: `net10.0`
- Tags: `scibasic;dataplot;chart;plotting;gdi-plus;ggplot-theme;data-visualization`

## License
GPL-3.0-or-later
