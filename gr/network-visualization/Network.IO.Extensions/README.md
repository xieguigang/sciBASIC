# Network Graph File IO And Third-Party Format Interop

IO layer of the sciBASIC# network visualization stack: persists a `NetworkGraph` as CSV/JSON table sets or a zipped VB.NET network package, and imports or exports Cytoscape, Gephi GML, sigma.js and graphology documents.

## Overview
- Tabular network model (`nodes.csv`, `network-edges.csv`, `meta.json`) with loaders and writers in `NetworkFileIO`, `StreamTable`, `TabularCreator` and `Serialization`.
- Bidirectional conversion between the tabular model and the in-memory `NetworkGraph` (`ModelLoader.CreateGraph` / `GraphAPI`), mapping colour, size, degree and interaction type.
- Third-party interop: Cytoscape node/edge tables, Gephi GML, sigma.js JSON, graphology JSON and the zipped `.vbnet` network package.
- `CorrelationGraph` builds a network directly from a correlation matrix; `AnalysisAPI` computes degree data over table models.

## Key Types
- `Microsoft.VisualBasic.Data.visualize.Network.NetworkFileIO` — loads and saves a network directory of CSV tables plus `meta.json`.
- `Microsoft.VisualBasic.Data.visualize.Network.FileStream.NetworkTables` — the CSV table representation of a network graph.
- `Microsoft.VisualBasic.Data.visualize.Network.FileStream.ModelLoader` — `CreateGraph` / `CreateGraphGeneric` conversions into `NetworkGraph`.
- `Microsoft.VisualBasic.Data.visualize.Network.FileStream.Cytoscape.CytoscapeTableLoader` — Cytoscape table import and export.
- `Microsoft.VisualBasic.Data.visualize.Network.Gephi.GephiML` — GML document model and builder.
- `Microsoft.VisualBasic.Data.visualize.Network.SigmaJs` — sigma.js JSON output.
- `Microsoft.VisualBasic.Data.visualize.Network.FileStream.vbnet` — zipped sciBASIC network data package.
- `Microsoft.VisualBasic.Data.visualize.Network.CorrelationGraph` — network model built from a correlation matrix.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream

Dim tables As NetworkTables = NetworkFileIO.Load("D:/data/mynetwork/")
Dim g As NetworkGraph = tables.CreateGraph()

' export as a GML document
Dim gml As String = GephiML.BuildGML(g).BuildGML()
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.visualize.Network.IO.Extensions`
- TargetFramework: `net10.0`
- Tags: `scibasic;network-io;cytoscape;gephi;file-format`

## License
GPL-3.0-or-later
