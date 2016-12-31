# GeneticAlgorithm`1
_namespace: [Microsoft.VisualBasic.DataMining.Darwinism.GAF](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.GAF.GeneticAlgorithm`1.#ctor(Microsoft.VisualBasic.DataMining.Darwinism.GAF.Population{`0},Microsoft.VisualBasic.DataMining.Darwinism.GAF.Fitness{`0},Microsoft.VisualBasic.Mathematical.IRandomSeeds)
```


|Parameter Name|Remarks|
|--------------|-------|
|population|-|
|fitnessFunc|
 Calculates the fitness of the mutated chromesome in **`population`**
 |
|seeds|-|


#### __iterate
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.GAF.GeneticAlgorithm`1.__iterate(System.Int32)
```
并行化过程之中的单个迭代

|Parameter Name|Remarks|
|--------------|-------|
|i%|-|


#### Clear
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.GAF.GeneticAlgorithm`1.Clear
```
Clear the internal cache


### Properties

#### iterationListeners
listeners of genetic algorithm iterations (handle callback afterwards)
#### ParentChromosomesSurviveCount
Number of parental chromosomes, which survive (and move to new
 population)
