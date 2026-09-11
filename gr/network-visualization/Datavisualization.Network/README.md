# Network Graph Data Model And Topology Analysis Library

Core data model and topology analysis layer of the sciBASIC# network visualization stack: it defines the `NetworkGraph` node/edge model and runs community detection, degree, betweenness and graph-abstraction analysis on top of it.

## Overview
- `NetworkGraph`, `Node` and `Edge` carry the graph structure; `NodeData` / `EdgeData` hold styling, initial coordinates, bend points and arbitrary user properties.
- Dynamic graph tables (`AdjacencySet`, `DirectedVertex`, `GraphIndex`, `EdgeSet`) give fast node lookup and in/out edge traversal for directed and undirected graphs.
- `Analysis` provides community detection with modularity, degree and betweenness centrality, strongly connected components, sub-network decomposition and PAGA abstraction.
- `GraphEmbedding` / `GraphTopology` convert a graph into numeric vectors for similarity comparison.
- `Handle` and `WayPointVector` describe orthogonal edge bends used later by the layout and rendering layers.

## Key Types
- `Microsoft.VisualBasic.Data.visualize.Network.Graph.NetworkGraph` — the mutable graph object model; adds and indexes nodes and edges.
- `Microsoft.VisualBasic.Data.visualize.Network.Graph.Node` — a vertex, with `label`, degree and a `NodeData` payload.
- `Microsoft.VisualBasic.Data.visualize.Network.Graph.Edge` — a link between two nodes with weight and `EdgeData`.
- `Microsoft.VisualBasic.Data.visualize.Network.Analysis.Communities` — community extraction, `GetCommunitySet` and `Modularity`.
- `Microsoft.VisualBasic.Data.visualize.Network.Analysis.Statistics` — degree, connected degree and betweenness centrality.
- `Microsoft.VisualBasic.Data.visualize.Network.Analysis.Model.GraphIndex` — adjacency index backing fast edge queries.
- `Microsoft.VisualBasic.Data.visualize.Network.Analysis.PAGA` — partition-based graph abstraction.
- `Microsoft.VisualBasic.Data.visualize.Network.NetworkAPI` — neighbourhood, sub-graph and duplicate-edge removal extensions.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Dim g As New NetworkGraph()
Dim a = g.AddNode(New Node("A"))
Dim b = g.AddNode(New Node("B"))

Call g.CreateEdge(a, b, 0.8)

Dim degrees = g.ComputeNodeDegrees()
Dim groups = Communities.Community(g)
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.visualize.Network`
- TargetFramework: `net10.0`
- Tags: `scibasic;network-graph;graph-model;graph-analysis;centrality`

## License
GPL-3.0-or-later
