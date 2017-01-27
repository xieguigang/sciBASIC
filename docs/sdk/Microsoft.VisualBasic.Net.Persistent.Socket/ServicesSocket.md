# ServicesSocket
_namespace: [Microsoft.VisualBasic.Net.Persistent.Socket](./index.md)_



> 
>  一、TCP长连接
>  
>  正常情况下，一条TCP连接建立后，只要双不提出关闭请求并且不出现异常情况，这条连接是一直存在的，
>  操作系统不会自动去关闭它，甚至经过物理网络拓扑的改变之后仍然可以使用。
>  所以一条连接保持几天、几个月、几年或者更长时间都有可能，只要不出现异常情况或由用户（应用层）主动关闭。
>  在编程中， 往往需要建立一条TCP连接， 并且长时间处于连接状态。
>  所谓的TCP长连接并没有确切的时间限制， 而是说这条连接需要的时间比较长。
>  
>  二、TCP连接的正常中断
>  
>  TCP连接在事务处理完毕之后， 由一方提出关闭连接请求， 双方通过四次握手（建立连接是三次握手， 
>  当然可以通过优化TCP / IP协议栈来减少握手的次数来提高性能， 但这样会形成不规范或者不优雅的通信）来正常关闭连接
>  
>  三、TCP连接的异常中断
>  
>  导致TCP连接异常中断的因素有： 物理连接被中断、操作系统down机、程序崩溃等等。
>  


### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.Persistent.Socket.ServicesSocket.#ctor(System.Int32,Microsoft.VisualBasic.Net.Abstract.ExceptionHandler)
```
消息处理的方法接口： Public Delegate Function DataResponseHandler(str As String, RemotePort As Integer) As String

|Parameter Name|Remarks|
|--------------|-------|
|LocalPort|监听的本地端口号，假若需要进行端口映射的话，则可以在@``M:Microsoft.VisualBasic.Net.Persistent.Socket.ServicesSocket.Run``方法之中设置映射的端口号|


#### __initSocket
```csharp
Microsoft.VisualBasic.Net.Persistent.Socket.ServicesSocket.__initSocket(System.Net.IPEndPoint)
```
Bind the socket to the local endpoint and listen for incoming connections.

|Parameter Name|Remarks|
|--------------|-------|
|localEndPoint|-|


#### __initSocketThread
```csharp
Microsoft.VisualBasic.Net.Persistent.Socket.ServicesSocket.__initSocketThread(System.Net.Sockets.Socket)
```
Create the state object for the async receive.

|Parameter Name|Remarks|
|--------------|-------|
|handler|-|


#### AcceptCallback
```csharp
Microsoft.VisualBasic.Net.Persistent.Socket.ServicesSocket.AcceptCallback(System.IAsyncResult)
```
Get the socket that handles the client request.

|Parameter Name|Remarks|
|--------------|-------|
|ar|-|


#### Dispose
```csharp
Microsoft.VisualBasic.Net.Persistent.Socket.ServicesSocket.Dispose
```
Stop the server socket listening threads.(终止服务器Socket监听线程)

#### Run
```csharp
Microsoft.VisualBasic.Net.Persistent.Socket.ServicesSocket.Run(System.Net.IPEndPoint)
```
This server waits for a connection and then uses asychronous operations to
 accept the connection, get data from the connected client,
 echo that data back to the connected client.
 It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)


### Properties

#### _LocalPort
Socket对象监听的端口号
#### disposedValue
退出监听线程所需要的
#### LocalPort
The server services listening on this local port.(当前的这个服务器对象实例所监听的本地端口号)
