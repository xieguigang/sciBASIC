# KMeansAlgorithm
_namespace: [Microsoft.VisualBasic.DataMining.KMeans](./index.md)_

This class implement a KMeans clustering algorithm.(请注意，实体对象的属性必须要长度一致)



### Methods

#### ClusterDataSet``1
```csharp
Microsoft.VisualBasic.DataMining.KMeans.KMeansAlgorithm.ClusterDataSet``1(Microsoft.VisualBasic.DataMining.KMeans.ClusterCollection{``0},``0[],System.Boolean)
```
Seperates a dataset into clusters or groups with similar characteristics

|Parameter Name|Remarks|
|--------------|-------|
|clusters|A collection of data clusters|
|data|An array containing data to be clustered|
|parallel|是否采用并行算法|


_returns: A collection of clusters of data_

#### ClusterMean
```csharp
])
```
Calculates The Mean Of A Cluster OR The Cluster Center
 
 ```vbnet
 Dim cluster#(,) = {
 {15, 32, 35.6},
 {19, 54, 65.1}
 }
 Dim centroid#() = Kmeans.ClusterMean(cluster)

 Call $"Cluster mean Calc: {centroid}".__DEBUG_ECHO
 ```

|Parameter Name|Remarks|
|--------------|-------|
|cluster|
 A two-dimensional array containing a dataset of numeric values
 |


_returns: 
 Returns an Array Defining A Data Point Representing The Cluster Mean or Centroid
 _

#### EuclideanDistance
```csharp
Microsoft.VisualBasic.DataMining.KMeans.KMeansAlgorithm.EuclideanDistance(System.Double[],System.Double[])
```
Calculates the Euclidean Distance Measure between two data points

|Parameter Name|Remarks|
|--------------|-------|
|X|An array with the values of an object or datapoint|
|Y|An array with the values of an object or datapoint|


_returns: Returns the Euclidean Distance Measure Between Points X and Points Y_

#### ManhattanDistance
```csharp
Microsoft.VisualBasic.DataMining.KMeans.KMeansAlgorithm.ManhattanDistance(System.Double[],System.Double[])
```
Calculates the Manhattan Distance Measure between two data points

|Parameter Name|Remarks|
|--------------|-------|
|X|An array with the values of an object or datapoint|
|Y|An array with the values of an object or datapoint|


_returns: Returns the Manhattan Distance Measure Between Points X and Points Y_

#### ToFloatMatrix
```csharp
Microsoft.VisualBasic.DataMining.KMeans.KMeansAlgorithm.ToFloatMatrix(System.Data.DataTable)
```
Converts a System.Data.DataTable to a 2-dimensional array

|Parameter Name|Remarks|
|--------------|-------|
|table|A System.Data.DataTable containing data to cluster|


_returns: A 2-dimensional array containing data to cluster_


