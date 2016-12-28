# BifurcationAnalysis
_namespace: [Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo](./index.md)_

Search for all possible system status clusters

> 
>  ###### Figure 3: Bifurcation analysis.
>  
>  > For Each important parameters, performing bifurcation analysis to (1) find out 
>  > how many possible stable steady states in the system (model), for example, for 
>  > example, V, may have more than two states such as very low amount (may Not 
>  > enough to cause symptoms), very high amount (immediately cause symptoms), And 
>  > mediate amount (may have long “latency” period in the cell); (2) find out how 
>  > Virus (V, Or all the other five species) changes with its parameters' change 
>  > (the parameter regions). For easy understanding, for example, in some parameter 
>  > region, virus amount would decrease/increase significantly; whereas in another 
>  > region, V would have interesting phenomenon such as oscillating. 
>  


### Methods

#### __clusterInternal
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.BifurcationAnalysis.__clusterInternal(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut},System.String[],System.Int32,System.Int32,System.Func{Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut,System.String})
```


|Parameter Name|Remarks|
|--------------|-------|
|validResults|-|
|ys$|-|
|ncluster%|-|
|stop%|-|
|uidProvider|
 都是从@``T:System.Collections.Generic.Dictionary`2``类型json序列化而来，只不过数据的来源不同，
 + 对于y变量而言，来源于@``P:Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut.y0``，
 + 对于方程参数而言，来源于@``P:Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut.params``
 |


_returns: 
 由于不同的组合也可能产生相同的系统状态，所以在这里是不是还需要做进一步的聚类？
 从这里populates一个可能的系统状态的范围
 _

#### GetRandomRange
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.BifurcationAnalysis.GetRandomRange(System.Double,System.Double,System.Double,Microsoft.VisualBasic.Mathematical.IRandomSeeds)
```


|Parameter Name|Remarks|
|--------------|-------|
|x#|通过拟合所得到的一个具体的值|
|ldelta#|小数位往下浮动多少|
|udelta#|小数位往上浮动多少|
|rnd|-|


#### KMeansCluster
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.BifurcationAnalysis.KMeansCluster(System.Type,System.Int64,System.Int32,System.Double,System.Double,System.Collections.Generic.Dictionary{System.String,System.Double},System.Int32,System.Int32)
```
Search for all possible system status clusters by using MonteCarlo method from random system inits.
 (在参数固定不变的情况下，使用不同的y变量初始值来计算，使用蒙特卡洛的方法来搜索可能的系统状态空间)

|Parameter Name|Remarks|
|--------------|-------|
|model|-|


_returns: 可能的系统状态的KMeans聚类结果_

#### Run
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.BifurcationAnalysis.Run(System.Type,Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut,System.String,Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange,System.Int32,System.Boolean)
```
返回来的结果是按照突变的参数进行从小到大排序了的

|Parameter Name|Remarks|
|--------------|-------|
|model|-|
|base|-|
|param$|-|
|range|-|
|n%|-|
|parallel|-|



