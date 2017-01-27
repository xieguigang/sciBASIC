# Bootstraping
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_

Data sampling bootstrapping extensions



### Methods

#### DeviationStandardization
```csharp
Microsoft.VisualBasic.Mathematical.Bootstraping.DeviationStandardization(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector)
```
###### 0-1标准化(0-1 normalization)
 也叫离差标准化，是对原始数据的线性变换，使结果落到[0,1]区间
 其中max为样本数据的最大值，min为样本数据的最小值。这种方法有一个缺陷就是当有新数据加入时，可能导致max和min的变化，需要重新定义。

|Parameter Name|Remarks|
|--------------|-------|
|x|-|

> 
>  数据的标准化（normalization）是将数据按比例缩放，使之落入一个小的特定区间。这样去除数据的单位限制，
>  将其转化为无量纲的纯数值，便于不同单位或量级的指标能够进行比较和加权。
>  其中最典型的就是0-1标准化和Z标准化
>  

#### Distributes
```csharp
Microsoft.VisualBasic.Mathematical.Bootstraping.Distributes(System.Collections.Generic.IEnumerable{System.Double},System.Single)
```
返回来的标签数据之中的标签是在某个区间范围内的数值集合的平均值

|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|base|-|


#### Logistic
```csharp
Microsoft.VisualBasic.Mathematical.Bootstraping.Logistic(System.Double,System.Double,System.Double,System.Double)
```
A logistic function or logistic curve is a common "S" shape (sigmoid curve)
 > https://en.wikipedia.org/wiki/Logistic_function

|Parameter Name|Remarks|
|--------------|-------|
|L#|the curve's maximum value|
|x#|current x value|
|x0#|the x-value of the sigmoid's midpoint,|
|k#|the steepness of the curve.|


#### ProbabilityDensity
```csharp
Microsoft.VisualBasic.Mathematical.Bootstraping.ProbabilityDensity(System.Double,System.Double,System.Double)
```
Normal Distribution.(正态分布)

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|m|Mean|
|sd|-|


#### Samples``1
```csharp
Microsoft.VisualBasic.Mathematical.Bootstraping.Samples``1(System.Collections.Generic.IEnumerable{``0},System.Int32,System.Int32)
```
bootstrap是一种非参数估计方法，它用到蒙特卡洛方法。bootstrap算法如下：
 假设样本容量为N

 + 有放回的从样本中随机抽取N次(所以可能x1..xn中有的值会被抽取多次)，每次抽取一个元素。并将抽到的元素放到集合S中；
 + 重复**步骤1** B次（例如``B = 100``）， 得到B个集合， 记作S1, S2,…, SB;
 + 对每个Si （i=1,2,…,B），用蒙特卡洛方法估计随机变量的数字特征d，分别记作d1,d2,…,dB;
 + 用d1,d2,…dB来近似d的分布；
 
 本质上，bootstrap算法是最大似然估计的一种实现，它和最大似然估计相比的优点在于，它不需要用参数来刻画总体分布。

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|N|-|
|B|-|


#### StandardDistribution
```csharp
Microsoft.VisualBasic.Mathematical.Bootstraping.StandardDistribution(System.Double)
```
标准正态分布, delta = 1, u = 0

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### TruncNDist
```csharp
Microsoft.VisualBasic.Mathematical.Bootstraping.TruncNDist(System.Int32,System.Double)
```


|Parameter Name|Remarks|
|--------------|-------|
|len|-|
|sd|-|

> https://github.com/mpadge/tnorm

#### Z
```csharp
Microsoft.VisualBasic.Mathematical.Bootstraping.Z(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector)
```


|Parameter Name|Remarks|
|--------------|-------|
|x|-|

> 
>  http://blog.163.com/huai_jing@126/blog/static/171861983201321074124426/
>  


