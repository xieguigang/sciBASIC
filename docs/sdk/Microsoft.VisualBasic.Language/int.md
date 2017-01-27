# int
_namespace: [Microsoft.VisualBasic.Language](./index.md)_

Alias of @``T:System.Int32``



### Methods

#### CompareTo
```csharp
Microsoft.VisualBasic.Language.int.CompareTo(System.Object)
```
Compare @``T:Microsoft.VisualBasic.Language.int`` or @``T:System.Int32``

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### op_Addition
```csharp
Microsoft.VisualBasic.Language.int.op_Addition(Microsoft.VisualBasic.Language.int,System.Int32)
```
对于@``T:Microsoft.VisualBasic.Language.int``类型而言，其更加侧重于迭代器中的位移，所以这个加法运算是符合
 ```vbnet
 x += n
 ```
 
 但是对于@``T:Microsoft.VisualBasic.Language.float``类型而言，其更加侧重于模型计算，所以其加法不符合上述的语法，
 不会修改源变量的值，返回的是一个单纯的@``T:System.Double``值类型

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|n%|-|


#### op_GreaterThan
```csharp
Microsoft.VisualBasic.Language.int.op_GreaterThan(Microsoft.VisualBasic.Language.int,System.Int32)
```
``x.value > n``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|n|-|


#### op_Implicit
```csharp
Microsoft.VisualBasic.Language.int.op_Implicit(System.Int32)~Microsoft.VisualBasic.Language.int
```
必须要overloads这个方法，否则会出现无法将Value(Of Integer)转换为int的错误

|Parameter Name|Remarks|
|--------------|-------|
|n|-|


#### op_LeftShift
```csharp
Microsoft.VisualBasic.Language.int.op_LeftShift(Microsoft.VisualBasic.Language.int,System.Int32)
```
p的值增加x，然后返回之前的值

|Parameter Name|Remarks|
|--------------|-------|
|p|-|
|x|-|


#### op_LessThan
```csharp
Microsoft.VisualBasic.Language.int.op_LessThan(Microsoft.VisualBasic.Language.int,System.Int32)
```
``x.value < n``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|n|-|


#### op_UnaryPlus
```csharp
Microsoft.VisualBasic.Language.int.op_UnaryPlus(Microsoft.VisualBasic.Language.int)
```
自增1然后返回之前的值

|Parameter Name|Remarks|
|--------------|-------|
|x|-|



