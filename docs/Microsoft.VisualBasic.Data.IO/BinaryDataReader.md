# BinaryDataReader
_namespace: [Microsoft.VisualBasic.Data.IO](./index.md)_

Represents an extended @``T:System.IO.BinaryReader`` supporting special file format data types.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.#ctor(System.IO.Stream,System.Text.Encoding,System.Boolean)
```
Initializes a new instance of the @``T:Microsoft.VisualBasic.Data.IO.BinaryDataReader`` class based on the specified stream and
 character encoding, and optionally leaves the stream open.

|Parameter Name|Remarks|
|--------------|-------|
|input|The input stream.|
|encoding__1|The character encoding to use.|
|leaveOpen|true to leave the stream open after the @``T:Microsoft.VisualBasic.Data.IO.BinaryDataReader`` object
 is disposed; otherwise false.|


#### Align
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.Align(System.Int32)
```
Aligns the reader to the next given byte multiple.

|Parameter Name|Remarks|
|--------------|-------|
|alignment|The byte multiple.|


#### ReadDateTime
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadDateTime(Microsoft.VisualBasic.Data.IO.BinaryDateTimeFormat)
```
Reads a @``T:System.DateTime`` from the current stream. The @``T:System.DateTime`` is available in the
 specified binary format.

|Parameter Name|Remarks|
|--------------|-------|
|format|The binary format, in which the @``T:System.DateTime`` will be read.|


_returns: The @``T:System.DateTime`` read from the current stream._

#### ReadDecimal
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadDecimal
```
Reads an 16-byte floating point value from the current stream and advances the current position of the
 stream by sixteen bytes.

_returns: The 16-byte floating point value read from the current stream._

#### ReadDecimals
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadDecimals(System.Int32)
```
Reads the specified number of @``T:System.Decimal`` values from the current stream into a
 @``T:System.Decimal`` array and advances the current position by that number of @``T:System.Decimal`` values
 multiplied with the size of a single value.

|Parameter Name|Remarks|
|--------------|-------|
|count|The number of @``T:System.Decimal`` values to read.|


_returns: The @``T:System.Decimal`` array containing data read from the current stream. This might be less
 than the number of bytes requested if the end of the stream is reached._

#### ReadDouble
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadDouble
```
Reads an 8-byte floating point value from the current stream and advances the current position of the stream
 by eight bytes.

_returns: The 8-byte floating point value read from the current stream._

#### ReadDoubles
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadDoubles(System.Int32)
```
Reads the specified number of @``T:System.Double`` values from the current stream into a
 @``T:System.Double`` array and advances the current position by that number of @``T:System.Double`` values
 multiplied with the size of a single value.

|Parameter Name|Remarks|
|--------------|-------|
|count|The number of @``T:System.Double`` values to read.|


_returns: The @``T:System.Double`` array containing data read from the current stream. This might be less
 than the number of bytes requested if the end of the stream is reached._

#### ReadInt16
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadInt16
```
Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two
 bytes.

_returns: The 2-byte signed integer read from the current stream._

#### ReadInt16s
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadInt16s(System.Int32)
```
Reads the specified number of @``T:System.Int16`` values from the current stream into a @``T:System.Int16``
 array and advances the current position by that number of @``T:System.Int16`` values multiplied with the
 size of a single value.

|Parameter Name|Remarks|
|--------------|-------|
|count|The number of @``T:System.Int16`` values to read.|


_returns: The @``T:System.Int16`` array containing data read from the current stream. This might be less than
 the number of bytes requested if the end of the stream is reached._

#### ReadInt32
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadInt32
```
Reads a 4-byte signed integer from the current stream and advances the current position of the stream by
 four bytes.

_returns: The 4-byte signed integer read from the current stream._

#### ReadInt32s
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadInt32s(System.Int32)
```
Reads the specified number of @``T:System.Int32`` values from the current stream into a @``T:System.Int32``
 array and advances the current position by that number of @``T:System.Int32`` values multiplied with the
 size of a single value.

|Parameter Name|Remarks|
|--------------|-------|
|count|The number of @``T:System.Int32`` values to read.|


_returns: The @``T:System.Int32`` array containing data read from the current stream. This might be less than
 the number of bytes requested if the end of the stream is reached._

#### ReadInt64
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadInt64
```
Reads an 8-byte signed integer from the current stream and advances the current position of the stream by
 eight bytes.

_returns: The 8-byte signed integer read from the current stream._

#### ReadInt64s
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadInt64s(System.Int32)
```
Reads the specified number of @``T:System.Int64`` values from the current stream into a @``T:System.Int64``
 array and advances the current position by that number of @``T:System.Int64`` values multiplied with the
 size of a single value.

|Parameter Name|Remarks|
|--------------|-------|
|count|The number of @``T:System.Int64`` values to read.|


_returns: The @``T:System.Int64`` array containing data read from the current stream. This might be less than
 the number of bytes requested if the end of the stream is reached._

#### ReadSBytes
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadSBytes(System.Int32)
```
Reads the specified number of @``T:System.SByte`` values from the current stream into a @``T:System.SByte``
 array and advances the current position by that number of @``T:System.SByte`` values multiplied with the
 size of a single value.

|Parameter Name|Remarks|
|--------------|-------|
|count|The number of @``T:System.SByte`` values to read.|


_returns: The @``T:System.SByte`` array containing data read from the current stream. This might be less than
 the number of bytes requested if the end of the stream is reached._

#### ReadSingle
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadSingle
```
Reads a 4-byte floating point value from the current stream and advances the current position of the stream
 by four bytes.

_returns: The 4-byte floating point value read from the current stream._

#### ReadSingles
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadSingles(System.Int32)
```
Reads the specified number of @``T:System.Single`` values from the current stream into a
 @``T:System.Single`` array and advances the current position by that number of @``T:System.Single`` values
 multiplied with the size of a single value.

|Parameter Name|Remarks|
|--------------|-------|
|count|The number of @``T:System.Single`` values to read.|


_returns: The @``T:System.Single`` array containing data read from the current stream. This might be less
 than the number of bytes requested if the end of the stream is reached._

#### ReadString
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadString(System.Int32,System.Text.Encoding)
```
Reads a string from the current stream. The string has neither a prefix or postfix, the length has to be
 specified manually. The string is available in the specified encoding.

|Parameter Name|Remarks|
|--------------|-------|
|length|The length of the string.|
|encoding|The encoding to use for reading the string.|


_returns: The string read from the current stream._

#### ReadUInt16
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadUInt16
```
Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the
 position of the stream by two bytes.

_returns: The 2-byte unsigned integer read from the current stream._

#### ReadUInt16s
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadUInt16s(System.Int32)
```
Reads the specified number of @``T:System.UInt16`` values from the current stream into a
 @``T:System.UInt16`` array and advances the current position by that number of @``T:System.UInt16`` values
 multiplied with the size of a single value.

|Parameter Name|Remarks|
|--------------|-------|
|count|The number of @``T:System.UInt16`` values to read.|


_returns: The @``T:System.UInt16`` array containing data read from the current stream. This might be less
 than the number of bytes requested if the end of the stream is reached._

#### ReadUInt32
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadUInt32
```
Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight
 bytes.

_returns: The 8-byte unsigned integer read from the current stream._

#### ReadUInt32s
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadUInt32s(System.Int32)
```
Reads the specified number of @``T:System.UInt32`` values from the current stream into a
 @``T:System.UInt32`` array and advances the current position by that number of @``T:System.UInt32`` values
 multiplied with the size of a single value.

|Parameter Name|Remarks|
|--------------|-------|
|count|The number of @``T:System.UInt32`` values to read.|


_returns: The @``T:System.UInt32`` array containing data read from the current stream. This might be less
 than the number of bytes requested if the end of the stream is reached._

#### ReadUInt64
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadUInt64
```
Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight
 bytes.

_returns: The 8-byte unsigned integer read from the current stream._

#### ReadUInt64s
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.ReadUInt64s(System.Int32)
```
Reads the specified number of @``T:System.UInt64`` values from the current stream into a
 @``T:System.UInt64`` array and advances the current position by that number of @``T:System.UInt64`` values
 multiplied with the size of a single value.

|Parameter Name|Remarks|
|--------------|-------|
|count|The number of @``T:System.UInt64`` values to read.|


_returns: The @``T:System.UInt64`` array containing data read from the current stream. This might be less
 than the number of bytes requested if the end of the stream is reached._

#### Seek
```csharp
Microsoft.VisualBasic.Data.IO.BinaryDataReader.Seek(System.Int64,System.IO.SeekOrigin)
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
Microsoft.VisualBasic.Data.IO.BinaryDataReader.TemporarySeek(System.Int64,System.IO.SeekOrigin)
```
Creates a @``T:Microsoft.VisualBasic.Data.IO.SeekTask`` with the given parameters. As soon as the returned @``T:Microsoft.VisualBasic.Data.IO.SeekTask``
 is disposed, the previous stream position will be restored.

|Parameter Name|Remarks|
|--------------|-------|
|offset|A byte offset relative to the origin parameter.|
|origin|A value of type @``T:System.IO.SeekOrigin`` indicating the reference point used to obtain
 the new position.|


_returns: The @``T:Microsoft.VisualBasic.Data.IO.SeekTask`` to be disposed to undo the seek._


### Properties

#### ByteOrder
Gets or sets the byte order used to parse binary data with.
#### Encoding
Gets the encoding used for string related operations where no other encoding has been provided. Due to the
 way the underlying @``T:System.IO.BinaryReader`` is instantiated, it can only be specified at creation time.
#### EndOfStream
Gets a value indicating whether the end of the stream has been reached and no more data can be read.
#### Length
Gets the length in bytes of the stream in bytes. This is a shortcut to the base stream Length property.
#### Position
Gets or sets the position within the current stream. This is a shortcut to the base stream Position
 property.
