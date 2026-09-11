# Statistical Chart Extensions: ROC, QQ, Heatmap and PCA Plots

Statistical graphics extension for the sciBASIC# chart engine, adding ROC/QQ diagnostics, correlation and clustered heatmaps, density and time-trend plots, and multivariate PCA/regression views.

## Overview
- Diagnostic plots: ROC curves with AUC computed into the series title, QQ plots, forest plots, Z-score plots and bihistograms.
- Matrix plots: heatmaps with dendrograms, row/column class bars and row/column/global scaling, correlation heatmaps and correlation triangles, bubble heatmaps and point-density plots.
- Multivariate plots: PCA 2D/3D score plots with confidence ellipses, scree plots and regression plots with fitted and error-band points.
- Data helpers: time-trend serials, variable-width bar plots, joint plots, sample distribution views and CSV/DataSet loaders.

## Key Types
- `Microsoft.VisualBasic.Data.ChartPlots.Statistics.ROCPlot` — builds ROC serials from `Validation` or `DataSet` input and renders the curve with optional reference line and AUC fill.
- `Microsoft.VisualBasic.Data.ChartPlots.Statistics.QQPlot` — quantile-quantile plot comparing two distributions against a reference line.
- `Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap.HeatMapPlot` — heatmap with dendrograms, class bars and selectable scaling via `DrawElements`.
- `Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap.CorrelationHeatmap` — symmetric correlation matrix heatmap built from `CorrelationData`.
- `Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap.CorrelationTriangle` — lower/upper triangular correlation view.
- `Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap.DensityPlot` — point-density raster plot similar to a contour plot.
- `Microsoft.VisualBasic.Data.ChartPlots.Statistics.PCA.ScreePlot` — PCA scree (variance explained) plot.
- `Microsoft.VisualBasic.Data.ChartPlots.Statistics.RegressionPlot` — regression fit plot with fitted, predicted and error-band point styles.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics

' test As IEnumerable(Of Validation) exposing Specificity and Sensibility
Dim roc As SerialData = test.CreateSerial()
Dim img As GraphicsData = ROCPlot.Plot(roc, fillAUC:=True, showReference:=True)

Call img.Save("roc.png")
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.ChartPlots.Statistics`
- TargetFramework: `net10.0`
- Tags: `scibasic;statistics;roc-curve;heatmap;qq-plot;pca-plot`

## License
GPL-3.0-or-later
