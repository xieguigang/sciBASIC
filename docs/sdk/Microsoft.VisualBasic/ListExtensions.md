# ListExtensions
_namespace: [Microsoft.VisualBasic](./index.md)_

Initializes a new instance of the @``T:Microsoft.VisualBasic.Language.List`1```1 class that
 contains elements copied from the specified collection and has sufficient capacity
 to accommodate the number of elements copied.



### Methods

#### __reversedTake``1
```csharp
Microsoft.VisualBasic.ListExtensions.__reversedTake``1(System.Collections.Generic.IEnumerable{``0},System.Int32[])
```
反选，即将所有不出现在**`indexs`**之中的元素都选取出来

|Parameter Name|Remarks|
|--------------|-------|
|collection|-|
|indexs|-|


#### Takes``1
```csharp
Microsoft.VisualBasic.ListExtensions.Takes``1(System.Collections.Generic.IEnumerable{``0},System.Int32[],System.Int32,System.Boolean)
```


|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|indexs|所要获取的目标对象的下表的集合|
|reversed|是否为反向选择，即返回所有不在目标index集合之中的元素列表|
|OffSet|当进行反选的时候，本参数将不会起作用|


#### ToList``1
```csharp
Microsoft.VisualBasic.ListExtensions.ToList``1(System.Linq.ParallelQuery{``0})
```
Initializes a new instance of the @``T:Microsoft.VisualBasic.Language.List`1```1 class that
 contains elements copied from the specified collection and has sufficient capacity
 to accommodate the number of elements copied.

|Parameter Name|Remarks|
|--------------|-------|
|linq|The collection whose elements are copied to the new list.|


#### ToList``2
```csharp
Microsoft.VisualBasic.ListExtensions.ToList``2(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1},System.Boolean)
```
Initializes a new instance of the @``T:Microsoft.VisualBasic.Language.List`1```1 class that
 contains elements copied from the specified collection and has sufficient capacity
 to accommodate the number of elements copied.

|Parameter Name|Remarks|
|--------------|-------|
|source|The collection whose elements are copied to the new list.|



