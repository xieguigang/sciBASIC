# Spatial Density Query and SLIC Superpixel Segmentation

Estimates local point density on 2D grids and KD-tree indexes, and segments images into compact superpixels with SLIC.

## Overview
- Grid-based 2D density: bins points into a `Grid(Of T)` and returns a normalised `[0, 1]` local density per point from a sliding `GridBox` window, serially or in parallel.
- KD-tree density: `KDQuery` implements `IQueryDensity(Of ClusterEntity)` and returns `1 / mean distance to the k nearest neighbours`, a standard outlier/lof-style score.
- SLIC (Simple Linear Iterative Clustering) superpixels: reads pixels from a `BitmapBuffer`, seeds centres on a regular grid and iteratively re-assigns pixels in combined colour plus image-plane space.
- Works directly on the sciBASIC# `ClusterEntity` vector model, so the density scores can be fed back into the clustering toolkit.

## Key Types
- `Microsoft.VisualBasic.DataMining.DensityQuery.SLIC` — superpixel segmenter: `ReadImagePixels`, `InitializeCenters`, `IterateClustering`, `MeasureSegments`.
- `Microsoft.VisualBasic.DataMining.DensityQuery.SLICPixel` — a pixel with `x`, `y`, `color` channels, a `cluster` label and `DistanceTo`.
- `Microsoft.VisualBasic.DataMining.DensityQuery.KDQuery` — KD-tree index over `ClusterEntity` rows implementing `QueryDensity(row, k)` and `Raw()`.
- `Microsoft.VisualBasic.DataMining.DensityQuery.Metric` — distance metric adapter used to build the underlying `KdTree`.
- `Microsoft.VisualBasic.DataMining.DensityQuery.Density2D` — module with `WindowSize` and the `Density` extension methods over 2D grids.
- `Microsoft.VisualBasic.DataMining.DensityQuery.GridBox` — sliding window query box over a `Grid(Of T)`.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DataMining.DensityQuery
Imports Microsoft.VisualBasic.DataMining.ComponentModel.EntityModels

' density of a point set through a KD-tree index
Dim query As New KDQuery(entities)          ' entities: IEnumerable(Of ClusterEntity)
Dim d As NamedValue(Of Double) = query.QueryDensity(entities(0), k:=10)

' SLIC superpixels over an image buffer (buffer As BitmapBuffer)
Dim slic As New SLIC(buffer)
Dim pixels As SLICPixel() = slic.MeasureSegments(regionSize:=20, numIterations:=10)
```

## Package
- Assembly: `Microsoft.VisualBasic.DataMining.DensityQuery`
- TargetFramework: `net10.0`
- Tags: `scibasic;density;superpixel;slic;kd-tree`

## License
GPL-3.0-or-later
