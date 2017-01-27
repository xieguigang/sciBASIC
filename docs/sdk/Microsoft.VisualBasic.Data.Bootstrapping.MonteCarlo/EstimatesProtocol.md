# EstimatesProtocol
_namespace: [Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo](./index.md)_

使用蒙特卡洛的方法估算出系统的参数，不过这个方法的效率太低了，没有遗传算法的效率好



### Methods

#### DllParser
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.EstimatesProtocol.DllParser(System.String)
```
加载dll文件之中的计算模型

|Parameter Name|Remarks|
|--------------|-------|
|dll|-|


#### GetEigenvector
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.EstimatesProtocol.GetEigenvector(System.Type)
```
Sampling method of the y output values.(假若模型定义之中没有定义这个特征向量的构建方法的话，则使用默认的方法：平均数+标准差)

|Parameter Name|Remarks|
|--------------|-------|
|def|-|


#### Iterations
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.EstimatesProtocol.Iterations(System.String,Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut,System.Int64,System.Int32,System.Int32,System.Int32,System.Double,System.String)
```
k是采样的次数， n,a,b 是进行ODEs计算的参数，可以直接从观测数据之中提取出来，**`expected`**是期望的cluster数量

|Parameter Name|Remarks|
|--------------|-------|
|dll|-|
|observation|实验观察里面只需要y值列表就足够了，不需要参数信息|
|k|-|
|expected|-|
|[stop]|-|
|work|工作的临时文件夹工作区间，默认使用dll的文件夹|


_returns: 函数返回收敛成功了之后的最后一次迭代的参数数据_

#### Run
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.EstimatesProtocol.Run(System.String,System.Int64,System.Int32,System.Int32,System.Int32)
```
加载目标dll之中的计算模型然后提供计算数据

|Parameter Name|Remarks|
|--------------|-------|
|dll|-|
|k|-|
|n|-|
|a|-|
|b|-|


#### Sampling
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.EstimatesProtocol.Sampling(Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut,System.Collections.Generic.Dictionary{System.String,Microsoft.VisualBasic.Data.Bootstrapping.Eigenvector},System.Int32,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|eigenvector|-|
|partN|-|



