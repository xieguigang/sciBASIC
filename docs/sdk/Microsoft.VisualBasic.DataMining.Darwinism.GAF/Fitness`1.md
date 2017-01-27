# Fitness`1
_namespace: [Microsoft.VisualBasic.DataMining.Darwinism.GAF](./index.md)_





### Methods

#### Calculate
```csharp
Microsoft.VisualBasic.DataMining.Darwinism.GAF.Fitness`1.Calculate(`0)
```
Assume that chromosome1 is better than chromosome2 
 fit1 = calculate(chromosome1) 
 fit2 = calculate(chromosome2) 
 So the following condition must be true 
 fit1.compareTo(fit2) <= 0 
 (假若是并行模式的之下，还要求这个函数是线程安全的)


