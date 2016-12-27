# FuzzyCMeansAlgorithm
_namespace: [Microsoft.VisualBasic.DataMining.FuzzyCMeans](./index.md)_

**Fuzzy clustering** (also referred to as **soft clustering**) is a form of clustering in which 
 each data point can belong to more than one cluster.

 Clustering Or cluster analysis involves assigning data points to clusters (also called buckets, 
 bins, Or classes), Or homogeneous classes, such that items in the same class Or cluster are as 
 similar as possible, while items belonging to different classes are as dissimilar as possible. 
 Clusters are identified via similarity measures. These similarity measures include distance, 
 connectivity, And intensity. Different similarity measures may be chosen based on the data Or 
 the application.
 
 > https://en.wikipedia.org/wiki/Fuzzy_clustering

> 
>  Clustering problems have applications in **biology**, medicine, psychology, economics, and many other disciplines.
> 
>  ##### Bioinformatics
>  
>  In the field of bioinformatics, clustering Is used for a number of applications. One use Is as 
>  a pattern recognition technique to analyze gene expression data from microarrays Or other 
>  technology. In this case, genes with similar expression patterns are grouped into the same cluster, 
>  And different clusters display distinct, well-separated patterns of expression. Use of clustering 
>  can provide insight into gene function And regulation. Because fuzzy clustering allows genes 
>  to belong to more than one cluster, it allows for the identification of genes that are conditionally 
>  co-regulated Or co-expressed. For example, one gene may be acted on by more than one Transcription 
>  factor, And one gene may encode a protein that has more than one function. Thus, fuzzy clustering 
>  Is more appropriate than hard clustering.
>  


### Methods

#### FuzzyCMeans
```csharp
Microsoft.VisualBasic.DataMining.FuzzyCMeans.FuzzyCMeansAlgorithm.FuzzyCMeans(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.DataMining.FuzzyCMeans.Entity},System.Int32,System.Double,System.Int32,System.Double,System.Collections.Generic.Dictionary{System.Int32,Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.DataMining.FuzzyCMeans.Entity}}@)
```
**Fuzzy clustering** (also referred to as **soft clustering**) is a form of clustering in which 
 each data point can belong to more than one cluster.

|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|numberOfClusters%|-|
|fuzzificationParameter#|This parameter value should greater than **1.0**|
|maxIterates%|-|
|threshold#|-|
|trace|-|


#### MarkClusterCenter
```csharp
Microsoft.VisualBasic.DataMining.FuzzyCMeans.FuzzyCMeansAlgorithm.MarkClusterCenter(Microsoft.VisualBasic.DataMining.FuzzyCMeans.Entity,System.Drawing.Color)
```
·½±ãÓ¦ÓÃÓÚ¿ÉÊÓ»¯

|Parameter Name|Remarks|
|--------------|-------|
|clusterCenterPoint|-|
|color|-|



