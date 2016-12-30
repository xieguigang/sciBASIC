# GraphAPI
_namespace: [Microsoft.VisualBasic.Data.visualize.Network.FileStream](./index.md)_

Data Model Extensions



### Methods

#### CreateGraph``2
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.GraphAPI.CreateGraph``2(Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network{``0,``1})
```
Transform the network data model to graph model

|Parameter Name|Remarks|
|--------------|-------|
|net|-|


#### CytoscapeExportAsGraph
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.GraphAPI.CytoscapeExportAsGraph(System.String,System.String)
```
Load cytoscape exports as network graph model.

|Parameter Name|Remarks|
|--------------|-------|
|edgesDf|``edges.csv``|
|nodesDf|``nodes.csv``|


#### RemovesByDegree
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.GraphAPI.RemovesByDegree(Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network,System.Int32,System.String[]@)
```
直接按照节点的``Degree``来筛选

|Parameter Name|Remarks|
|--------------|-------|
|net|-|
|degree%|``@``T:Microsoft.VisualBasic.Data.visualize.Network.FileStream.Node`` -> "Degree"``|
|removeIDs$|-|


#### RemovesByDegreeQuantile
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.GraphAPI.RemovesByDegreeQuantile(Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network,System.Double,System.String[]@)
```
默认移除degree少于10% quantile的节点

|Parameter Name|Remarks|
|--------------|-------|
|net|-|
|quantile#|-|



