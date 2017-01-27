# Protocol
_namespace: [Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF](./index.md)_

参数拟合的方法



### Methods

#### __runInternal
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Protocol.__runInternal(System.String[],System.Int32,System.Double,System.Int32,Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Driver.GAFFitness,Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint}@,System.Collections.Generic.Dictionary{System.String,System.Double},Microsoft.VisualBasic.Mathematical.IRandomSeeds,Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.MutateLevels,System.Action{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint,Microsoft.VisualBasic.Mathematical.Calculus.var[]},System.Double,Microsoft.VisualBasic.DataMining.Darwinism.GAF.ParallelComputing{Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ParameterVector},System.Collections.Generic.Dictionary{System.String,System.Double})
```


|Parameter Name|Remarks|
|--------------|-------|
|vars$|从模型内部定义所解析出来的需要进行拟合的参数的名称列表|
|popSize%|-|
|threshold#|-|
|evolIterations%|-|
|fitness|-|
|outPrint|-|
|base|-|
|weights|Weights for variable fitness calcaulation|


#### Fitting
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Protocol.Fitting(Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.Model,System.Int32,System.Double,System.Double,System.Int32,System.Int32,Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint}@,System.Double,System.Collections.Generic.Dictionary{System.String,System.Double},System.Boolean,Microsoft.VisualBasic.Mathematical.IRandomSeeds,Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.MutateLevels,System.Action{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint,Microsoft.VisualBasic.Mathematical.Calculus.var[]},System.Double,Microsoft.VisualBasic.DataMining.Darwinism.GAF.ParallelComputing{Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ParameterVector},System.Collections.Generic.Dictionary{System.String,System.Double})
```
Using for model testing debug.(测试用)

|Parameter Name|Remarks|
|--------------|-------|
|model|-|
|n%|-|
|a#|-|
|b#|-|
|popSize%|-|
|evolIterations%|-|
|outPrint|-|


#### Fitting``1
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Protocol.Fitting``1(Microsoft.VisualBasic.DataMining.Darwinism.GAF.Fitness{Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ParameterVector},System.Int32,System.Int32,Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint}@,System.Double,System.Collections.Generic.Dictionary{System.String,System.Double},Microsoft.VisualBasic.Mathematical.IRandomSeeds,Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.MutateLevels,System.Action{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint,Microsoft.VisualBasic.Mathematical.Calculus.var[]},System.Double,Microsoft.VisualBasic.DataMining.Darwinism.GAF.ParallelComputing{Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ParameterVector})
```
用于实际分析的GAF工具

|Parameter Name|Remarks|
|--------------|-------|
|popSize%|
 更小的种群规模能够产生更快的进化速度，更大的种群规模能够产生更多的解集
 |
|evolIterations%|-|
|outPrint|-|
|threshold#|-|
|radicals|参数值介于[0-1]之间|


#### Mutate
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Protocol.Mutate(System.Double[]@,System.Random,System.Double)
```
Mutate a bit in an array.

|Parameter Name|Remarks|
|--------------|-------|
|array#|The abstraction of a chromosome(parameter list).
 (需要被拟合的参数列表，在这个函数里面会被修改一点产生突变)
 |
|rnd|-|


#### y0
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Protocol.y0(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{System.Double[]}})
```
Gets the first value as ``y0`` from the inputs samples

|Parameter Name|Remarks|
|--------------|-------|
|data|-|



