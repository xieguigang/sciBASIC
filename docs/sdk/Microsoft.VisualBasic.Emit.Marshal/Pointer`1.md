# Pointer`1
_namespace: [Microsoft.VisualBasic.Emit.Marshal](./index.md)_





### Methods

#### MoveNext
```csharp
Microsoft.VisualBasic.Emit.Marshal.Pointer`1.MoveNext
```
Pointer move to next and then returns is @``P:Microsoft.VisualBasic.Emit.Marshal.Pointer`1.EndRead``

#### op_GreaterThan
```csharp
Microsoft.VisualBasic.Emit.Marshal.Pointer`1.op_GreaterThan(Microsoft.VisualBasic.Emit.Marshal.Pointer{`0},System.Int32)
```
后移**`offset`**个单位，然后返回值，这个和Peek的作用一样，不会改变指针位置

|Parameter Name|Remarks|
|--------------|-------|
|p|-|
|offset|-|


#### op_LessThan
```csharp
Microsoft.VisualBasic.Emit.Marshal.Pointer`1.op_LessThan(Microsoft.VisualBasic.Emit.Marshal.Pointer{`0},System.Int32)
```
前移**`offset`**个单位，然后返回值，这个和Peek的作用一样，不会改变指针位置

|Parameter Name|Remarks|
|--------------|-------|
|p|-|
|offset|-|


#### op_UnaryPlus
```csharp
Microsoft.VisualBasic.Emit.Marshal.Pointer`1.op_UnaryPlus(Microsoft.VisualBasic.Emit.Marshal.Pointer{`0})
```
Pointer move to next and then returns the previous value

|Parameter Name|Remarks|
|--------------|-------|
|ptr|-|



### Properties

#### Current
@``P:Microsoft.VisualBasic.Emit.Marshal.Pointer`1.Pointer`` -> its current value
#### EndRead
Is read to end?
#### Length
Memory block size
#### Pointer
Current read position
#### Raw
Raw memory of this pointer
#### Value
相对于当前的指针的位置而言的
