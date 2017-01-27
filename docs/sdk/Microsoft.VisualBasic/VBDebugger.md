# VBDebugger
_namespace: [Microsoft.VisualBasic](./index.md)_

Debugger helper module for VisualBasic Enterprises System.



### Methods

#### __DEBUG_ECHO
```csharp
Microsoft.VisualBasic.VBDebugger.__DEBUG_ECHO(System.Text.StringBuilder,System.Int32)
```
Output the full debug information while the project is debugging in debug mode.
 (向标准终端和调试终端输出一些带有时间戳的调试信息)

|Parameter Name|Remarks|
|--------------|-------|
|MSG|The message fro output to the debugger console, this function will add a time stamp automaticly To the leading position Of the message.|
|Indent|-|


_returns: 其实这个函数是不会返回任何东西的，只是因为为了Linq调试输出的需要，所以在这里是返回Nothing的_

#### Assertion
```csharp
Microsoft.VisualBasic.VBDebugger.Assertion(System.Boolean,System.String,System.String)
```
If **`test`** is false(means this assertion test failure), then throw exception.

|Parameter Name|Remarks|
|--------------|-------|
|test|-|
|msg|-|


#### Echo
```csharp
Microsoft.VisualBasic.VBDebugger.Echo(System.Char)
```
Alias for @``M:System.Console.Write(System.Boolean)``

|Parameter Name|Remarks|
|--------------|-------|
|c|-|


#### LinqProc``1
```csharp
Microsoft.VisualBasic.VBDebugger.LinqProc``1(System.Collections.Generic.IEnumerable{``0},System.String)
```
当在执行大型的数据集合的时候怀疑linq里面的某一个任务进入了死循环状态，可以使用这个方法来检查是否如此

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|TAG|-|


#### PrintException
```csharp
Microsoft.VisualBasic.VBDebugger.PrintException(System.String,System.String)
```
可以使用这个方法@``M:System.Reflection.MethodBase.GetCurrentMethod``.@``M:Microsoft.VisualBasic.EmitReflection.GetFullName(System.Reflection.MethodBase,System.Boolean)``获取得到**`memberName`**所需要的参数信息

|Parameter Name|Remarks|
|--------------|-------|
|msg|-|
|memberName|-|


#### PrintException``1
```csharp
Microsoft.VisualBasic.VBDebugger.PrintException``1(``0,System.String)
```
The function will print the exception details information on the standard @``T:System.Console``, @``T:System.Diagnostics.Debug`` console, and system @``T:System.Diagnostics.Trace`` console.
 (分别在标准终端，调试终端，系统调试终端之中打印出错误信息，请注意，函数会直接返回False可以用于指定调用者函数的执行状态，这个函数仅仅是在终端上面打印出错误，不会保存为日志文件)

|Parameter Name|Remarks|
|--------------|-------|
|exception|-|


#### this
```csharp
Microsoft.VisualBasic.VBDebugger.this(System.String)
```
Returns the current function name.

|Parameter Name|Remarks|
|--------------|-------|
|caller|
 The caller function name, do not assign any value to this parameter! Just leave it blank.
 |


#### Warning
```csharp
Microsoft.VisualBasic.VBDebugger.Warning(System.String,System.String)
```
Display the wraning level(YELLOW color) message on the console.

|Parameter Name|Remarks|
|--------------|-------|
|msg|-|
|calls|-|



### Properties

#### ForceSTDError
Force the app debugging output redirect into the std_error device.
#### Mute
Disable the debugger information outputs on the console if this @``P:Microsoft.VisualBasic.VBDebugger.Mute`` property is set to True, 
 and enable the output if this property is set to False. 
 NOTE: this debugger option property can be overrides by the debugger parameter from the CLI parameter named '--echo'
