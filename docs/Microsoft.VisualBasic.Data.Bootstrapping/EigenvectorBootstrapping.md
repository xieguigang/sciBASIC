# EigenvectorBootstrapping
_namespace: [Microsoft.VisualBasic.Data.Bootstrapping](./index.md)_





### Methods

#### DefaultEigenvector
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.EigenvectorBootstrapping.DefaultEigenvector(System.Double[])
```
默认的特征向量: ``{data.Average, data.StdError}``

|Parameter Name|Remarks|
|--------------|-------|
|data|-|


#### KMeans
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.EigenvectorBootstrapping.KMeans(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.ComponentModel.TagData.VectorTagged{System.Collections.Generic.Dictionary{System.String,System.Double}}},System.Int32,System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|data|@``M:Microsoft.VisualBasic.Data.Bootstrapping.EigenvectorBootstrapping.LoadData(System.String,System.Collections.Generic.Dictionary{System.String,Microsoft.VisualBasic.Data.Bootstrapping.Eigenvector},System.Int32)``的输出数据|
|n|所期望的Kmeans集合的数量|


#### LoadData
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.EigenvectorBootstrapping.LoadData(System.String,System.Collections.Generic.Dictionary{System.String,Microsoft.VisualBasic.Data.Bootstrapping.Eigenvector},System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|DIR|-|
|eigenvector|-|
|partN|将原始数据分解为多少个数据块来抽取特征向量从而进行数据采样|



