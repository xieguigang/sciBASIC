# Graph Theory Algorithms, Network Models and Graph Search

Graph theory and network analysis toolkit for sciBASIC#, providing shortest-path routing, community detection, flow and spanning-tree algorithms, and nearest-neighbour search over generic graph models.

## Overview
- Shortest paths and routing: object-oriented Dijkstra (`DijkstraRouter`), priority-queue Dijkstra (`DijkstraFast`), contraction hierarchies with bidirectional Dijkstra, and A* routing over grid graphs.
- Community detection: Louvain, Leiden (refinement phase guarantees well-connected communities) and label propagation (LPA), plus modularity evaluation helpers.
- Network algorithms: Dinic maximum flow, Kruskal minimum spanning tree, Ullmann subgraph isomorphism, bipartite matching, PageRank and weighted PageRank, Morgan fingerprints.
- Search structures and data models: KD-tree with approximate k-NN, HNSW small-world graph, cosine and Earth Mover's distances, Huffman tree, Trie, PQ-tree, and 2D/3D spatial grid graphs.

## Key Types
- `Microsoft.VisualBasic.Data.GraphTheory.Graph` — abstract `G = (V, E)` container holding vertices and edges.
- `Microsoft.VisualBasic.Data.GraphTheory.Network.NetworkGraph(Of Node, Edge)` — concrete generic network graph with node degree and edge metadata.
- `Microsoft.VisualBasic.Data.GraphTheory.Analysis.Dijkstra.DijkstraRouter` — single-source Dijkstra routing, returns a `Route` for every reachable vertex.
- `Microsoft.VisualBasic.Data.GraphTheory.Analysis.Louvain.LouvainCommunity` — fast unfolding of communities in large networks (`SolveClusters`, `SolveClustersParallel`).
- `Microsoft.VisualBasic.Data.GraphTheory.Analysis.Louvain.LeidenCommunity` — Leiden refinement of Louvain that guarantees intra-community connectivity.
- `Microsoft.VisualBasic.Data.GraphTheory.Analysis.LPA.LabelPropagation` — near-linear-time label propagation community detection.
- `Microsoft.VisualBasic.Data.GraphTheory.MinimumSpanningTree.Kruskal` — minimum spanning tree via Kruskal on disjoint sets.
- `Microsoft.VisualBasic.Data.GraphTheory.KNearNeighbors.HNSW.SmallWorld` — hierarchical navigable small-world graph for approximate k-NN search.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.Louvain
Imports Microsoft.VisualBasic.Data.GraphTheory.Network

' g is an existing NetworkGraph(Of Node, Edge)
Dim louvain As LouvainCommunity = Builder.Load(g)
Dim community As String() = louvain.SolveClusters().GetCommunity()
Dim clusterCount As Integer = louvain.GetClusterCount()
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.GraphTheory`
- TargetFramework: `net10.0`
- Tags: `scibasic;graph-theory;graph-algorithm;shortest-path;community-detection`

## License
GPL-3.0-or-later
