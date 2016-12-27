# xConsole
_namespace: [Microsoft.VisualBasic.Terminal](./index.md)_

Allows you to color and animate the console. ~ overpowered.it ~ TheTrigger - 💸

> http://www.codeproject.com/Tips/626856/xConsole-Project


### Methods

#### ClearInput
```csharp
Microsoft.VisualBasic.Terminal.xConsole.ClearInput
```
Clear the user input in stack

#### ClosestConsoleColor
```csharp
Microsoft.VisualBasic.Terminal.xConsole.ClosestConsoleColor(System.Byte,System.Byte,System.Byte)
```
Convert rgb color to ConsoleColor. From stackoverflow

#### ConvertHexStringToByteArray
```csharp
Microsoft.VisualBasic.Terminal.xConsole.ConvertHexStringToByteArray(System.String)
```
Convert String to byte array

|Parameter Name|Remarks|
|--------------|-------|
|hexString|-|


#### CoolWrite
```csharp
Microsoft.VisualBasic.Terminal.xConsole.CoolWrite(System.String,System.Object[])
```
Gradual output animation 👍👍

|Parameter Name|Remarks|
|--------------|-------|
|format|The input string|
|args|-|


#### CoolWriteLine
```csharp
Microsoft.VisualBasic.Terminal.xConsole.CoolWriteLine(System.String,System.Object[])
```
Gradual output animation

|Parameter Name|Remarks|
|--------------|-------|
|format|The input string|
|args|Arguments|


#### Credits
```csharp
Microsoft.VisualBasic.Terminal.xConsole.Credits
```
Show credits

#### getColor
```csharp
Microsoft.VisualBasic.Terminal.xConsole.getColor(System.String,System.Nullable{System.ConsoleColor},System.Nullable{System.ConsoleColor})
```
Convert input to color

|Parameter Name|Remarks|
|--------------|-------|
|s|Input string|


_returns: 👽👽👽👾_

#### Implode
```csharp
Microsoft.VisualBasic.Terminal.xConsole.Implode(Microsoft.VisualBasic.Language.List{System.String},System.Int32)
```
(php-like) Implode a List of strings

|Parameter Name|Remarks|
|--------------|-------|
|args|The list input|
|start|Index offset|


_returns: Imploded string_

#### ListFonts
```csharp
Microsoft.VisualBasic.Terminal.xConsole.ListFonts
```
Show list of fonts

#### ParseLine
```csharp
Microsoft.VisualBasic.Terminal.xConsole.ParseLine(System.String)
```
Parse the input string

|Parameter Name|Remarks|
|--------------|-------|
|s|Input string|


#### Print
```csharp
Microsoft.VisualBasic.Terminal.xConsole.Print(System.String)
```
The Parser

|Parameter Name|Remarks|
|--------------|-------|
|input|Input string|


#### ReadKeys
```csharp
Microsoft.VisualBasic.Terminal.xConsole.ReadKeys(System.Boolean)
```
Read EACH keys from the buffer input (visible and hidden chars)

|Parameter Name|Remarks|
|--------------|-------|
|ClearInput|Clear the buffer input|


_returns: string with all chars_

#### ReadLine
```csharp
Microsoft.VisualBasic.Terminal.xConsole.ReadLine(System.Boolean)
```
Read the line, then parse it.

|Parameter Name|Remarks|
|--------------|-------|
|ClearInput|Clear the buffer input|


_returns: Return a List of strings_

#### RestoreColors
```csharp
Microsoft.VisualBasic.Terminal.xConsole.RestoreColors
```
Restore default colors

#### RetrieveLinkerTimestamp
```csharp
Microsoft.VisualBasic.Terminal.xConsole.RetrieveLinkerTimestamp
```
Linker Timestamp

#### SetFont
```csharp
Microsoft.VisualBasic.Terminal.xConsole.SetFont(System.UInt32)
```
Change console font

|Parameter Name|Remarks|
|--------------|-------|
|i|-|


#### SetWindowPos
```csharp
Microsoft.VisualBasic.Terminal.xConsole.SetWindowPos(System.Int32,System.Int32)
```
Set new window position

#### Wait
```csharp
Microsoft.VisualBasic.Terminal.xConsole.Wait(System.Int32)
```
Just wait. in milliseconds

|Parameter Name|Remarks|
|--------------|-------|
|time|-|


#### Write
```csharp
Microsoft.VisualBasic.Terminal.xConsole.Write(System.String,System.Object[])
```
Allows you to write in the console-output with custom colors

|Parameter Name|Remarks|
|--------------|-------|
|format|The input string|
|args|Arguments|


#### WriteLine
```csharp
Microsoft.VisualBasic.Terminal.xConsole.WriteLine(System.String,System.Object[])
```
Allows you to write in the console-output with custom colors, followed by the current line terminator

|Parameter Name|Remarks|
|--------------|-------|
|format|The input string|
|args|Arguments|



### Properties

#### BACKGROUND_COLOR
This value is used when restoring the colors of the console.
#### CheckForUpdatesEnabled
Check for updates every 7days. False to disable. (Default = true);
#### ClearColorsAtEnd
Clear colors automatically at the end of each Writeline. (Default = false);
#### FONT_COLOR
This value is used when restoring the colors of the console.
#### MyASM
My ASM FILE
#### NEW_LINE
Default line terminator
#### RDN
Random number Generator
