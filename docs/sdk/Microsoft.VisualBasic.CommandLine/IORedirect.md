# IORedirect
_namespace: [Microsoft.VisualBasic.CommandLine](./index.md)_

A communication fundation class type for the commandline program interop.
 (一个简单的用于从当前进程派生子进程的Wrapper对象，假若需要folk出来的子进程对象
 不需要终端交互功能，则更加推荐使用@``T:Microsoft.VisualBasic.CommandLine.IORedirectFile``对象来进行调用)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.CommandLine.IORedirect.#ctor(System.String,System.String,System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.String}},System.Boolean,System.Boolean,System.Boolean)
```
Creates a @``T:System.Diagnostics.Process`` wrapper for the CLI program operations.
 (在服务器上面可能会有一些线程方面的兼容性BUG的问题，不太清楚为什么会导致这样)

|Parameter Name|Remarks|
|--------------|-------|
|Exe|The file path of the executable file.|
|args|The CLI arguments for the folked program.|
|envir|Set up the environment variable for the target invoked child process.|
|_disp_debug|-|
|disp_STDOUT|是否显示目标被调用的外部程序的标准输出|


#### __detectProcessExit
```csharp
Microsoft.VisualBasic.CommandLine.IORedirect.__detectProcessExit
```
检测目标子进程是否已经结束

#### __listenSTDOUT
```csharp
Microsoft.VisualBasic.CommandLine.IORedirect.__listenSTDOUT
```
输出目标子进程的标准输出设备的内容

#### GetError
```csharp
Microsoft.VisualBasic.CommandLine.IORedirect.GetError
```
Gets a @``T:System.String`` used to read the error output of the application.

_returns: A @``T:System.String`` text value that read from the std_error of @``T:System.IO.StreamReader`` 
 that can be used to read the standard error stream of the application._

#### op_Implicit
```csharp
Microsoft.VisualBasic.CommandLine.IORedirect.op_Implicit(System.String)~Microsoft.VisualBasic.CommandLine.IORedirect
```
在进行隐士转换的时候，假若可执行文件的文件路径之中含有空格，则这个时候应该要特别的小心

|Parameter Name|Remarks|
|--------------|-------|
|CLI|-|


#### Run
```csharp
Microsoft.VisualBasic.CommandLine.IORedirect.Run
```
线程会被阻塞在这里，直到外部应用程序执行完毕

#### Start
```csharp
Microsoft.VisualBasic.CommandLine.IORedirect.Start(System.Boolean)
```
Gets the value that the associated process specified when it terminated.

|Parameter Name|Remarks|
|--------------|-------|
|WaitForExit|-|


_returns: The code that the associated process specified when it terminated._


### Properties

#### _processStateRunning
当前的这个进程实例是否处于运行的状态
#### ProcessInfo
The process invoke interface of current IO redirect operation.
#### StandardOutput
Gets the standard output for the target invoke process.
