# Correlations
_namespace: [Microsoft.VisualBasic.Mathematical.Correlations](./index.md)_





### Methods

#### GetPearson
```csharp
Microsoft.VisualBasic.Mathematical.Correlations.Correlations.GetPearson(System.Double[],System.Double[],System.Double@,System.Double@,System.Double@)
```


|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|y|-|
|prob|-|
|prob2|-|
|z|-|

> 
>  checked by Excel
>  

#### kendallTauBeta
```csharp
Microsoft.VisualBasic.Mathematical.Correlations.Correlations.kendallTauBeta(System.Double[],System.Double[])
```
Provides rank correlation coefficient metrics Kendall tau

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|y|-|

> 
>  https://github.com/felipebravom/RankCorrelation
>  

#### rankKendallTauBeta
```csharp
Microsoft.VisualBasic.Mathematical.Correlations.Correlations.rankKendallTauBeta(System.Double[],System.Double[])
```
Provides rank correlation coefficient metrics Kendall tau

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|y|-|

> 
>  https://github.com/felipebravom/RankCorrelation
>  

#### Spearman
```csharp
Microsoft.VisualBasic.Mathematical.Correlations.Correlations.Spearman(System.Double[],System.Double[])
```
This method should not be used in cases where the data set is truncated; that is,
 when the Spearman correlation coefficient is desired for the top X records
 (whether by pre-change rank or post-change rank, or both), the user should use the
 Pearson correlation coefficient formula given above.
 (斯皮尔曼相关性)

|Parameter Name|Remarks|
|--------------|-------|
|X|-|
|Y|-|

> 
>  https://en.wikipedia.org/wiki/Spearman%27s_rank_correlation_coefficient
>  checked!
>  

#### SW
```csharp
Microsoft.VisualBasic.Mathematical.Correlations.Correlations.SW(System.Double[],System.Double[])
```
假若所有的元素都是0-1之间的话，结果除以2可以得到相似度

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|y|-|



### Properties

#### TINY
will regularize the unusual case of complete correlation
