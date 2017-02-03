# SeekTask
_namespace: [Microsoft.VisualBasic.Data.IO](./index.md)_

Represents a temporary seek to another position which is undone after the task has been disposed.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.IO.SeekTask.#ctor(System.IO.Stream,System.Int64,System.IO.SeekOrigin)
```
Initializes a new instance of the @``T:Microsoft.VisualBasic.Data.IO.SeekTask`` class to temporarily seek the given
 @``P:Microsoft.VisualBasic.Data.IO.SeekTask.Stream`` to the specified position. The @``T:System.IO.Stream`` is rewound to its
 previous position after the task is disposed.

|Parameter Name|Remarks|
|--------------|-------|
|stream__1|A @``T:System.IO.Stream`` to temporarily seek.|
|offset|A byte offset relative to the origin parameter.|
|origin|A value of type @``T:System.IO.SeekOrigin`` indicating the reference point used to obtain
 the new position.|


#### Dispose
```csharp
Microsoft.VisualBasic.Data.IO.SeekTask.Dispose
```
Rewinds the @``P:Microsoft.VisualBasic.Data.IO.SeekTask.Stream`` to its previous position.


### Properties

#### PreviousPosition
Gets the absolute position to which the @``P:Microsoft.VisualBasic.Data.IO.SeekTask.Stream`` will be rewound after this task is disposed.
#### Stream
Gets the @``P:Microsoft.VisualBasic.Data.IO.SeekTask.Stream`` which is temporarily sought to another position.
