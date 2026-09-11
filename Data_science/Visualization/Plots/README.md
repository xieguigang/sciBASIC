# 2D and 3D Chart Plotting Library for GDI+ Graphics

The core sciBASIC# charting engine that renders 2D and 3D scientific plots onto GDI+ graphics with configurable axes, legends, colour legends and CSS-driven themes.

## Overview
- 2D plots: scatter, line, bubble, polygon/fill, histogram, bar (simple, stacked, bidirectional, percentage, alignment), box, violin, contour and contour heatmap.
- Composition plots: pie, pyramid, radar, treemap and Venn diagrams via the fraction and `VennPlot` modules.
- 3D plots: scatter, serial/surface and pie rendering through a camera/projection device, with element sorting in `RenderEngine`.
- Graphics infrastructure: `DataScaler`/`Mapper` coordinate scaling, `Axis`/`AxisProvider` tick generation, `LegendObject` legends, colour bars and `Theme`/`Layout` styling built from CSS.

## Key Types
- `Microsoft.VisualBasic.Data.ChartPlots.Scatter` — module entry point; `Scatter.Plot(SerialData(), ...)` renders scatter/line/bubble charts to `GraphicsData`.
- `Microsoft.VisualBasic.Data.ChartPlots.SerialData` — one curve `[x, y]` model with colour, line style, point size, legend shape and annotations.
- `Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data.BarDataGroup` — grouped named-value data model consumed by the bar plot APIs.
- `Microsoft.VisualBasic.Data.ChartPlots.BoxPlot.BoxPlot` — box-and-whisker plotting from grouped `BoxData`.
- `Microsoft.VisualBasic.Data.ChartPlots.Contour.HeatMap.ContourHeatMapPlot` — contour heatmap of a 2D surface or evaluation function.
- `Microsoft.VisualBasic.Data.ChartPlots.Fractions.PieChart` — pie and donut style fraction charts.
- `Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Device.RenderEngine` — sorts and draws `Element3D` scene elements through a camera.
- `Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis.DataScaler` — maps user data coordinates to plot-region coordinates.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.ChartPlots

Dim serial As New SerialData With {
    .title = "series A",
    .color = Color.SteelBlue,
    .pointSize = 8,
    .pts = points          ' PointData()
}

Dim img As GraphicsData = Scatter.Plot(
    {serial},
    size:="1600,1200",
    Xlabel:="x",
    Ylabel:="y",
    showLegend:=True
)

Call img.Save("scatter.png")
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.ChartPlots`
- TargetFramework: `net10.0`
- Tags: `scibasic;chart;plotting;data-visualization;gdi-plus;graphics`

## License
GPL-3.0-or-later
