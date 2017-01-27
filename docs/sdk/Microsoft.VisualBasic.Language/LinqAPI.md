# LinqAPI
_namespace: [Microsoft.VisualBasic.Language](./index.md)_

Language syntax extension for the Linq expression in VisualBasic language



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Language.LinqAPI.#ctor
```
2016-10-21
 在这里被设计成Class而不是Module是为了防止和Linq拓展之中的函数产生冲突

#### Exec``1
```csharp
Microsoft.VisualBasic.Language.LinqAPI.Exec``1
```
Execute a linq expression. Creates an array from a @``T:System.Collections.Generic.IEnumerable`1``.

_returns: An array that contains the elements from the input sequence._

#### Exec``2
```csharp
Microsoft.VisualBasic.Language.LinqAPI.Exec``2(System.Collections.Generic.IEnumerable{``0})
```


|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### MakeList``1
```csharp
Microsoft.VisualBasic.Language.LinqAPI.MakeList``1
```
Initializes a new instance of the @``T:Microsoft.VisualBasic.Language.List`1```1 class that
 contains elements copied from the specified collection and has sufficient capacity
 to accommodate the number of elements copied.


