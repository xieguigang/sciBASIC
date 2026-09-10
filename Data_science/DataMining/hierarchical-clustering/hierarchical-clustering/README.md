# Hierarchical Clustering with BIRCH and Linkage Strategies

Agglomerative hierarchical clustering engine that turns a distance matrix or a stream of vectors into a reusable `Cluster` dendrogram.

## Overview
- JBIRCH streaming clustering: a balanced CF-tree (`CFTree`, `CFNode`, `CFEntry`) with configurable node capacity, distance threshold, distance function (D0-D4) and optional merging refinement.
- Agglomerative clustering through `ClusteringAlgorithm` / `DefaultClusteringAlgorithm`, supporting standard, weighted and flat (threshold cut) clustering.
- Pluggable linkage criteria: single, complete, average (UPGMA) and weighted linkage strategies.
- Reusable hierarchy construction from pre-computed linkages via `DistanceMap`, `Distance`, `HierarchyLink` and `HierarchyBuilder`, plus a parallel Euclidean distance matrix helper in `DoCluster`.

## Key Types
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.Cluster` — a node of the dendrogram: leaf or merged cluster with children, distance, weight and leaf names.
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.ClusteringAlgorithm` — interface for performing standard, weighted and flat clustering from a distance matrix.
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.DefaultClusteringAlgorithm` — default agglomerative implementation driven by a `LinkageStrategy`.
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.LinkageStrategy` — abstraction for cluster-to-cluster distance, with `SingleLinkageStrategy`, `CompleteLinkageStrategy`, `AverageLinkageStrategy` and `WeightedLinkageStrategy`.
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.CFTree` — BIRCH clustering feature tree for incremental/streaming inserts.
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.HierarchyBuilder` — builds a `HierarchyTreeNode` tree from a `DistanceMap`.
- `Microsoft.VisualBasic.DataMining.HierarchicalClustering.DoCluster` — convenience extensions that build the Euclidean distance matrix and run clustering over named vectors.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering

' agglomerative clustering from a pre-computed distance matrix
Dim alg As ClusteringAlgorithm = New DefaultClusteringAlgorithm
Dim tree As Cluster = alg.performClustering(distances, names, New AverageLinkageStrategy)

' streaming BIRCH clustering
Dim cf As New CFTree(maxNodeEntries:=50, distThreshold:=0.5)
Call cf.insertEntry(New Double() {1.0, 2.0, 3.0})
```

## Package
- Assembly: `Microsoft.VisualBasic.DataMining.HierarchicalClustering`
- TargetFramework: `net10.0`
- Tags: `scibasic;hierarchical-clustering;birch;linkage;clustering`

## License
GPL-3.0-or-later
