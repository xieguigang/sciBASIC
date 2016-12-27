# UnbufferedStringReader
_namespace: [Microsoft.VisualBasic.Text](./index.md)_

Represents a reader that can read a sequential series of characters.



### Methods

#### Close
```csharp
Microsoft.VisualBasic.Text.UnbufferedStringReader.Close
```
Closes the System.IO.TextReader and releases any system resources associated
 with the TextReader.

#### Peek
```csharp
Microsoft.VisualBasic.Text.UnbufferedStringReader.Peek
```
Reads the next character without changing the state of the reader or the character
 source. Returns the next available character without actually reading it from
 the reader.

#### Read
```csharp
Microsoft.VisualBasic.Text.UnbufferedStringReader.Read(System.Char[],System.Int32,System.Int32)
```
Reads a specified maximum number of characters from the current reader and writes
 the data to a buffer, beginning at the specified index.

|Parameter Name|Remarks|
|--------------|-------|
|buffer|-|
|index|-|
|count|-|


#### ReadLine
```csharp
Microsoft.VisualBasic.Text.UnbufferedStringReader.ReadLine
```
Reads a line of characters from the text reader and returns the data as a string.

#### ReadToEnd
```csharp
Microsoft.VisualBasic.Text.UnbufferedStringReader.ReadToEnd
```
Reads all characters from the current position to the end of the text reader
 and returns them as one string.


### Properties

#### Position
The current read position.
