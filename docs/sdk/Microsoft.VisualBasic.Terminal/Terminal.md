# Terminal
_namespace: [Microsoft.VisualBasic.Terminal](./index.md)_

Represents the standard input, output, and error streams for console applications. 交互式的命令行终端



### Methods

#### Beep
```csharp
Microsoft.VisualBasic.Terminal.Terminal.Beep(System.Int32,System.Int32)
```
Plays the sound of a beep of a specified frequency and duration through the console speaker.

|Parameter Name|Remarks|
|--------------|-------|
|frequency|The frequency of the beep, ranging from 37 to 32767 hertz.|
|duration|The duration of the beep measured in milliseconds.|


#### Clear
```csharp
Microsoft.VisualBasic.Terminal.Terminal.Clear
```
Clears the console buffer and corresponding console window of display information.

#### MoveBufferArea
```csharp
Microsoft.VisualBasic.Terminal.Terminal.MoveBufferArea(System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Char,System.ConsoleColor,System.ConsoleColor)
```
Copies a specified source area of the screen buffer to a specified destination area.

|Parameter Name|Remarks|
|--------------|-------|
|sourceLeft|The leftmost column of the source area.|
|sourceTop|The topmost row of the source area.|
|sourceWidth|The number of columns in the source area.|
|sourceHeight|The number of rows in the source area.|
|targetLeft|The leftmost column of the destination area.|
|targetTop|The topmost row of the destination area.|
|sourceChar|The character used to fill the source area.|
|sourceForeColor|The foreground color used to fill the source area.|
|sourceBackColor|The background color used to fill the source area.|


#### OpenStandardError
```csharp
Microsoft.VisualBasic.Terminal.Terminal.OpenStandardError(System.Int32)
```
Acquires the standard error stream, which is set to a specified buffer size.

|Parameter Name|Remarks|
|--------------|-------|
|bufferSize|The internal stream buffer size.|


_returns: The standard error stream._

#### OpenStandardInput
```csharp
Microsoft.VisualBasic.Terminal.Terminal.OpenStandardInput(System.Int32)
```
Acquires the standard input stream, which is set to a specified buffer size.

|Parameter Name|Remarks|
|--------------|-------|
|bufferSize|The internal stream buffer size.|


_returns: The standard input stream._

#### OpenStandardOutput
```csharp
Microsoft.VisualBasic.Terminal.Terminal.OpenStandardOutput(System.Int32)
```
Acquires the standard output stream, which is set to a specified buffer size.

|Parameter Name|Remarks|
|--------------|-------|
|bufferSize|The internal stream buffer size.|


_returns: The standard output stream._

#### Read
```csharp
Microsoft.VisualBasic.Terminal.Terminal.Read
```
Reads the next character from the standard input stream.

_returns: The next character from the input stream, or negative one (-1) if there are currently no more characters to be read._

#### ReadKey
```csharp
Microsoft.VisualBasic.Terminal.Terminal.ReadKey(System.Boolean)
```
Obtains the next character or function key pressed by the user. The pressed key is optionally displayed in the console window.

|Parameter Name|Remarks|
|--------------|-------|
|intercept|Determines whether to display the pressed key in the console window. true to not display the pressed key; otherwise, false.|


_returns: A System.ConsoleKeyInfo object that describes the System.ConsoleKey constant and Unicode character, if any, that correspond to the pressed console key. The System.ConsoleKeyInfo object also describes, in a bitwise combination of System.ConsoleModifiers values, whether one or more Shift, Alt, or Ctrl modifier keys was pressed simultaneously with the console key._

#### ReadLine
```csharp
Microsoft.VisualBasic.Terminal.Terminal.ReadLine
```
Reads the next line of characters from the standard input stream.

_returns: The next line of characters from the input stream, or null if no more lines are available._

#### ResetColor
```csharp
Microsoft.VisualBasic.Terminal.Terminal.ResetColor
```
Sets the foreground and background console colors to their defaults.

#### SetBufferSize
```csharp
Microsoft.VisualBasic.Terminal.Terminal.SetBufferSize(System.Int32,System.Int32)
```
Sets the height and width of the screen buffer area to the specified values.

|Parameter Name|Remarks|
|--------------|-------|
|width|The width of the buffer area measured in columns.|
|height|The height of the buffer area measured in rows.|


#### SetCursorPosition
```csharp
Microsoft.VisualBasic.Terminal.Terminal.SetCursorPosition(System.Int32,System.Int32)
```
Sets the position of the cursor.

|Parameter Name|Remarks|
|--------------|-------|
|left|The column position of the cursor.|
|top|The row position of the cursor.|


#### SetError
```csharp
Microsoft.VisualBasic.Terminal.Terminal.SetError(System.IO.TextWriter)
```
Sets the System.Console.Error property to the specified System.IO.TextWriter object.

|Parameter Name|Remarks|
|--------------|-------|
|newError|A stream that is the new standard error output.|


#### SetIn
```csharp
Microsoft.VisualBasic.Terminal.Terminal.SetIn(System.IO.TextReader)
```
Sets the System.Console.In property to the specified System.IO.TextReader object.

|Parameter Name|Remarks|
|--------------|-------|
|newIn|A stream that is the new standard input.|


#### SetOut
```csharp
Microsoft.VisualBasic.Terminal.Terminal.SetOut(System.IO.TextWriter)
```
Sets the System.Console.Out property to the specified System.IO.TextWriter object.

|Parameter Name|Remarks|
|--------------|-------|
|newOut|A stream that is the new standard output.|


#### SetWindowPosition
```csharp
Microsoft.VisualBasic.Terminal.Terminal.SetWindowPosition(System.Int32,System.Int32)
```
Sets the position of the console window relative to the screen buffer.

|Parameter Name|Remarks|
|--------------|-------|
|left|The column position of the upper left corner of the console window.|
|top|The row position of the upper left corner of the console window.|


#### SetWindowSize
```csharp
Microsoft.VisualBasic.Terminal.Terminal.SetWindowSize(System.Int32,System.Int32)
```
Sets the height and width of the console window to the specified values.

|Parameter Name|Remarks|
|--------------|-------|
|width|The width of the console window measured in columns.|
|height|The height of the console window measured in rows.|


#### Write
```csharp
Microsoft.VisualBasic.Terminal.Terminal.Write(System.UInt64)
```
Writes the text representation of the specified 64-bit unsigned integer value to the standard output stream.

|Parameter Name|Remarks|
|--------------|-------|
|value|The value to write.|


#### WriteLine
```csharp
Microsoft.VisualBasic.Terminal.Terminal.WriteLine(System.UInt64)
```
Writes the text representation of the specified 64-bit unsigned integer value, followed by the current line terminator, to the standard output stream.

|Parameter Name|Remarks|
|--------------|-------|
|value|The value to write.|



### Properties

#### BackgroundColor
Gets or sets the background color of the console.
#### BufferHeight
Gets or sets the height of the buffer area.
#### BufferWidth
Gets or sets the width of the buffer area.
#### CapsLock
Gets a value indicating whether the CAPS LOCK keyboard toggle is turned on or turned off.
#### CursorLeft
Gets or sets the column position of the cursor within the buffer area.
#### CursorSize
Gets or sets the height of the cursor within a character cell.
#### CursorTop
Gets or sets the row position of the cursor within the buffer area.
#### CursorVisible
Gets or sets a value indicating whether the cursor is visible.
#### Error
Gets the standard error output stream.
#### ForegroundColor
Gets or sets the foreground color of the console.
#### In
Gets the standard input stream.
#### InputEncoding
Gets or sets the encoding the console uses to read input.
#### IsErrorRedirected
Gets a value that indicates whether the error output stream has been redirected from the standard error stream.
#### IsInputRedirected
Gets a value that indicates whether input has been redirected from the standard input stream.
#### IsOutputRedirected
Gets a value that indicates whether output has been redirected from the standard output stream.
#### KeyAvailable
Gets a value indicating whether a key press is available in the input stream.
#### LargestWindowHeight
Gets the largest possible number of console window rows, based on the current font and screen resolution.
#### LargestWindowWidth
Gets the largest possible number of console window columns, based on the current font and screen resolution.
#### NumberLock
Gets a value indicating whether the NUM LOCK keyboard toggle is turned on or turned off.
#### Out
Gets the standard output stream.
#### OutputEncoding
Gets or sets the encoding the console uses to write output.
#### Title
Gets or sets the title to display in the console title bar.
#### TreatControlCAsInput
Gets or sets a value indicating whether the combination of the System.ConsoleModifiers.Control modifier key and System.ConsoleKey.C console key (Ctrl+C) is treated as ordinary input or as an interruption that is handled by the operating system.
#### WindowHeight
Gets or sets the height of the console window area.
#### WindowLeft
Gets or sets the leftmost position of the console window area relative to the screen buffer.
#### WindowTop
Gets or sets the top position of the console window area relative to the screen buffer.
#### WindowWidth
Gets or sets the width of the console window.
