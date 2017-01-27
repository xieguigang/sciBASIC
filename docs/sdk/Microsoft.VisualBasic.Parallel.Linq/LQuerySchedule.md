# LQuerySchedule
_namespace: [Microsoft.VisualBasic.Parallel.Linq](./index.md)_

Parallel Linq query library for VisualBasic.
 (用于高效率执行批量查询操作和用于检测操作超时的工具对象，请注意，为了提高查询的工作效率，请尽量避免在查询操作之中生成新的临时对象
 并行版本的LINQ查询和原始的线程操作相比具有一些性能上面的局限性)

> 
>  在使用``Parallel LINQ``的时候，请务必要注意不能够使用Let语句操作共享变量，因为排除死锁的开销比较大
>  
>  在设计并行任务的时候应该遵循的一些原则:
>  
>  1. 假若每一个任务之间都是相互独立的话，则才可以进行并行化调用
>  2. 在当前程序域之中只能够通过线程的方式进行并行化，对于时间较短的任务而言，非并行化会比并行化更加有效率
>  3. 但是对于这些短时间的任务，仍然可以将序列进行分区合并为一个大型的长时间任务来产生并行化
>  4. 对于长时间的任务，可以直接使用并行化Linq拓展执行并行化
>  
>  这个模块主要是针对大量的短时间的任务序列的并行化的，用户可以在这里配置线程的数量自由的控制并行化的程度
>  


### Methods

#### AutoConfig
```csharp
Microsoft.VisualBasic.Parallel.Linq.LQuerySchedule.AutoConfig(System.Int32)
```
假如小于0，则认为是自动配置，0被认为是单线程，反之直接返回

|Parameter Name|Remarks|
|--------------|-------|
|n|-|


#### LQuery``2
```csharp
Microsoft.VisualBasic.Parallel.Linq.LQuerySchedule.LQuery``2(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1},System.Func{``1,System.Boolean},System.Int32)
```
将大量的短时间的任务进行分区，合并，然后再执行并行化

|Parameter Name|Remarks|
|--------------|-------|
|inputs|-|
|task|-|
|outWhere|Processing where test on the output|



### Properties

#### CPU_NUMBER
Get the number of processors on the current machine.(获取当前的系统主机的CPU核心数)
#### Recommended_NUM_THREADS
The possible recommended threads of the linq based on you machine processors number, i'm not sure...
