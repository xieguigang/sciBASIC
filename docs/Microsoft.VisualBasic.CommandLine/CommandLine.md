# CommandLine
_namespace: [Microsoft.VisualBasic.CommandLine](./index.md)_

A command line object that parse from the user input commandline string.
 (从用户所输入的命令行字符串之中解析出来的命令行对象，标准的命令行格式为：
 ==<EXE> <CLI_Name> ["Parameter" "Value"]==)



### Methods

#### Add
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.Add(System.String,System.String)
```
Add a parameter with name and its value.

|Parameter Name|Remarks|
|--------------|-------|
|key|-|
|value|-|


#### Assert
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.Assert(System.String,System.String)
```
Determined that the specific Boolean flag is exists or not? 
 if not then returns **`failure`**, if exists such flag, then returns the **`name`**.

|Parameter Name|Remarks|
|--------------|-------|
|name|Boolean flag name|
|failure|-|


#### CheckMissingRequiredArguments
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.CheckMissingRequiredArguments(System.String[])
```
Gets a list of missing required argument name.

|Parameter Name|Remarks|
|--------------|-------|
|args|-|


#### CheckMissingRequiredParameters
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.CheckMissingRequiredParameters(System.Collections.Generic.IEnumerable{System.String})
```
Checking for the missing required parameter, this function will returns the missing parameter
 in the current cli command line object using a specific parameter name list.
 (检查**`list`**之中的所有参数是否存在，函数会返回不存在的参数名)

|Parameter Name|Remarks|
|--------------|-------|
|list|-|


#### Clear
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.Clear
```
Clear the inner list buffer

#### Contains
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.Contains(Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{System.String})
```
只是通过比较名称来判断是否存在，值没有进行比较

|Parameter Name|Remarks|
|--------------|-------|
|item|-|


#### ContainsParameter
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.ContainsParameter(System.String,System.Boolean)
```
Does the specific argument exists in this commandline? argument name is not case sensitity.
 (参数名称字符串大小写不敏感)

|Parameter Name|Remarks|
|--------------|-------|
|parameterName|-|


#### GetBoolean
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetBoolean(System.String)
```
Gets the value Of the specified column As a Boolean.
 (这个函数也同时包含有开关参数的，开关参数默认为逻辑值类型，当包含有开关参数的时候，其逻辑值为True，反之函数会检查参数列表，参数不存在则为空值字符串，则也为False)

|Parameter Name|Remarks|
|--------------|-------|
|parameter|可以包含有开关参数|


#### GetByte
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetByte(System.String)
```
Gets the 8-bit unsigned Integer value Of the specified column.

|Parameter Name|Remarks|
|--------------|-------|
|parameter|-|


#### GetBytes
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetBytes(System.String)
```
Reads a stream Of bytes from the specified column offset into the buffer As an array, starting at the given buffer offset.

#### GetChar
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetChar(System.String)
```
Gets the character value Of the specified column.

#### GetChars
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetChars(System.String)
```
Reads a stream Of characters from the specified column offset into the buffer As an array, starting at the given buffer offset.

#### GetCommandsOverview
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetCommandsOverview
```
Gets the brief summary information of current cli command line object.
 (获取当前的命令行对象的参数摘要信息)

#### GetDateTime
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetDateTime(System.String)
```
Gets the Date And time data value Of the specified field.

#### GetDecimal
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetDecimal(System.String)
```
Gets the fixed-position numeric value Of the specified field.

#### GetDictionary
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetDictionary(System.String)
```
If the target parameter is not presents in the CLI, then this function will returns nothing.
 (键值对之间使用分号分隔)

|Parameter Name|Remarks|
|--------------|-------|
|name$|-|


#### GetDouble
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetDouble(System.String)
```
Gets the Double-precision floating point number Of the specified field.

#### GetEnumerator
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetEnumerator
```
这个枚举函数也会将开关给包含进来，与@``M:Microsoft.VisualBasic.CommandLine.CommandLine.GetValueArray``方法所不同的是，这个函数里面的逻辑值开关的名称没有被修饰剪裁

#### GetFloat
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetFloat(System.String)
```
Gets the Single-precision floating point number Of the specified field.

#### GetFullDIRPath
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetFullDIRPath(System.String)
```
Get specific argument value as full directory path.

|Parameter Name|Remarks|
|--------------|-------|
|name|parameter name|


#### GetFullFilePath
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetFullFilePath(System.String)
```
Get specific argument value as full file path.(这个函数还会同时修正file://协议的头部)

|Parameter Name|Remarks|
|--------------|-------|
|name|parameter name|


#### GetGuid
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetGuid(System.String)
```
Returns the GUID value Of the specified field.

#### GetInt16
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetInt16(System.String)
```
Gets the 16-bit signed Integer value Of the specified field.

#### GetInt32
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetInt32(System.String)
```
Gets the 32-bit signed Integer value Of the specified field.

#### GetInt64
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetInt64(System.String)
```
Gets the 64-bit signed Integer value Of the specified field.

#### GetObject``1
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetObject``1(System.String,System.Func{System.String,``0})
```


|Parameter Name|Remarks|
|--------------|-------|
|parameter|Command parameter name in the command line inputs.|
|__getObject|-|


#### GetOrdinal
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetOrdinal(System.String)
```
Return the index Of the named field. If the name is not exists in the parameter list, then a -1 value will be return.

#### GetString
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetString(System.String)
```
Gets the String value Of the specified field.

#### GetValue``1
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetValue``1(System.String,``0,System.Func{System.String,``0})
```
If the given parameter is not exists in the user input arguments, then a developer specific default value will be return.

|Parameter Name|Remarks|
|--------------|-------|
|name|-|
|[default]|The default value for returns when the parameter is not exists in the user input.|


#### GetValueArray
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.GetValueArray
```
ToArray拓展好像是有BUG的，所以请使用这个函数来获取所有的参数信息，请注意，逻辑值开关的名称会被去掉前缀

#### HavebFlag
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.HavebFlag(System.String)
```
See if the target logical flag argument is exists in the commandline?
 (查看命令行之中是否存在某一个逻辑开关)

|Parameter Name|Remarks|
|--------------|-------|
|name|-|


#### IsNull
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.IsNull(System.String)
```
Return whether the specified field Is Set To null.

#### op_Addition
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.op_Addition(Microsoft.VisualBasic.CommandLine.CommandLine,System.String)
```
Open a handle for a file system object.

|Parameter Name|Remarks|
|--------------|-------|
|args|-|
|fs|-|


#### op_Implicit
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.op_Implicit(System.String)~Microsoft.VisualBasic.CommandLine.CommandLine
```
Parsing the commandline string as object model

|Parameter Name|Remarks|
|--------------|-------|
|CommandLine|-|


#### op_LessThanOrEqual
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.op_LessThanOrEqual(Microsoft.VisualBasic.CommandLine.CommandLine,System.String)
```
Gets the CLI parameter value.

|Parameter Name|Remarks|
|--------------|-------|
|args|-|
|name|-|


#### op_Subtraction
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.op_Subtraction(Microsoft.VisualBasic.CommandLine.CommandLine,Microsoft.VisualBasic.CommandLine.CommandLine)
```
Try get parameter value.

|Parameter Name|Remarks|
|--------------|-------|
|args|-|


#### OpenStreamInput
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.OpenStreamInput(System.String,System.String@)
```
About **`s`**:
 
 + If the file path is not a value path, then is the value is not null, the argument value will be returned from this parameter. 
 + If the value is nothing, then this function will open the standard input as input.
 + If the file path is valid as input file, then a local file system pointer will be returned.
 
 [管道函数] 假若参数名存在并且所指向的文件也存在，则返回本地文件的文件指针，否则返回标准输入的指针

|Parameter Name|Remarks|
|--------------|-------|
|param|-|
|s|
 + If the file path is not a value path, then is the value is not null, the argument value will be returned from this parameter. 
 + If the value is nothing, then this function will open the standard input as input.
 + If the file path is valid as input file, then a local file system pointer will be returned.
 |


#### OpenStreamOutput
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.OpenStreamOutput(System.String)
```
[管道函数] 假若参数名存在，则返回本地文件的文件指针，否则返回标准输出的指针

|Parameter Name|Remarks|
|--------------|-------|
|param|-|


#### ReadInput
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.ReadInput(System.String)
```
Read all of the text input from the file or ``std_in``

|Parameter Name|Remarks|
|--------------|-------|
|param|-|


#### Remove
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.Remove(Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{System.String})
```
Removes a parameter by @``P:Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue`1.Name``

|Parameter Name|Remarks|
|--------------|-------|
|item|-|


#### ToString
```csharp
Microsoft.VisualBasic.CommandLine.CommandLine.ToString
```
Returns the original cli command line argument string.(返回所传入的命令行的原始字符串)


### Properties

#### _CLICommandArgvs
原始的命令行字符串
#### BoolFlags
对于参数而言，都是--或者-或者/或者\开头的，下一个单词为单引号或者非上面的字符开头的，例如/o <path>
 对于开关而言，与参数相同的其实符号，但是后面不跟参数而是其他的开关，通常开关用来进行简要表述一个逻辑值
#### CLICommandArgvs
Get the original command line string.(获取所输入的命令行对象的原始的字符串)
#### Count
Get the switch counts in this commandline object.(获取本命令行对象中的所定义的开关的数目)
#### IsNothing
@``T:System.String`` of @``P:Microsoft.VisualBasic.CommandLine.CommandLine.Name`` AndAlso @``P:Microsoft.VisualBasic.CommandLine.CommandLine.IsNullOrEmpty``
#### IsNullOrEmpty
Does this cli command line object contains any parameter argument information.
 (查看本命令行参数对象之中是否存在有参数信息)
#### Item
The parameter name is not case sensitive.(开关的名称是不区分大小写的)
#### Name
The command name that parse from the input command line.
 (从输入的命令行中所解析出来的命令的名称)
#### ParameterList
Listing all of the parameter value collection that parsed from the commandline string.
#### Parameters
The parameters in the commandline without the first token of the command name.
 (将命令行解析为词元之后去掉命令的名称之后所剩下的所有的字符串列表)
#### Tokens
The command tokens that were parsed from the input commandline.
 (从所输入的命令行之中所解析出来的命令参数单元)
