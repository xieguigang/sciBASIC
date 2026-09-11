# AVL Binary Tree Clustering with Newick Tree Output

Clusters vector data by inserting items into an AVL binary tree under a fuzzy similarity comparison, then cuts the tree into partitions.

## Overview
- Builds a cluster hierarchy by inserting every item id into an `AVLTree(Of String, String)` whose comparer comes from a pluggable similarity provider, so membership is decided by an `equals`/`gt` threshold pair rather than by a distance cutoff.
- Similarity can be supplied from a pre-computed `DistanceMatrix`, from named numeric vectors (dot product, cosine, etc. via `CompareMethods`), or from any custom `ComparisonProvider` subclass.
- Cuts the resulting tree into flat partitions (`CreateClusterPartitions`, `GetClusterResult`) and can export it as a `NetworkGraph` for visualisation.
- Extra helpers shipped in the same assembly: Newick tree parsing, affinity propagation with a sparse similarity matrix and quick-select, KNN graph construction and diffusion-map embedding.

## Key Types
- `Microsoft.VisualBasic.DataMining.BinaryTree.BuildTree` — module of extension methods (`BTreeCluster`, `BTreeClusterVector`) that turn a distance matrix or an `IEnumerable(Of T)` into a `BTreeCluster` root.
- `Microsoft.VisualBasic.DataMining.BinaryTree.BTreeCluster` — node of the cluster tree, exposing `uuid`, `members`, `data` and `left`/`right` children; also hosts `GetClusters` and `PullAllClusterNodes`.
- `Microsoft.VisualBasic.DataMining.BinaryTree.ComparisonProvider` — abstract similarity source (`GetSimilarity`, `GetObject`) plus `GetComparer()` for the AVL tree.
- `Microsoft.VisualBasic.DataMining.BinaryTree.AlignmentComparison` — comparison provider built from a set of named vectors or a matrix, with a selectable `CompareMethods` metric.
- `Microsoft.VisualBasic.DataMining.BinaryTree.Comparison` — adapter that reads scores out of a pre-computed `DistanceMatrix`.
- `Microsoft.VisualBasic.DataMining.BinaryTree.Partitioning` — module with `CreateClusterPartitions` and `GetClusterResult` for flattening the tree into clusters.
- `Microsoft.VisualBasic.DataMining.BinaryTree.NewickParser` — module that parses a Newick string into a `TreeNode(Of T)`.
- `Microsoft.VisualBasic.DataMining.BinaryTree.AffinityPropagation.AffinityPropagation` — message-passing clustering with `Fit()` returning cluster labels and `centers`.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DataMining.BinaryTree
Imports Microsoft.VisualBasic.DataMining.ComponentModel.EntityModels

' data: any IEnumerable(Of T) where T is INamedValue + IVector, e.g. ClusterEntity
Dim root As BTreeCluster = data.BTreeClusterVector(equals:=0.9, gt:=0.7)

For Each part As BTreeCluster In Partitioning.CreateClusterPartitions(root, depth:=3)
    Console.WriteLine($"{part.uuid} -> {part.members.Length} members")
Next
```

## Package
- Assembly: `Microsoft.VisualBasic.DataMining.BinaryTreeClustering`
- TargetFramework: `net10.0`
- Tags: `scibasic;clustering;binary-tree;avl-tree;newick`

## License
GPL-3.0-or-later
