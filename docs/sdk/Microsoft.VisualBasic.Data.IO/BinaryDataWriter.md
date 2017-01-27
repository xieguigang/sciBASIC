# BinaryDataWriter
_namespace: [Microsoft.VisualBasic.Data.IO](./index.md)_

Represents an extended @``T:System.IO.BinaryWriter`` supporting special file format data types.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataWriter.#ctor(System.IO.Stream,System.Text.Encoding,System.Boolean)
```
Initializes a new instance of the @``T:Microsoft.VisualBasic.Data.IO.BinaryDataWriter`` class based on the specified stream and
 character encoding, and optionally leaves the stream open.

|Parameter Name|Remarks|
|--------------|-------|
|output|The output stream.|
|encoding__1|The character encoding to use.|
|leaveOpen|true to leave the stream open after the @``T:Microsoft.VisualBasic.Data.IO.BinaryDataWriter`` object
 is disposed; otherwise false.|


#### Align
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataWriter.Align(System.Int32)
```
Aligns the reader to the next given byte multiple..

|Parameter Name|Remarks|
|--------------|-------|
|alignment|The byte multiple.|


#### ReserveOffset
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataWriter.ReserveOffset
```
Allocates space for an @``T:Microsoft.VisualBasic.Data.IO.Offset`` which can be satisfied later on.

_returns: An @``T:Microsoft.VisualBasic.Data.IO.Offset`` to satisfy later on._

#### Seek
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataWriter.Seek(System.Int64,System.IO.SeekOrigin)
```
Sets the position within the current stream. This is a shortcut to the base stream Seek method.

|Parameter Name|Remarks|
|--------------|-------|
|offset|A byte offset relative to the origin parameter.|
|origin|A value of type @``T:System.IO.SeekOrigin`` indicating the reference point used to obtain
 the new position.|


_returns: The new position within the current stream._

#### TemporarySeek
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataWriter.TemporarySeek(System.Int64,System.IO.SeekOrigin)
```
Creates a @``T:Microsoft.VisualBasic.Data.IO.SeekTask`` with the given parameters. As soon as the returned @``T:Microsoft.VisualBasic.Data.IO.SeekTask``
 is disposed, the previous stream position will be restored.

|Parameter Name|Remarks|
|--------------|-------|
|offset|A byte offset relative to the origin parameter.|
|origin|A value of type @``T:System.IO.SeekOrigin`` indicating the reference point used to obtain
 the new position.|


_returns: A @``T:Microsoft.VisualBasic.Data.IO.SeekTask`` to be disposed to undo the seek._

#### Write
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataWriter.Write(System.UInt64[])
```
Writes the specified number of @``T:System.UInt64`` values into the current stream and advances the current
 position by that number of @``T:System.UInt64`` values multiplied with the size of a single value.

|Parameter Name|Remarks|
|--------------|-------|
|values|The @``T:System.UInt64`` values to write.|



### Properties

#### ByteOrder
Gets or sets the byte order used to parse binary data with.
#### Encoding
Gets the encoding used for string related operations where no other encoding has been provided. Due to the
 way the underlying @``T:System.IO.BinaryWriter`` is instantiated, it can only be specified at creation time.
#### Position
Gets or sets the position within the current stream. This is a shortcut to the base stream Position
 property.
