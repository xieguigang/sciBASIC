# Offset
_namespace: [Microsoft.VisualBasic.Data.IO](./index.md)_

Represents a space of 4 bytes reserved in the underlying stream of a @``T:Microsoft.VisualBasic.Data.IO.BinaryDataWriter`` which can
 be comfortably satisfied later on.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.IO.Offset.#ctor(Microsoft.VisualBasic.Data.IO.BinaryDataWriter)
```
Initializes a new instance of the @``T:Microsoft.VisualBasic.Data.IO.Offset`` class reserving an offset with the specified
 @``T:Microsoft.VisualBasic.Data.IO.BinaryDataWriter`` at the current position.

|Parameter Name|Remarks|
|--------------|-------|
|writer__1|The @``T:Microsoft.VisualBasic.Data.IO.BinaryDataWriter`` holding the stream in which the offset will be
 reserved.|


#### Satisfy
```csharp
Microsoft.VisualBasic.Data.IO.Offset.Satisfy
```
Satisfies the offset by writing the current position of the underlying stream at the reserved
 @``P:Microsoft.VisualBasic.Data.IO.Offset.Position``, then seeking back to the current position.


### Properties

#### Position
Gets the address at which the allocation is made.
#### Writer
Gets the @``T:Microsoft.VisualBasic.Data.IO.BinaryDataWriter`` in which underlying stream the allocation is made.
