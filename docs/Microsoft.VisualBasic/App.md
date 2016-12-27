# App
_namespace: [Microsoft.VisualBasic](./index.md)_

Provides information about, and means to manipulate, the current environment Application information collection.
 (More easily runtime environment information provider on @``F:System.PlatformID.Unix``/LINUX platform for visualbasic program.)



### Methods

#### __CLI
```csharp
Microsoft.VisualBasic.App.__CLI
```
Makes compatibility with git bash: @``F:Microsoft.VisualBasic.App.gitBash`` = ``C:/Program Files/Git``

#### __completeCLI
```csharp
Microsoft.VisualBasic.App.__completeCLI(System.Int32)
```
自动停止GC当前程序的线程

|Parameter Name|Remarks|
|--------------|-------|
|state|-|


#### __GCThreadInvoke
```csharp
Microsoft.VisualBasic.App.__GCThreadInvoke
```
自动垃圾回收线程

#### __getTEMP
```csharp
Microsoft.VisualBasic.App.__getTEMP
```
由于可能会运行多个使用本模块的进程，单独考哈希来作为表示会产生冲突，所以这里使用应用程序的启动时间戳以及当前的哈希值来生成唯一标示

#### __sysTEMP
```csharp
Microsoft.VisualBasic.App.__sysTEMP
```
@``M:Microsoft.VisualBasic.FileIO.FileSystem.GetParentPath(System.String)``(@``M:Microsoft.VisualBasic.FileIO.FileSystem.GetTempFileName``)
 当临时文件夹被删除掉了的时候，会出现崩溃。。。。所以弃用改用读取环境变量

#### BugsFormatter
```csharp
Microsoft.VisualBasic.App.BugsFormatter(System.Exception,System.String)
```
Generates the formatted error log file content.(生成简单的日志板块的内容)

|Parameter Name|Remarks|
|--------------|-------|
|ex|-|
|Trace|-|


#### CLICode
```csharp
Microsoft.VisualBasic.App.CLICode(System.Boolean,System.Int32)
```
IF the flag is True, that means cli API execute successfully, function returns ZERO, or a negative integer(Default -100) for failures.

|Parameter Name|Remarks|
|--------------|-------|
|b|-|
|Failed|-|


#### ElapsedMilliseconds
```csharp
Microsoft.VisualBasic.App.ElapsedMilliseconds
```
The distance of time that this application running from start and to current time.
 (当前距离应用程序启动所逝去的时间)

#### Exit
```csharp
Microsoft.VisualBasic.App.Exit(System.Int32)
```
Terminates this @``T:System.Diagnostics.Process`` and gives the underlying operating system the specified exit code.
 (这个方法还会终止本应用程序里面的自动GC线程)

|Parameter Name|Remarks|
|--------------|-------|
|state|Exit code to be given to the operating system. Use 0 (zero) to indicate that the process completed successfully.|


#### GenerateTemp
```csharp
Microsoft.VisualBasic.App.GenerateTemp(System.String,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|sysTemp|临时文件路径|


#### GetAppSysTempFile
```csharp
Microsoft.VisualBasic.App.GetAppSysTempFile(System.String,System.String)
```
Get temp file name in app system temp directory.

|Parameter Name|Remarks|
|--------------|-------|
|ext|-|
|sessionID|-|


#### GetAppVariables
```csharp
Microsoft.VisualBasic.App.GetAppVariables
```
获取@``T:Microsoft.VisualBasic.App``的可读属性值来作为环境变量

#### GetProductSharedDIR
```csharp
Microsoft.VisualBasic.App.GetProductSharedDIR(System.Type)
```
使用@``P:Microsoft.VisualBasic.App.ProductSharedDIR``的位置会变化的，则使用本函数则会使用获取当前的模块的文件夹，即使其不是exe程序而是一个dll文件

|Parameter Name|Remarks|
|--------------|-------|
|type|-|


#### GetProductSharedTemp
```csharp
Microsoft.VisualBasic.App.GetProductSharedTemp
```
Gets a temp file name which is located at directory @``P:Microsoft.VisualBasic.App.ProductSharedDIR``.
 (获取位于共享文件夹@``P:Microsoft.VisualBasic.App.ProductSharedDIR``里面的临时文件)

#### GetTempFile
```csharp
Microsoft.VisualBasic.App.GetTempFile
```
Creates a uniquely named zero-byte temporary file on disk and returns the full
 path of that file.

#### GetVariable
```csharp
Microsoft.VisualBasic.App.GetVariable(System.String)
```
这个函数只是会从设置的变量之中查找，本模块之中的变量请直接从属性进行引用

|Parameter Name|Remarks|
|--------------|-------|
|name$|
 因为由于是从命令行之中输入进来的，所以可能有些时候大小写会影响直接字典查找，在这里需要用字符串手工查找
 |


_returns: 当没有查找到相对应的环境变量的时候会返回空值_

#### LogException
```csharp
Microsoft.VisualBasic.App.LogException(System.String,System.String)
```
This is the custom message of the exception, not extract from the function @``M:System.Exception.ToString``

|Parameter Name|Remarks|
|--------------|-------|
|exMsg|This is the custom message of the exception, not extract from the function @``M:System.Exception.ToString``|
|Trace|-|


#### RunAsAdmin
```csharp
Microsoft.VisualBasic.App.RunAsAdmin(System.String)
```
Restart the current process with administrator credentials.(以管理员的身份重启本应用程序)

#### RunCLI
```csharp
Microsoft.VisualBasic.App.RunCLI(System.Type,Microsoft.VisualBasic.CommandLine.CommandLine,Microsoft.VisualBasic.CommandLine.__ExecuteFile,Microsoft.VisualBasic.CommandLine.__ExecuteEmptyCLI,System.String)
```
Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)

|Parameter Name|Remarks|
|--------------|-------|
|args|The command line arguments value, which its value can be gets from the @``P:Microsoft.VisualBasic.App.Command`` function.|


_returns: Returns the function execute result to the operating system._

#### SelfFolk
```csharp
Microsoft.VisualBasic.App.SelfFolk(System.String)
```
Self call this program itself for batch parallel task calculation.
 (调用自身程序，这个通常是应用于批量的数据的计算任务的实现)

|Parameter Name|Remarks|
|--------------|-------|
|CLI|-|


#### SelfFolks
```csharp
Microsoft.VisualBasic.App.SelfFolks(System.Collections.Generic.IEnumerable{System.String},System.Int32,System.Double)
```
Folk this program itself for the large amount data batch processing.

|Parameter Name|Remarks|
|--------------|-------|
|CLI|Self folk processing commandline collection.|
|parallel|If this parameter value less than 1, then will be a single 
 thread task. Any positive value that greater than 1 will be parallel task.
 (小于等于零表示非并行化，单线程任务)
 |
|smart|Smart mode CPU load threshold, if the **`parallel`** 
 parameter value is less than or equals to 1, then this parameter will be disabled.
 |


_returns: 
 Returns the total executation time for running this task collection.
 (返回任务的执行的总时长)
 _

#### Shell
```csharp
Microsoft.VisualBasic.App.Shell(System.String,System.String,System.Boolean)
```
Folk helper for running the other .NET application.
 (请注意，这个函数只能够运行.NET程序, 假若是在Linux系统之上，还需要安装mono运行时环境)

|Parameter Name|Remarks|
|--------------|-------|
|app|External application file full path|
|CLI|Commandline string that running the application **`app$`**|
|CLR|Is the calling external application is a .NET application?
 (是否为.NET程序?)
 |

> @``T:Microsoft.VisualBasic.CommandLine.IORedirectFile``这个建议在进行外部调用的时候才使用

#### StartGC
```csharp
Microsoft.VisualBasic.App.StartGC(System.Boolean)
```
Start the automatic garbage collection threads.
 (这条线程只会自动清理*.tmp临时文件，因为假若不清理临时文件的话，有时候临时文件比较多的时候，会严重影响性能，甚至无法运行应用程序框架里面的IO重定向操作)

#### StopGC
```csharp
Microsoft.VisualBasic.App.StopGC
```
Stop the automatic garbage collection threads.

#### TraceBugs
```csharp
Microsoft.VisualBasic.App.TraceBugs(System.Exception,System.String)
```
Function returns the file path of the application log file.
 (函数返回的是日志文件的文件路径)


### Properties

#### __GCThread
自动垃圾回收线程
#### _preDIR
-
 Linux里面的前一个文件夹
#### _tmpHash
Example: ``tmp2A10.tmp``
#### AppSystemTemp
Application temp data directory in the system temp root: %@``P:Microsoft.VisualBasic.App.SysTemp``%/@``P:Microsoft.VisualBasic.App.AssemblyName``
#### AssemblyName
Gets the name, without the extension, of the assembly file for the application.
#### Command
Returns the argument portion of the @``T:Microsoft.VisualBasic.CommandLine.CommandLine`` used to start Visual Basic or
 an executable program developed with Visual Basic. The My feature provides greater
 productivity and performance than the @``M:Microsoft.VisualBasic.Interaction.Command`` function. For more information,
 see @``P:Microsoft.VisualBasic.ApplicationServices.ConsoleApplicationBase.CommandLineArgs``.
#### CommandLine
Gets the command-line arguments for this @``P:Microsoft.VisualBasic.App.Process``.
#### CPUCoreNumbers
Numbers of the CPU kernels on the current machine.(获取当前的系统主机的CPU核心数)
#### CurrentDirectory
The currrent working directory of this application.(应用程序的当前的工作目录)
#### Desktop
Gets a path name pointing to the Desktop directory.
#### ExceptionLogFile
@``P:Microsoft.VisualBasic.App.LocalData``/error.log
#### ExecutablePath
The file path of the current running program executable file.(本应用程序的可执行文件的文件路径)
#### HOME
The program directory of the current running program.
#### IsMicrosoftPlatform
Is this application running on a Microsoft OS platform.(是否是运行于微软的操作系统平台？)
#### LocalData
The local data dir of the application in the %user%/<CurrentUser>/Local/Product/App
#### LocalDataTemp
The temp directory in the application local data.
#### LogErrDIR
Error default log fie location from function @``M:Microsoft.VisualBasic.App.LogException(System.Exception,System.String)``.(存放自动存储的错误日志的文件夹)
#### NanoTime
Gets the number of ticks that represent the date and time of this instance.
 
 The number of ticks that represent the date and time of this instance. The value
 is between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks.
#### NextTempName
是名称，不是文件路径
#### PID
Get the @``T:System.Diagnostics.Process`` id(PID) of the current program process.
#### Platform
Gets a @``T:System.PlatformID`` enumeration value that identifies the operating system
 platform.
#### Process
Gets a new @``T:System.Diagnostics.Process`` component and associates it with the currently active process.
#### ProductProgramData
The repository root of the product application program data.
#### ProductSharedDIR
The shared program data directory for a group of app which have the same product series name.
 (同一產品程序集所共享的數據文件夾)
#### StartTime
The time tag of the application started.(应用程序的启动的时间)
#### StartupDirectory
Gets the path for the executable file that started the application, not including the executable name.
#### SysTemp
The directory path of the system temp data.
#### UserHOME
Getting the path of the home directory
#### Version
Gets the product version associated with this application.
