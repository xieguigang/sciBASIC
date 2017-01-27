# CLITools
_namespace: [Microsoft.VisualBasic.CommandLine](./index.md)_

CLI parser and @``T:Microsoft.VisualBasic.CommandLine.CommandLine`` object creates.



### Methods

#### Args
```csharp
Microsoft.VisualBasic.CommandLine.CLITools.Args
```
Gets the commandline object for the current program.

#### CreateObject
```csharp
Microsoft.VisualBasic.CommandLine.CLITools.CreateObject(System.String,System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.String}},System.Collections.Generic.IEnumerable{System.String})
```
Creates command line object from a set obj @``T:System.Collections.Generic.KeyValuePair`2``

|Parameter Name|Remarks|
|--------------|-------|
|Name|-|
|args|-|
|bFlags|-|


#### CreateParameterValues
```csharp
Microsoft.VisualBasic.CommandLine.CLITools.CreateParameterValues(System.String[],System.Boolean)
```
Parsing parameters from a specific tokens.
 (从给定的词组之中解析出参数的结构)

|Parameter Name|Remarks|
|--------------|-------|
|Tokens|个数为偶数的，但是假若含有开关的时候，则可能为奇数了|
|IncludeLogicSW|返回来的列表之中是否包含有逻辑开关|


#### Equals
```csharp
Microsoft.VisualBasic.CommandLine.CLITools.Equals(Microsoft.VisualBasic.CommandLine.CommandLine,Microsoft.VisualBasic.CommandLine.CommandLine)
```
请注意，这个是有方向性的，由于是依照参数1来进行比较的，假若args2里面的参数要多于第一个参数，但是第一个参数里面的所有参数值都可以被参数2完全比对得上的话，就认为二者是相等的

|Parameter Name|Remarks|
|--------------|-------|
|args1|-|
|args2|-|


#### GetLogicSWs
```csharp
Microsoft.VisualBasic.CommandLine.CLITools.GetLogicSWs(System.String[],System.String@)
```
Get all of the logical parameters from the input tokens

|Parameter Name|Remarks|
|--------------|-------|
|Tokens|要求第一个对象不能够是命令的名称|


#### GetTokens
```csharp
Microsoft.VisualBasic.CommandLine.CLITools.GetTokens(System.String)
```
Try parse the argument tokens which comes from the user input commandline string. 
 (尝试从用户输入的命令行字符串之中解析出所有的参数)

|Parameter Name|Remarks|
|--------------|-------|
|CLI|-|


#### IsNumeric
```csharp
Microsoft.VisualBasic.CommandLine.CLITools.IsNumeric(System.String)
```
Is this token value string is a number?

|Parameter Name|Remarks|
|--------------|-------|
|str|-|


#### IsPossibleLogicFlag
```csharp
Microsoft.VisualBasic.CommandLine.CLITools.IsPossibleLogicFlag(System.String)
```
Is this string tokens is a possible @``T:System.Boolean`` value flag

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### Join
```csharp
Microsoft.VisualBasic.CommandLine.CLITools.Join(System.Collections.Generic.IEnumerable{System.String})
```
ReGenerate the cli command line argument string text.(重新生成命令行字符串)

|Parameter Name|Remarks|
|--------------|-------|
|tokens|If the token value have a space character, then this function will be wrap that token with quot character automatically.|


#### TrimParamPrefix
```csharp
Microsoft.VisualBasic.CommandLine.CLITools.TrimParamPrefix(System.String)
```
修剪命令行参数名称的前置符号

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### TryParse
```csharp
Microsoft.VisualBasic.CommandLine.CLITools.TryParse(System.String,System.String,System.Char)
```
尝试从输入的语句之中解析出词法单元，注意，这个函数不是处理从操作系统所传递进入的命令行语句

|Parameter Name|Remarks|
|--------------|-------|
|CommandLine|-|



### Properties

#### SPLIT_REGX_EXPRESSION
A regex expression string that use for split the commandline text.
 (用于分析命令行字符串的正则表达式)
#### TokenSplitRegex
会对%进行替换的
