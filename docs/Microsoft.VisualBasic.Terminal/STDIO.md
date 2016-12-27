# STDIO
_namespace: [Microsoft.VisualBasic.Terminal](./index.md)_

A standard input/output compatibility package that makes VisualBasic console
 program easily running on the Linux server or mac osx operating system.
 (一个用于让VisualBasic应用程序更加容易的运行于Linux服务器或者MAC系统之上的标准输入输出流的系统兼容包)



### Methods

#### __testEquals
```csharp
Microsoft.VisualBasic.Terminal.STDIO.__testEquals(System.String,System.Char)
```


|Parameter Name|Remarks|
|--------------|-------|
|input|-|
|compare|大写的|


#### cat
```csharp
Microsoft.VisualBasic.Terminal.STDIO.cat(System.String[])
```
不换行

|Parameter Name|Remarks|
|--------------|-------|
|out|-|


#### MsgBox
```csharp
Microsoft.VisualBasic.Terminal.STDIO.MsgBox(System.String,Microsoft.VisualBasic.MsgBoxStyle)
```


|Parameter Name|Remarks|
|--------------|-------|
|prompt|-|
|style|
 Value just allow:
 @``F:Microsoft.VisualBasic.MsgBoxStyle.AbortRetryIgnore``,
 @``F:Microsoft.VisualBasic.MsgBoxStyle.OkCancel``,
 @``F:Microsoft.VisualBasic.MsgBoxStyle.OkOnly``,
 @``F:Microsoft.VisualBasic.MsgBoxStyle.RetryCancel``,
 @``F:Microsoft.VisualBasic.MsgBoxStyle.YesNo``,
 @``F:Microsoft.VisualBasic.MsgBoxStyle.YesNoCancel``|


#### printf
```csharp
Microsoft.VisualBasic.Terminal.STDIO.printf(System.String,System.Object[])
```
Output the string to the console using a specific formation.(按照指定的格式将字符串输出到终端窗口之上，请注意，这个函数除了将数据流输出到标准终端之外，还会输出到调试终端)

|Parameter Name|Remarks|
|--------------|-------|
|s|A string to print on the console window.(输出到终端窗口之上的字符串)|
|args|Formation parameters.(格式化参数)|


#### Read``1
```csharp
Microsoft.VisualBasic.Terminal.STDIO.Read``1(System.String,Microsoft.VisualBasic.Terminal.STDIO.TryParseDelegate{``0},``0)
```
Read Method with Generics & Delegate
 
 In a console application there is often the need to ask (and validate) some data from users. 
 For this reason I have created a function that make use of generics and delegates to 
 speed up programming.
 
 > http://www.codeproject.com/Tips/1108772/Read-Method-with-Generics-Delegate

|Parameter Name|Remarks|
|--------------|-------|
|msg|-|
|parser|-|
|_default|-|


#### scanf
```csharp
Microsoft.VisualBasic.Terminal.STDIO.scanf(System.String@,System.ConsoleColor)
```
Read the string that user input on the console to the function paramenter.
 (将用户在终端窗口之上输入的数据赋值给一个字符串变量)

|Parameter Name|Remarks|
|--------------|-------|
|s|-|


#### Write
```csharp
Microsoft.VisualBasic.Terminal.STDIO.Write(System.Object)
```
Writes the text representation of the specified object to the standard output
 stream.

|Parameter Name|Remarks|
|--------------|-------|
|o|The value to write, or null.|


#### WriteLine
```csharp
Microsoft.VisualBasic.Terminal.STDIO.WriteLine(System.Object)
```
Writes the text representation of the specified object, followed by the current
 line terminator, to the standard output stream.

|Parameter Name|Remarks|
|--------------|-------|
|o|The value to write.|


#### ZeroFill
```csharp
Microsoft.VisualBasic.Terminal.STDIO.ZeroFill(System.String,System.Int32)
```
Fill the number string with specific length of ZERO sequence to generates the fixed width string.

|Parameter Name|Remarks|
|--------------|-------|
|sn|-|
|len|-|



### Properties

#### Eschs
A dictionary list of the escape characters.(转义字符列表)
