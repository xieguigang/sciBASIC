# ProcExtensions
_namespace: [Microsoft.VisualBasic.CommandLine](./index.md)_

How to found the process by CLI



### Methods

#### Call
```csharp
Microsoft.VisualBasic.CommandLine.ProcExtensions.Call(System.String,System.String,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|app|The file path of the application to be called by its parent process.|
|args|CLI arguments|


#### ExecSub
```csharp
Microsoft.VisualBasic.CommandLine.ProcExtensions.ExecSub(System.String,System.String,Microsoft.VisualBasic.CommandLine.ProcExtensions.dReadLine,System.String)
```
执行CMD命令
 Example:excuteCommand("ipconfig", "/all", AddressOf PrintMessage)

|Parameter Name|Remarks|
|--------------|-------|
|app|命令|
|args|参数|
|onReadLine|行信息（委托）|

> https://github.com/lishewen/LSWFramework/blob/master/LSWClassLib/CMD/CMDHelper.vb

#### FindProc
```csharp
Microsoft.VisualBasic.CommandLine.ProcExtensions.FindProc(Microsoft.VisualBasic.CommandLine.IIORedirectAbstract)
```
这个主要是为了@``T:Microsoft.VisualBasic.CommandLine.IORedirectFile``对象进行相关进程的查找而设置的，
 对于@``T:Microsoft.VisualBasic.CommandLine.IORedirect``而言则直接可以从其属性@``P:Microsoft.VisualBasic.CommandLine.IORedirect.ProcessInfo``之中获取相关的进程信息

|Parameter Name|Remarks|
|--------------|-------|
|IO|-|


#### GetProc
```csharp
Microsoft.VisualBasic.CommandLine.ProcExtensions.GetProc(System.String)
```
Get process by command line parameter.(按照命令行参数来获取进程实例)

|Parameter Name|Remarks|
|--------------|-------|
|CLI|-|



