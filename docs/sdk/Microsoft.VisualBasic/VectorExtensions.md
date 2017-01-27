# VectorExtensions
_namespace: [Microsoft.VisualBasic](./index.md)_





### Methods

#### After``1
```csharp
Microsoft.VisualBasic.VectorExtensions.After``1(System.Collections.Generic.IEnumerable{``0},System.Predicate{``0})
```
取出在判定条件成立的元素之后的所有元素

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|predicate|-|


#### GetIndexes``1
```csharp
Microsoft.VisualBasic.VectorExtensions.GetIndexes``1(``0[],System.Func{``0,System.Boolean})
```
查找出列表之中符合条件的所有的索引编号

|Parameter Name|Remarks|
|--------------|-------|
|array|-|
|condi|-|


#### InsideAny
```csharp
Microsoft.VisualBasic.VectorExtensions.InsideAny(Microsoft.VisualBasic.ComponentModel.Ranges.IntRange,System.Collections.Generic.IEnumerable{System.Int32})
```


|Parameter Name|Remarks|
|--------------|-------|
|range|-|
|sites|-|


#### Last``1
```csharp
Microsoft.VisualBasic.VectorExtensions.Last``1(System.Collections.Generic.IEnumerable{``0},System.Int32)
```
从后往前访问集合之中的元素，请注意请不要使用Linq查询表达式，尽量使用``list``或者``array``

|Parameter Name|Remarks|
|--------------|-------|
|source|请不要使用Linq查询表达式，尽量使用``list``或者``array``|
|index|-|


#### LoadDblArray
```csharp
Microsoft.VisualBasic.VectorExtensions.LoadDblArray(System.String)
```
Each line in the text file should be a @``T:System.Double`` type numeric value.

|Parameter Name|Remarks|
|--------------|-------|
|path|-|


#### Midv``1
```csharp
Microsoft.VisualBasic.VectorExtensions.Midv``1(System.Collections.Generic.IEnumerable{``0},System.Int32,System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|start|0 base|
|length|-|


#### Split``1
```csharp
Microsoft.VisualBasic.VectorExtensions.Split``1(System.Collections.Generic.IEnumerable{``0},System.Func{``0,System.Boolean})
```


|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|delimiter|和字符串的Split函数一样，这里作为delimiter的元素都不会出现在结果之中|



