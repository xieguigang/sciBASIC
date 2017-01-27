# WindowsServices
_namespace: [Microsoft.VisualBasic.Win32](./index.md)_

Windows event logger services & installer services.
 (这个模块主要的功能是日志服务，包括在安装阶段对日志记录的创建以及自定义url协议的创建等，
 请注意，这个模块之中的大部分的功能都需要你的应用程序是在管理员权限之下运行的)



### Methods

#### Initialize
```csharp
Microsoft.VisualBasic.Win32.WindowsServices.Initialize(System.String,System.String)
```
You should execute the log category entry creates job under the administrators privileges!

|Parameter Name|Remarks|
|--------------|-------|
|ServicesName|-|
|Product|This value is usually the property value of @``P:Microsoft.VisualBasic.App.AssemblyName``|


#### LogsInstaller
```csharp
Microsoft.VisualBasic.Win32.WindowsServices.LogsInstaller(System.String,System.String[])
```
You should execute the log category entry creates job under the administrators privileges!

|Parameter Name|Remarks|
|--------------|-------|
|ServicesName|-|
|Products|This value is usually the property value of @``P:Microsoft.VisualBasic.App.AssemblyName``|


#### RegisterURLProtocol
```csharp
Microsoft.VisualBasic.Win32.WindowsServices.RegisterURLProtocol(System.String,System.String,System.String)
```
(**** Please notice, that the application has To have admin privileges To be able To write the needed stuff into registry. ****)
 
 Everyone knows HTTP-URLs. Windows Shell also enables to define own ``URL protocols``. 
 Some programs (like Visual Studio Help ``ms-help://`` ... or Steam ``steam://`` ...) take advantage of this feature. 
 By creating some registry entries one is able to set up a self-made URL protocol. 
 This allows to access your applications by URL (originating from every software).
 
 Please notice, that **the application has To have admin privileges To be able To write the needed stuff into registry**. 
 You can test your application very easy by opening Windows Explorer And typing ``yoururlprotocol://testdata`` 
 into the path/address field.
 
 Registers an user defined URL protocol for the usage with the Windows Shell, the Internet Explorer and Office.
 
 Example for an URL of an user defined URL protocol:
 
 ```
 rainbird://RemoteControl/OpenFridge/GetBeer
 ```

|Parameter Name|Remarks|
|--------------|-------|
|protocolName|
 Name of the protocol (e.g. "rainbird" for "rainbird://...")
 |
|applicationPath|
 Complete file system path to the EXE file, which processes the URL being called (the complete URL is handed over as a Command Line Parameter).
 |
|description|
 Description (e.g. "URL:Rainbird Custom URL")
 |



### Properties

#### Initialized
Does component @``P:Microsoft.VisualBasic.Win32.WindowsServices.ServicesLogs`` have been initialized?
#### ServicesLogs
Windows system logging services interface, you can viewing the application log events from Event Viewer:
 Explorer >> Manage >> Event Viewer >> Applications and Services Logs >> <Your_Product>
