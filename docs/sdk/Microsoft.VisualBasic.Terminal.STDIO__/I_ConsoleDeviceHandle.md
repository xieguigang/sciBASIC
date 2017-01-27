# I_ConsoleDeviceHandle
_namespace: [Microsoft.VisualBasic.Terminal.STDIO__](./index.md)_

Represents the standard input, output, and error streams for console applications.(表示一个输入输出流控制台界面接口)



### Methods

#### Read
```csharp
Microsoft.VisualBasic.Terminal.STDIO__.I_ConsoleDeviceHandle.Read
```
Reads the next character from the standard input stream.(从输入流读取下一个字符)

_returns: The next character from the input stream, or negative one (-1) if there are currently no more characters to be read._

#### ReadLine
```csharp
Microsoft.VisualBasic.Terminal.STDIO__.I_ConsoleDeviceHandle.ReadLine
```
Reads the next line of characters from the standard input stream.(从输入流读取下一行字符)

_returns: The next line of characters from the input stream, or null if no more lines are available._

#### WriteLine
```csharp
Microsoft.VisualBasic.Terminal.STDIO__.I_ConsoleDeviceHandle.WriteLine(System.String,System.String[])
```
Writes the text representation of the specified array of objects, followed by the current line terminator, to the standard output stream using the specified format information.
 (将指定的字符串值（后跟当前行终止符）写入输出流。)

|Parameter Name|Remarks|
|--------------|-------|
|s|-|
|args|-|



