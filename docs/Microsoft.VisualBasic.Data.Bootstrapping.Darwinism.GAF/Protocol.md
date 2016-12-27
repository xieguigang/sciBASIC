# Protocol
_namespace: [Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF](./index.md)_

参数拟合的方法



### Methods

#### __runInternal
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Protocol.__runInternal(System.String[],System.Int32,System.Double,System.Int32,Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.GAFFitness,Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint}@,System.Collections.Generic.Dictionary{System.String,System.Double},Microsoft.VisualBasic.Mathematical.IRandomSeeds,Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.MutateLevels,System.Action{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint,Microsoft.VisualBasic.Mathematical.Calculus.var[]},System.Double,Microsoft.VisualBasic.DataMining.Darwinism.GAF.ParallelComputing{Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ParameterVector})
```


|Parameter Name|Remarks|
|--------------|-------|
|vars$|从模型内部定义所解析出来的需要进行拟合的参数的名称列表|
|popSize%|-|
|threshold#|-|
|evolIterations%|-|
|fitness|-|
|outPrint|-|
|argsInit|-|


#### Fitting
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Protocol.Fitting(Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.Model,System.Int32,System.Double,System.Double,System.Int32,System.Int32,Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint}@,System.Double,System.Collections.Generic.Dictionary{System.String,System.Double},System.Boolean,Microsoft.VisualBasic.Mathematical.IRandomSeeds,Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.MutateLevels,System.Action{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint,Microsoft.VisualBasic.Mathematical.Calculus.var[]},System.Double,Microsoft.VisualBasic.DataMining.Darwinism.GAF.ParallelComputing{Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ParameterVector})
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
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Protocol.Fitting``1(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{System.Double[]}},System.Double[],System.Int32,System.Int32,Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint}@,System.Double,System.Boolean,System.String[],System.Collections.Generic.Dictionary{System.String,System.Double},System.Collections.Generic.Dictionary{System.String,System.Double},System.Boolean,Microsoft.VisualBasic.Mathematical.IRandomSeeds,Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.MutateLevels,System.Action{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint,Microsoft.VisualBasic.Mathematical.Calculus.var[]},System.Double,Microsoft.VisualBasic.DataMining.Darwinism.GAF.ParallelComputing{Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ParameterVector})
```
用于实际分析的GAF工具

|Parameter Name|Remarks|
|--------------|-------|
|observation|用于进行拟合的目标真实的实验数据，模型计算所使用的y0初值从这里面来|
|popSize%|-|
|evolIterations%|-|
|outPrint|-|
|threshold#|-|
|log10Fit|-|

> 
>  ###### 2016-11-28
>  一般情况下，**`log10Fit`**会导致曲线失真，所以默认关闭这个参数
>  

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



