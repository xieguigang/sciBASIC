# Tree
_namespace: [Microsoft.VisualBasic.DataMining.KMeans](./index.md)_

KMeans.Tree.NET



### Methods

#### __buildNET
```csharp
Microsoft.VisualBasic.DataMining.KMeans.Tree.__buildNET(Microsoft.VisualBasic.DataMining.KMeans.Tree.__edgePath[],Microsoft.VisualBasic.Data.visualize.Network.FileStream.Node,System.Int32,Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.Data.visualize.Network.FileStream.Node}@)
```
从某一个分支点下来

|Parameter Name|Remarks|
|--------------|-------|
|array|-|
|depth|-|
|nodes|-|


#### __firstCluster``1
```csharp
Microsoft.VisualBasic.DataMining.KMeans.Tree.__firstCluster``1(System.Collections.Generic.IEnumerable{``0},System.Int32,System.Int32)
```
两条线程并行化进行二叉树聚类

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|[stop]|-|


#### bTreeNET
```csharp
Microsoft.VisualBasic.DataMining.KMeans.Tree.bTreeNET(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.DataMining.KMeans.EntityLDM})
```
Create network model for visualize the binary tree clustering result.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### Partitioning
```csharp
Microsoft.VisualBasic.DataMining.KMeans.Tree.Partitioning(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.DataMining.KMeans.EntityLDM},System.Int32,System.Boolean)
```


|Parameter Name|Remarks|
|--------------|-------|
|cluster|-|
|depth|将会以最短的聚类作为数据分区的深度|


#### TreeCluster
```csharp
Microsoft.VisualBasic.DataMining.KMeans.Tree.TreeCluster(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.DataMining.KMeans.Entity},System.Boolean,System.Int32)
```
二叉树聚类的路径会在@``P:Microsoft.VisualBasic.DataMining.KMeans.Entity.uid``上面出现

|Parameter Name|Remarks|
|--------------|-------|
|source|函数会在这里自动调用ToArray方法结束Linq查询|
|parallel|-|
|stop|Max iteration number for the kmeans kernel|



