# PageRank
_namespace: [Microsoft.VisualBasic.Data.visualize.Network](./index.md)_

https://github.com/jeffersonhwang/pagerank



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.visualize.Network.PageRank.#ctor(System.Collections.Generic.List{System.Int32}[],System.Double,System.Double,System.Int32)
```
``outGoingLinks(i)`` contains the indices of the pages pointed to by page i.
 (每一行都是指向第i行的页面的index值的集合)

|Parameter Name|Remarks|
|--------------|-------|
|linkMatrix|@``T:Microsoft.VisualBasic.Data.visualize.Network.GraphMatrix``|
|alpha|-|
|convergence|-|
|checkSteps|-|


#### ComputePageRank
```csharp
Microsoft.VisualBasic.Data.visualize.Network.PageRank.ComputePageRank
```
Convenience wrap for the link matrix transpose and the generator.
 See @``M:Microsoft.VisualBasic.Data.visualize.Network.PageRank.PageRankGenerator(System.Collections.Generic.List{System.Int32}[],Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector,System.Collections.Generic.List{System.Int32},System.Double,System.Double,System.Int32)`` method for parameter descriptions

#### PageRankGenerator
```csharp
Microsoft.VisualBasic.Data.visualize.Network.PageRank.PageRankGenerator(System.Collections.Generic.List{System.Int32}[],Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector,System.Collections.Generic.List{System.Int32},System.Double,System.Double,System.Int32)
```
Computes an approximate page rank vector of N pages to within some convergence factor.

|Parameter Name|Remarks|
|--------------|-------|
|at|At a sparse square matrix with N rows. At[i] contains the indices of pages jj linking to i|
|leafNodes|contains the indices of pages without links|
|numLinks|iNumLinks[i] is the number of links going out from i.|
|alpha|a value between 0 and 1. Determines the relative importance of "stochastic" links.|
|convergence|a relative convergence criterion. Smaller means better, but more expensive.|
|checkSteps|check for convergence after so many steps|


#### TransposeLinkMatrix
```csharp
Microsoft.VisualBasic.Data.visualize.Network.PageRank.TransposeLinkMatrix(System.Collections.Generic.List{System.Int32}[])
```
Transposes the link matrix which contains the links from each page. 
 Returns a Tuple of: 
 
 + 1) pages pointing to a given page, 
 + 2) how many links each page contains, and
 + 3) which pages contain no links at all. 
 
 We want to know is which pages

|Parameter Name|Remarks|
|--------------|-------|
|outGoingLinks|``outGoingLinks(i)`` contains the indices of the pages pointed to by page i|


_returns: A tuple of (incomingLinks, numOutGoingLinks, leafNodes)_


