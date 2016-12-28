# PipelineCLI
_namespace: [Microsoft.VisualBasic.Terminal](./index.md)_

a | b - 管道命令在读写方面更加适合于文本数据，由于省去了IO的时间，故而效率较高



### Methods

#### Start
```csharp
Microsoft.VisualBasic.Terminal.PipelineCLI.Start(System.IO.Stream,System.String,System.String)
```
使用管道的方法启动下游的应用程序

|Parameter Name|Remarks|
|--------------|-------|
|app|-|
|args|-|

> 
>  http://stackoverflow.com/questions/30546522/how-to-use-a-pipe-between-two-processes-in-process-start
>  
>  let the OS do it. ``StartInfo.FileName = "cmd"`` then prepend ``executablepath`` to params so it looks 
>  the way you would enter it in a command window; 
>  ``StartInfo.Arguments = params`` then start the process 
>  
>  – Plutonix May 30 '15 at 15:13
>  


