# List`1
_namespace: [Microsoft.VisualBasic.Language](./index.md)_

Represents a strongly typed list of objects that can be accessed by index. Provides
 methods to search, sort, and manipulate lists.To browse the .NET Framework source
 code for this type, see the Reference Source.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Language.List`1.#ctor(System.Int32)
```
Initializes a new instance of the List`1 class that
 is empty and has the specified initial capacity.

|Parameter Name|Remarks|
|--------------|-------|
|capacity|The number of elements that the new list can initially store.|


#### op_Addition
```csharp
Microsoft.VisualBasic.Language.List`1.op_Addition(System.Collections.Generic.IEnumerable{`0},Microsoft.VisualBasic.Language.List{`0})
```
Adds the elements of the specified collection to the end of the System.Collections.Generic.List`1.

|Parameter Name|Remarks|
|--------------|-------|
|vals|-|
|list|-|


#### op_Exponent
```csharp
Microsoft.VisualBasic.Language.List`1.op_Exponent(Microsoft.VisualBasic.Language.List{`0},System.Func{`0,System.Boolean})
```
Find a item in the list

|Parameter Name|Remarks|
|--------------|-------|
|list|-|
|find|-|


#### op_GreaterThan
```csharp
Microsoft.VisualBasic.Language.List`1.op_GreaterThan(Microsoft.VisualBasic.Language.List{`0},System.String)
```
Dump this collection data to the file system.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|path|-|


#### op_Subtraction
```csharp
Microsoft.VisualBasic.Language.List`1.op_Subtraction(Microsoft.VisualBasic.Language.List{`0},System.Int32)
```
@``M:System.Collections.Generic.List`1.RemoveAt(System.Int32)``

|Parameter Name|Remarks|
|--------------|-------|
|list|-|
|index|-|


#### op_UnaryPlus
```csharp
Microsoft.VisualBasic.Language.List`1.op_UnaryPlus(Microsoft.VisualBasic.Language.List{`0})
```
Move Next

|Parameter Name|Remarks|
|--------------|-------|
|list|-|


#### PopAll
```csharp
Microsoft.VisualBasic.Language.List`1.PopAll
```
Pop all of the elements value in to array from the list object and then clear all of the list data.


