# Shell
_namespace: [Microsoft.VisualBasic.Terminal.STDIO__](./index.md)_





### Methods

#### AllocConsole
```csharp
Microsoft.VisualBasic.Terminal.STDIO__.Shell.AllocConsole
```
You can create a console window In a Windows Forms project. Project + properties, turn off "Enable application framework" 
 And Set Startup Object To "Sub Main". 
 
 Modify the Application.Run() statement To create the proper startup form, If necessary.

#### Shell
```csharp
Microsoft.VisualBasic.Terminal.STDIO__.Shell.Shell(System.String,System.Diagnostics.ProcessWindowStyle,System.Boolean)
```


|Parameter Name|Remarks|
|--------------|-------|
|CommandLine|-|
|WindowStyle|-|
|WaitForExit|If NOT, then the function returns the associated process id value. Else returns the process exit code.|


#### ShowConsoleWindows
```csharp
Microsoft.VisualBasic.Terminal.STDIO__.Shell.ShowConsoleWindows
```
为WinForm应用程序分配一个终端窗口，这个函数一般是在Debug模式之下进行程序调试所使用的


