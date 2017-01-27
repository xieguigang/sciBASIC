# Population`1
_namespace: [Microsoft.VisualBasic.DataMining.Darwinism.GAF](./index.md)_





### Methods

#### Add
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.GAF.Population`1.Add(`0)
```
Add chromosome

|Parameter Name|Remarks|
|--------------|-------|
|chromosome|-|


#### GA_PLinq
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.GAF.Population`1.GA_PLinq(Microsoft.VisualBasic.DataMining.Darwinism.GAF.GeneticAlgorithm{`0},Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{`0}[])
```
使用PLinq进行并行计算

|Parameter Name|Remarks|
|--------------|-------|
|GA|-|
|source|-|


#### SortPopulationByFitness
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.GAF.Population`1.SortPopulationByFitness(Microsoft.VisualBasic.DataMining.Darwinism.GAF.GeneticAlgorithm{`0},Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ChromosomesComparator{`0})
```
这里是ODEs参数估计的限速步骤

|Parameter Name|Remarks|
|--------------|-------|
|GA|-|
|comparator|-|


#### Trim
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.GAF.Population`1.Trim(System.Int32)
```
shortening population till specific number


### Properties

#### Item
Gets chromosome by index
#### Parallel
是否使用并行模式在排序之前来计算出fitness
#### Random
Gets random chromosome
#### Size
The number of chromosome elements in the inner list
