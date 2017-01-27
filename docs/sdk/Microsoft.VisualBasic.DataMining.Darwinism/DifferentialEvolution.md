# DifferentialEvolution
_namespace: [Microsoft.VisualBasic.DataMining.Darwinism](./index.md)_

In evolutionary computation, differential evolution (DE) is a method that optimizes a problem by 
 iteratively trying to improve a candidate solution with regard to a given measure of quality. 
 Such methods are commonly known as metaheuristics as they make few or no assumptions about the 
 problem being optimized and can search very large spaces of candidate solutions. However, 
 metaheuristics such as DE do not guarantee an optimal solution is ever found.
 
 DE Is used For multidimensional real-valued functions but does Not use the gradient Of the problem 
 being optimized, which means DE does Not require For the optimization problem To be differentiable 
 As Is required by classic optimization methods such As gradient descent And quasi-newton methods. 
 DE can therefore also be used On optimization problems that are Not even continuous, are noisy, 
 change over time, etc.[1]
 
 DE optimizes a problem by maintaining a population Of candidate solutions And creating New candidate 
 solutions by combining existing ones according To its simple formulae, And Then keeping whichever 
 candidate solution has the best score Or fitness On the optimization problem at hand. In this way 
 the optimization problem Is treated As a black box that merely provides a measure Of quality given 
 a candidate solution And the gradient Is therefore Not needed.
 
 DE Is originally due To Storn And Price.[2][3] Books have been published On theoretical And practical 
 aspects Of Using DE In parallel computing, multiobjective optimization, constrained optimization, 
 And the books also contain surveys of application areas.[4][5][6][7] Excellent surveys on the 
 multi-faceted research aspects of DE can be found in journal articles Like.[8][9]



### Methods

#### __subPopulationEvolute``1
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.DifferentialEvolution.__subPopulationEvolute``1(``0[],System.Double,System.Int32,System.Double,System.Double,System.Int32,System.Action{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint},System.Func{``0,System.Double},Microsoft.VisualBasic.Mathematical.IRandomSeeds)
```


|Parameter Name|Remarks|
|--------------|-------|
|population|-|
|F#|-|
|N%|-|
|CR#|-|
|bestFit#|-|
|iterates%|i|
|iteratePrints|-|
|fitnessFunction|-|


#### Evolution``1
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.DifferentialEvolution.Evolution``1(System.Func{``0,System.Double},Microsoft.VisualBasic.DataMining.Darwinism.DifferentialEvolution.New{``0},System.Int32,System.Double,System.Double,System.Double,System.Int32,System.Int32,System.Action{Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper.outPrint},System.Boolean,Microsoft.VisualBasic.Mathematical.IRandomSeeds)
```


|Parameter Name|Remarks|
|--------------|-------|
|target|-|
|[new]|How to creates a new **`Individual`**|
|N%|dimensionality of problem, means how many variables problem has.|
|threshold#|-|
|maxIterations%|-|
|F|differential weight [0,2]|
|CR|crossover probability [0,1]|
|PopulationSize%|-|


#### GetPopulation``1
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.DifferentialEvolution.GetPopulation``1(Microsoft.VisualBasic.DataMining.Darwinism.DifferentialEvolution.New{``0},System.Int32,Microsoft.VisualBasic.Mathematical.IRandomSeeds)
```
Initialize population with individuals that have been initialized with uniform random noise
 uniform noise means random value inside your search space

|Parameter Name|Remarks|
|--------------|-------|
|__new|-|
|PopulationSize%|-|



