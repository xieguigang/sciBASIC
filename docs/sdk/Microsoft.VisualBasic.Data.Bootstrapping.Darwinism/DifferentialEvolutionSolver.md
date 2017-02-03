# DifferentialEvolutionSolver
_namespace: [Microsoft.VisualBasic.Data.Bootstrapping.Darwinism](./index.md)_

Differential Evolution estimates solver.



### Methods

#### Fitting``1
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.DifferentialEvolutionSolver.Fitting``1(Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut,System.Double,System.Double,System.Double,System.Int32,System.Int32,Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint}@,System.Collections.Generic.Dictionary{System.String,System.Double},System.Collections.Generic.Dictionary{System.String,System.Double},System.String[],System.Boolean,System.Boolean,Microsoft.VisualBasic.Mathematical.IRandomSeeds)
```


|Parameter Name|Remarks|
|--------------|-------|
|observation|-|
|F|-|
|CR|-|
|threshold#|
 现实的曲线太复杂了，因为模型是简单方程，只能够计算出简单的曲线，所以肯定不能完全拟合，
 最终的结果fitness也会较大，默认的0.1的fitness这个要求肯定不能够达到，
 所以只要达到一定次数的迭代就足够了，这个fitness的阈值参数值可以设置大一些
 |
|maxIterations%|-|
|PopulationSize%|-|
|iteratePrints|-|
|initOverrides|-|
|isRefModel|-|
|parallel|并行化计算要在种群的规模足够大的情况下才会有性能上的提升|
|ignores|在计算fitness的时候将要被忽略掉的函数变量的名称|



