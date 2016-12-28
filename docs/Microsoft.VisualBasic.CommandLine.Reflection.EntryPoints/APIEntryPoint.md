# APIEntryPoint
_namespace: [Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints](./index.md)_

The entry point data of the commands in the command line which was original loaded 
 from the source meta data in the compiled target.
 (命令行命令的执行入口点)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.#ctor(Microsoft.VisualBasic.CommandLine.Reflection.ExportAPIAttribute,System.Reflection.MethodInfo,System.Boolean)
```
Instance method can be initialize from this constructor.
 (假若目标方法为实例方法，请使用本方法进行初始化)

|Parameter Name|Remarks|
|--------------|-------|
|attribute|-|
|Invoke|-|


#### __directInvoke
```csharp
Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.__directInvoke(System.Object[],System.Object,System.Boolean)
```
记录错误信息的最上层的堆栈

|Parameter Name|Remarks|
|--------------|-------|
|callParameters|-|
|target|-|
|[Throw]|-|


#### DirectInvoke
```csharp
Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.DirectInvoke(System.Object[],System.Boolean)
```
不会自动调整补齐参数

|Parameter Name|Remarks|
|--------------|-------|
|callParameters|-|
|[Throw]|-|


#### EntryPointFullName
```csharp
Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.EntryPointFullName(System.Boolean)
```
The full name path of the target invoked method delegate in the namespace library.

#### HelpInformation
```csharp
Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.HelpInformation(System.Boolean)
```
Returns the help information details for this command line entry object.(获取本命令行执行入口点的详细帮助信息)

#### Invoke
```csharp
Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.Invoke(System.Object[],System.Object,System.Boolean)
```
Invoke this command line and returns the function value.
 (函数会补齐可选参数)

|Parameter Name|Remarks|
|--------------|-------|
|parameters|The function parameter for the target invoked method, the optional value will be filled 
 using the paramter default value if you are not specific the optional paramter value is the element position of 
 this paramter value.|
|target|Target entry pointer of this function method delegate.|
|Throw|If throw then if the exception happened from delegate invocation then the program will throw an 
 exception and terminated, if not then the program will save the exception information into a log file and then 
 returns a failure status.|


#### InvokeCLI
```csharp
Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.InvokeCLI(System.Object[],System.Object,System.Boolean)
```
Invoke this command line but returns the function execute success, Zero for success and -1 for failure.
 (函数会补齐可选参数)

|Parameter Name|Remarks|
|--------------|-------|
|parameters|-|
|target|-|
|Throw|-|



### Properties

#### Arguments
当前的这个命令对象的参数帮助信息列表
#### EntryPoint
The reflection entry point in the assembly for the target method object.
#### IsInstanceMethod
The shared method did not requires of the object instance.(这个方法是否为实例方法)
#### target
If the target invoked @``P:Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.EntryPoint``[method delegate] is a instance method, 
 then this property value should be the target object instance which has the method delegate.
 (假若目标方法不是共享的方法，则必须要使用本对象来进行Invoke的调用)
