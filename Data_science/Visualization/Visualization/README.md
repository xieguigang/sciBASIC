# Visualization Layer for Networks, Clusters and Embeddings

Visualization abstractions for sciBASIC# that turn data matrices, clustering results and dimensionality-reduction embeddings into network graphs and 2D/3D charts.

## Overview
- Network construction: correlation networks from `DataSet` collections, distance/correlation matrices or data frames, plus K-means clustering networks.
- Embedding rendering: 2D and 3D views of UMAP (and any `IDataEmbedding`) projections with cluster colouring and convex hulls, plus SOM embedding plots.
- Graph conversion: builds a `NetworkGraph` from a UMAP projection result so neighbours can be laid out and rendered with the network visualization stack.
- Tabular helpers: renders CSV tables into bar data groups and scatter serials for direct charting.

## Key Types
- `Microsoft.VisualBasic.Data.visualize.CorrelationNetwork` — builds a `NetworkGraph` from datasets or matrices using an absolute-correlation cutoff.
- `Microsoft.VisualBasic.Data.visualize.KMeans.KMeansNetwork` — builds a K-means clustering network graph from cluster memberships.
- `Microsoft.VisualBasic.Data.visualize.Embedding2D` — 2D chart of an embedding with cluster colours and optional convex hulls.
- `Microsoft.VisualBasic.Data.visualize.Embedding3D` — 3D chart of an embedding rendered through a camera.
- `Microsoft.VisualBasic.Data.visualize.EmbeddingRenderExtensions` — `GetPoint2D`/`GetPoint3D` and `DrawEmbedding2D`/`DrawEmbedding3D` extension methods.
- `Microsoft.VisualBasic.Data.visualize.UMAPGraph` — converts a UMAP projection into a network graph model.
- `Microsoft.VisualBasic.Data.visualize.SOMEmbedding` — plots a self-organizing map embedding.
- `Microsoft.VisualBasic.Data.visualize.KMeans.Partition` — partitions cluster members into a binary tree of entity clusters.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.visualize

' matrix is a similarity (non-distance) matrix of your samples
Dim net = matrix.BuildNetwork(cutoff:=0.75)

' umap As IDataEmbedding; clusters maps label -> cluster id
Dim img As GraphicsData = umap.DrawEmbedding2D(labels, clusters, showConvexHull:=True)
Call img.Save("umap.png")
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.visualize`
- TargetFramework: `net10.0-windows;net10.0`
- Tags: `scibasic;visualization;network-graph;embedding;clustering;umap`

## License
GPL-3.0-or-later
