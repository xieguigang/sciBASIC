# DebuggerArgs
_namespace: [Microsoft.VisualBasic.Debugging](./index.md)_

调试器设置参数模块



### Methods

#### __logShell
```csharp
Microsoft.VisualBasic.Debugging.DebuggerArgs.__logShell(Microsoft.VisualBasic.CommandLine.CommandLine)
```
Logging command shell history.

|Parameter Name|Remarks|
|--------------|-------|
|args|-|


#### InitDebuggerEnvir
```csharp
Microsoft.VisualBasic.Debugging.DebuggerArgs.InitDebuggerEnvir(Microsoft.VisualBasic.CommandLine.CommandLine,System.String)
```
Initialize the global environment variables in this App process.

|Parameter Name|Remarks|
|--------------|-------|
|args|--echo on/off/all/warn/error --err <path.log>|


#### SaveErrorLog
```csharp
Microsoft.VisualBasic.Debugging.DebuggerArgs.SaveErrorLog(System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|log|日志文本|



### Properties

#### DebuggerHelps
Some optional VisualBasic debugger parameter help information.(VisualBasic调试器的一些额外的开关参数的帮助信息)
#### ErrLogs
错误日志的文件存储位置，默认是在AppData里面
