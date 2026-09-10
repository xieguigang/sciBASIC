# Dendrogram and Radial Tree Plotting for Cluster Hierarchies

Renders the `Cluster` trees produced by the hierarchical clustering engine as graphics for reports and publications.

## Overview
- Draws the same hierarchy in four layouts selected through `Layouts`: `Vertical`, `Horizon`, `HorizonRightToLeft` and `Radial`.
- Single entry point `Dendrogram.Plot` builds the theme, resolves class colours and returns a `GraphicsData` image.
- Optional class colouring: a `classinfo` map of leaf name to class label is expanded into a colour table from a designer colour set.
- Panels are reusable `Chart` plot objects (`DendrogramPanelV2`, `Horizon`, `HorizonRightToLeft`, `RadialDendrogram`, `Circular`) that can also be hosted directly.

## Key Types
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.Dendrogram` — module with the `Plot` extension method that dispatches to a layout and returns `GraphicsData`.
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.Layouts` — enum of available dendrogram layouts.
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.DendrogramPanelV2` — vertical dendrogram panel with leaf labels, link colour, padding and log-scaled distance axis.
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.Horizon` — horizontal dendrogram, leaves ordered left to right.
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.HorizonRightToLeft` — horizontal dendrogram rendered in the reverse direction.
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.RadialDendrogram` — circular/radial layout, can also emit leaf label anchor points.
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.Circular` — circular dendrogram panel derived from `DendrogramPanel`.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.Imaging.Driver

' tree is a Cluster returned by the hierarchical-clustering package;
' classinfo maps each leaf name onto a class label for colouring.
Dim g As GraphicsData = tree.Plot(
    classinfo:=classinfo,
    size:="2700,2100",
    colorSet:="Set1:category10",
    layout:=Layouts.Vertical)
```

## Package
- Assembly: `Microsoft.VisualBasic.DataMining.HierarchicalClustering.Plot`
- TargetFramework: `net10.0`
- Tags: `scibasic;dendrogram;visualization;hierarchical-clustering;plot`

## License
GPL-3.0-or-later
