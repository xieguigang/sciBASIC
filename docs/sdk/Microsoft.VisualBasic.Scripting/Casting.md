# Casting
_namespace: [Microsoft.VisualBasic.Scripting](./index.md)_

Methods for convert the @``T:System.String`` to some .NET data types.



### Methods

#### As``1
```csharp
Microsoft.VisualBasic.Scripting.Casting.As``1(System.Object)
```
DirectCast(obj, T)

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### CastChar
```csharp
Microsoft.VisualBasic.Scripting.Casting.CastChar(System.String)
```
字符串是空值会返回空字符

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### CastCommandLine
```csharp
Microsoft.VisualBasic.Scripting.Casting.CastCommandLine(System.String)
```
@``M:Microsoft.VisualBasic.CommandLine.CLITools.TryParse(System.Collections.Generic.IEnumerable{System.String},System.Boolean)``

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### CastImage
```csharp
Microsoft.VisualBasic.Scripting.Casting.CastImage(System.String)
```
@``M:Microsoft.VisualBasic.Imaging.GDIPlusExtensions.LoadImage(System.Byte[])``

|Parameter Name|Remarks|
|--------------|-------|
|path|-|


#### CastInteger
```csharp
Microsoft.VisualBasic.Scripting.Casting.CastInteger(System.String)
```
出错会返回默认是0

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### ParseNumeric
```csharp
Microsoft.VisualBasic.Scripting.Casting.ParseNumeric(System.String)
```
Will processing value NaN automatically and strip for the comma, percentage expression.

|Parameter Name|Remarks|
|--------------|-------|
|s|
 + numeric
 + NaN
 + p%
 + a/b
 |


#### RegexParseDouble
```csharp
Microsoft.VisualBasic.Scripting.Casting.RegexParseDouble(System.String)
```
Parsing a real number from the expression text by using the regex expression @``F:Microsoft.VisualBasic.Scripting.Casting.RegexpFloat``.
 (使用正则表达式解析目标字符串对象之中的一个实数)

|Parameter Name|Remarks|
|--------------|-------|
|s|-|



### Properties

#### RegexpDouble
用于解析出任意实数的正则表达式
