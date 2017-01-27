# ServicesFolk
_namespace: [Microsoft.VisualBasic.Parallel](./index.md)_

主服务和子服务之间的相互作用的特点是子服务不会知道主服务节点的数据接口，所有的交互都是通过子服务上面的一个模块来监听主服务来实现的
 当主服务有数据需要向子服务更新的时候，会主动发送数据请求至子服务节点

> 当前的用户规模还比较小这里仅仅是实现了本地的调用，后面考虑到业务吞吐量的问题，会将服务的调用分开到两台物理主机之上


### Methods

#### Folk
```csharp
Microsoft.VisualBasic.Parallel.ServicesFolk.Folk(System.String,System.String@,System.Diagnostics.Process@)
```
函数返回子进程的交互数据通信的端口号

|Parameter Name|Remarks|
|--------------|-------|
|assm|-|
|CLI|命令行参数字符串，可以在这里加入一些其他的自定义数据|


_returns: 函数返回子服务的交互端口_

#### ReturnPortal
```csharp
Microsoft.VisualBasic.Parallel.ServicesFolk.ReturnPortal(Microsoft.VisualBasic.CommandLine.CommandLine,System.Int32,System.String)
```
子服务向服务主节点返回端口号数据，这个方法需要要在子服务上面的服务程序启动之后再调用

|Parameter Name|Remarks|
|--------------|-------|
|CLI|-|
|Port|-|
|addArgs|额外返回的参数信息|



