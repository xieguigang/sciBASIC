# PreciseRandom
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_

主要针对的是非常小的小数（仅适用于Positive Number）



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Mathematical.PreciseRandom.#ctor(System.Double,System.Double,Microsoft.VisualBasic.Mathematical.IRandomSeeds)
```


|Parameter Name|Remarks|
|--------------|-------|
|from|最小的精度为@``F:System.Double.Epsilon``|
|[to]|-|


#### NextDouble
```csharp
Microsoft.VisualBasic.Mathematical.PreciseRandom.NextDouble(Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange)
```
这个方法可能只适用于很小的数，例如1e-100到1e-10这样子的

|Parameter Name|Remarks|
|--------------|-------|
|range|-|


#### NextNumber
```csharp
Microsoft.VisualBasic.Mathematical.PreciseRandom.NextNumber
```
获取一个在给定的小数位范围内的随机的数


### Properties

#### Epsilon
4.94065645841247E-324
