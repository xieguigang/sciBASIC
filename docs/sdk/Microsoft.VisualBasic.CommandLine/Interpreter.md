# Interpreter
_namespace: [Microsoft.VisualBasic.CommandLine](./index.md)_

Command line interpreter for your **CLI** program.
 (命令行解释器，请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待
 回车退出)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.#ctor(System.Type,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|type|A module or a class which contains some shared method for the command entry.
 (包含有若干使用@``T:Microsoft.VisualBasic.CommandLine.Reflection.ExportAPIAttribute``进行标记的命令行执行入口点的Module或者Class对象类型，
 可以使用 Object.GetType/GetType 关键词操作来获取所需要的类型信息)|


#### __executeEmpty
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.__executeEmpty
```
命令行是空的

#### __getsAllCommands
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.__getsAllCommands(System.Type,System.Boolean)
```
导出所有符合条件的静态方法

|Parameter Name|Remarks|
|--------------|-------|
|Type|-|
|[Throw]|-|


#### __methodInvoke
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.__methodInvoke(System.String,System.Object[],System.String[])
```
The interpreter runs all of the command from here.(所有的命令行都从这里开始执行)

|Parameter Name|Remarks|
|--------------|-------|
|commandName|-|
|argvs|就只有一个命令行对象|
|help_argvs|-|


#### AddCommand
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.AddCommand(Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint)
```
Add a command in current cli interpreter.(x向当前的这个CLI命令行解释器之中添加一个命令)

|Parameter Name|Remarks|
|--------------|-------|
|Command|-|


#### Clear
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.Clear
```
Clear the hash table of the cli command line interpreter command entry points.(清除本CLI解释器之中的所有的命令行执行入口点的哈希数据信息)

#### CreateEmptyCLIObject
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.CreateEmptyCLIObject
```
Create an empty cli command line interpreter object which contains no commands entry.
 (创建一个没有包含有任何命令入口点的空的CLI命令行解释器)

#### CreateInstance
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.CreateInstance(System.String)
```
Create a new interpreter instance from a specific dll/exe path, this program assembly file should be a standard .NET assembly.
 (从一个标准的.NET程序文件之中构建出一个命令行解释器)

|Parameter Name|Remarks|
|--------------|-------|
|assmPath|DLL/EXE file path.(标准的.NET程序集文件的文件路径)|


#### CreateInstance``1
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.CreateInstance``1
```
Create a new interpreter instance using the specific type information.
 (使用所制定的目标类型信息构造出一个CLI命令行解释器)

#### Execute
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.Execute(System.String[])
```
Process the command option arguments of the main function:
 'Public Function Main(argvs As String()) As Integer
 '

|Parameter Name|Remarks|
|--------------|-------|
|CommandLineArgs|The cli command line parameter string value collection.|


#### ExistsCommand
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.ExistsCommand(System.String)
```
The target command line command is exists in this cli interpreter using it name property?(判断目标命令行命令是否存在于本CLI命令行解释器之中)

|Parameter Name|Remarks|
|--------------|-------|
|CommandName|The command name value is not case sensitive.(命令的名称对大小写不敏感的)|


#### GetAllCommands
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.GetAllCommands(System.Type,System.Boolean)
```
导出所有符合条件的静态方法，请注意，在这里已经将外部的属性标记和所属的函数的入口点进行连接了

|Parameter Name|Remarks|
|--------------|-------|
|Type|-|
|[Throw]|-|


#### Help
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.Help(System.String)
```
Gets the help information of a specific command using its name property value.(获取某一个命令的帮助信息)

|Parameter Name|Remarks|
|--------------|-------|
|CommandName|If the paramteer command name value is a empty string then this function
 will list all of the commands' help information.(假若本参数为空则函数会列出所有的命令的帮助信息)|


_returns: Error code, ZERO for no error_

#### ListPossible
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.ListPossible(System.String)
```
列举出所有可能的命令

|Parameter Name|Remarks|
|--------------|-------|
|Name|模糊匹配|


#### SDKdocs
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.SDKdocs
```
Generate the sdk document for the target program assembly.(生成目标应用程序的命令行帮助文档，markdown格式的)

#### ToDictionary
```csharp
Microsoft.VisualBasic.CommandLine.Interpreter.ToDictionary
```
Gets the dictionary data which contains all of the available command information in this assembly module.
 (获取从本模块之中获取得到的所有的命令行信息)


### Properties

#### __API_InfoHash
在添加之前请确保键名是小写的字符串
#### APIList
当前的解释器内所容纳的所有的CLI API列表
#### APINameList
List all of the command line entry point name which were contains in this cli interpreter.
 (列举出本CLI命令行解释器之中的所有的命令行执行入口点的名称)
#### Count
Gets the command counts in current cli interpreter.(返回本CLI命令行解释器之中所包含有的命令的数目)
#### ExecuteEmptyCli
Public Delegate Function __ExecuteEmptyCli() As Integer,
 (@``T:Microsoft.VisualBasic.CommandLine.__ExecuteEmptyCLI``: 假若所传入的命令行是空的，就会执行这个函数指针)
#### ExecuteFile
Public Delegate Function __ExecuteFile(path As String, args As String()) As Integer,
 (@``T:Microsoft.VisualBasic.CommandLine.__ExecuteFile``: 假若所传入的命令行的name是文件路径，解释器就会执行这个函数指针)
 这个函数指针一般是用作于执行脚本程序的
#### Item

#### ListCommandInfo
Returns the command entry info list array.
#### Type
The CLI API container Module/Class type information.(申明这个解释器的命令行API容器类型)
