# KMeansCluster`1
_namespace: [Microsoft.VisualBasic.DataMining.KMeans](./index.md)_

A class containing a group of data with similar characteristics (cluster), KMeans Cluster



### Methods

#### Add
```csharp
Microsoft.VisualBasic.DataMining.KMeans.KMeansCluster`1.Add(`0)
```
Adds a single dimension array data to the cluster.
 (请注意，每当使用这个方法新添加一个对象的时候，都会导致均值被重新计算)

|Parameter Name|Remarks|
|--------------|-------|
|data|A 1-dimensional array containing data that will be added to the cluster|


#### refresh
```csharp
Microsoft.VisualBasic.DataMining.KMeans.KMeansCluster`1.refresh
```
Will keep the center member variable, but clear the list of points
 within the cluster.


### Properties

#### ClusterMean
The mean of all the data in the cluster
#### ClusterSum
The sum of all the data in the cluster
#### Item
Returns the one dimensional array data located at the index
