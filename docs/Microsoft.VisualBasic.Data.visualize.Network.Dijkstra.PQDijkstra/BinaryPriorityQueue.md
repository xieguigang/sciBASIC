# BinaryPriorityQueue
_namespace: [Microsoft.VisualBasic.Data.visualize.Network.Dijkstra.PQDijkstra](./index.md)_





### Methods

#### Peek
```csharp
Microsoft.VisualBasic.Data.visualize.Network.Dijkstra.PQDijkstra.BinaryPriorityQueue.Peek
```
Get the smallest object without removing it.

_returns: The smallest object_

#### Pop
```csharp
Microsoft.VisualBasic.Data.visualize.Network.Dijkstra.PQDijkstra.BinaryPriorityQueue.Pop
```
Get the smallest object and remove it.

_returns: The smallest object_

#### Push
```csharp
Microsoft.VisualBasic.Data.visualize.Network.Dijkstra.PQDijkstra.BinaryPriorityQueue.Push(System.Object)
```
Push an object onto the PQ

|Parameter Name|Remarks|
|--------------|-------|
|O|The new object|


_returns: The index in the list where the object is _now_. This will change when objects are taken from or put onto the PQ._

#### Update
```csharp
Microsoft.VisualBasic.Data.visualize.Network.Dijkstra.PQDijkstra.BinaryPriorityQueue.Update(System.Int32)
```
Notify the PQ that the object at position i has changed
 and the PQ needs to restore order.
 Since you dont have access to any indexes (except by using the
 explicit IList.this) you should not call this function without knowing exactly
 what you do.

|Parameter Name|Remarks|
|--------------|-------|
|i|The index of the changed object.|



