# DijkstraFast
_namespace: [Microsoft.VisualBasic.Data.visualize.Network.Dijkstra.PQDijkstra](./index.md)_

Implements a generalized Dijkstra's algorithm to calculate 
 both minimum distance and minimum path.

>  
>  For this algorithm, all nodes should be provided, and handled 
>  in the delegate methods, including the start and finish nodes. 
>  


### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.visualize.Network.Dijkstra.PQDijkstra.DijkstraFast.#ctor(System.Int32,Microsoft.VisualBasic.Data.visualize.Network.Dijkstra.PQDijkstra.DijkstraFast.InternodeTraversalCost,Microsoft.VisualBasic.Data.visualize.Network.Dijkstra.PQDijkstra.DijkstraFast.NearbyNodesHint)
```
Creates an instance of the @``N:Microsoft.VisualBasic.Data.visualize.Network.Dijkstra`` class.

|Parameter Name|Remarks|
|--------------|-------|
|totalNodeCount__1| 
 The total number of nodes in the graph. 
 |
|traversalCost__2| 
 The delegate that can provide the cost of a transition between 
 any two nodes. 
 |
|hint__3| 
 An optional delegate that can provide a small subset of nodes 
 that a given node may be connected to. 
 |



