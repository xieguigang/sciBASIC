# IORedirectFile
_namespace: [Microsoft.VisualBasic.CommandLine](./index.md)_

Using this class object rather than @``T:Microsoft.VisualBasic.CommandLine.IORedirect`` is more encouraged.
 (假若所建立的子进程并不需要进行终端交互，相较于@``T:Microsoft.VisualBasic.CommandLine.IORedirect``对象，更加推荐使用本对象类型来执行。
 似乎@``T:Microsoft.VisualBasic.CommandLine.IORedirect``对象在创建一个子进程的时候的对象IO重定向的句柄的处理有问题，所以在这里构建一个更加简单的类型对象，
 这个IO重定向对象不具备终端交互功能)

> 先重定向到一个临时文件之中，然后再返回临时文件给用户代码


### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.CommandLine.IORedirectFile.#ctor(System.String,System.String,System.Collections.Generic.KeyValuePair{System.String,System.String}[],System.Boolean,System.String)
```
Using this class object rather than @``T:Microsoft.VisualBasic.CommandLine.IORedirect`` is more encouraged if there is no console interactive with your folked process.

|Parameter Name|Remarks|
|--------------|-------|
|file|The program file.(请注意检查路径参数，假若路径之中包含有%这个符号的话，在调用cmd的时候会失败)|
|argv|The program commandline arguments.(请注意检查路径参数，假若路径之中包含有%这个符号的话，在调用cmd的时候会失败)|
|environment|Temporary environment variable|
|FolkNew|Folk the process on a new console window if this parameter value is TRUE|
|stdRedirect|If not want to redirect the std out to your file, just leave this value blank.|


#### CopyRedirect
```csharp
Microsoft.VisualBasic.CommandLine.IORedirectFile.CopyRedirect(System.String)
```
将目标子进程的标准终端输出文件复制到一个新的文本文件之中

|Parameter Name|Remarks|
|--------------|-------|
|CopyToPath|-|


#### Run
```csharp
Microsoft.VisualBasic.CommandLine.IORedirectFile.Run
```
Start target child process and then wait for the child process exits. 
 So that the thread will be stuck at here until the sub process is 
 job done!
 (启动目标子进程，然后等待执行完毕并返回退出代码(请注意，在进程未执行完毕
 之前，整个线程会阻塞在这里))

#### Start
```csharp
Microsoft.VisualBasic.CommandLine.IORedirectFile.Start(System.Action)
```
启动子进程，但是不等待执行完毕，当目标子进程退出的时候，回调**`procExitCallback`**函数句柄

|Parameter Name|Remarks|
|--------------|-------|
|procExitCallback|-|



### Properties

#### _TempRedirect
重定向的临时文件
#### ProcessBAT
shell文件接口
#### StandardOutput
目标子进程的终端标准输出
