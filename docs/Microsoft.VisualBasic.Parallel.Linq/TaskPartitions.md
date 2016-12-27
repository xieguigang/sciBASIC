# TaskPartitions
_namespace: [Microsoft.VisualBasic.Parallel.Linq](./index.md)_

对大量的短时间的任务进行分区的操作是在这里完成的



### Methods

#### Partitioning``2
```csharp
Microsoft.VisualBasic.Parallel.Linq.TaskPartitions.Partitioning``2(System.Collections.Generic.IEnumerable{``0},System.Int32,System.Func{``0,``1},System.Func{``0,System.Boolean})
```
进行分区之后返回一个长时间的任务组合

#### PartTokens
```csharp
Microsoft.VisualBasic.Parallel.Linq.TaskPartitions.PartTokens(System.Int32,System.Int32)
```
根据任务总量计算出所需要的线程的数量

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|num_threads|-|

> 假设所有的任务都被平均的分配到每一个线程之上

#### SplitIterator``1
```csharp
Microsoft.VisualBasic.Parallel.Linq.TaskPartitions.SplitIterator``1(System.Collections.Generic.IEnumerable{``0},System.Int32,System.Boolean)
```
Performance the partitioning operation on the input sequence.
 (请注意，这个函数适用于数量非常多的序列。对所输入的序列进行分区操作，**`parTokens`**函数参数是每一个分区里面的元素的数量)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|parTokens|每一个分区之中的元素数量|

> 对于数量较少的序列，可以使用@``M:Microsoft.VisualBasic.Extensions.SplitIterator``1(System.Collections.Generic.IEnumerable{``0},System.Int32,System.Boolean)``进行分区操作，
>  该函数使用数组的@``M:System.Array.ConstrainedCopy(System.Array,System.Int32,System.Array,System.Int32,System.Int32)``方法进行分区复制，效率较高
>  
>  由于本函数需要处理大量的数据，使用Array的方法会内存占用较厉害，所以在这里更改为List操作以降低内存的占用
>  


