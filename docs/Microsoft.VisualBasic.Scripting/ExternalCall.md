# ExternalCall
_namespace: [Microsoft.VisualBasic.Scripting](./index.md)_

Shell object for the external script running.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Scripting.ExternalCall.#ctor(System.String,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|host|The program its file name to run the script|
|ext|File extension name of this type of script|


#### Run
```csharp
Microsoft.VisualBasic.Scripting.ExternalCall.Run(System.String,System.Collections.Specialized.NameValueCollection)
```


|Parameter Name|Remarks|
|--------------|-------|
|script|The script content|
|args|-|


#### Shell
```csharp
Microsoft.VisualBasic.Scripting.ExternalCall.Shell(System.String,System.Collections.Specialized.NameValueCollection)
```


|Parameter Name|Remarks|
|--------------|-------|
|path|The script file path|
|args|-|

> Perl脚本测试通过！


### Properties

#### __host
脚本宿主的可执行文件的路径
