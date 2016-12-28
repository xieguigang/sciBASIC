# GeneticHelper
_namespace: [Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper](./index.md)_





### Methods

#### Crossover``1
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.GeneticHelper.Crossover``1(System.Random,``0[]@,``0[]@)
```
Returns list of siblings 
 Siblings are actually new chromosomes, 
 created using any of crossover strategy

|Parameter Name|Remarks|
|--------------|-------|
|random|-|
|v1#|-|
|v2#|-|


#### InitialPopulation``1
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.GeneticHelper.InitialPopulation``1(``0,System.Int32,Microsoft.VisualBasic.DataMining.Darwinism.GAF.ParallelComputing{``0})
```
The simplest strategy for creating initial population 
 in real life it could be more complex

#### Mutate
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.GeneticHelper.Mutate(System.Int32[]@,System.Random)
```
Returns clone of current chromosome, which is mutated a bit

|Parameter Name|Remarks|
|--------------|-------|
|v%|-|
|random|-|



