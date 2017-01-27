# RandomNumbers
_namespace: [Microsoft.VisualBasic.Language.C](./index.md)_

This class provides the ability to simulate the behavior of the C/C++ functions for 
generating random numbers, using the .NET Framework @``T:System.Random`` class.

+ ``rand`` converts to the parameterless overload of NextNumber
+ ``random`` converts to the single-parameter overload of NextNumber
+ ``randomize`` converts to the parameterless overload of Seed
+ ``srand`` converts to the single-parameter overload of Seed



### Methods

#### rand
```csharp
Microsoft.VisualBasic.Language.C.RandomNumbers.rand
```
@``M:System.Random.Next``.(线程安全的函数)

#### random
```csharp
Microsoft.VisualBasic.Language.C.RandomNumbers.random(System.Int32)
```
@``M:System.Random.Next(System.Int32)``.(线程安全的函数)

|Parameter Name|Remarks|
|--------------|-------|
|ceiling|-|



