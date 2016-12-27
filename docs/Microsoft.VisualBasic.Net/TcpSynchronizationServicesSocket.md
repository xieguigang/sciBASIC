# TcpSynchronizationServicesSocket
_namespace: [Microsoft.VisualBasic.Net](./index.md)_

Socket listening object which is running at the server side asynchronous able multiple threading.
 (运行于服务器端上面的Socket监听对象，多线程模型)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.TcpSynchronizationServicesSocket.#ctor(Microsoft.VisualBasic.Net.Abstract.DataRequestHandler,System.Int32,Microsoft.VisualBasic.Net.Abstract.ExceptionHandler)
```
短连接socket服务端

|Parameter Name|Remarks|
|--------------|-------|
|DataArrivalEventHandler|-|
|LocalPort|-|
|exHandler|-|


#### BeginListen
```csharp
Microsoft.VisualBasic.Net.TcpSynchronizationServicesSocket.BeginListen(Microsoft.VisualBasic.Net.Abstract.DataRequestHandler,System.Int32,Microsoft.VisualBasic.Net.Abstract.ExceptionHandler)
```
函数返回Socket的注销方法

|Parameter Name|Remarks|
|--------------|-------|
|DataArrivalEventHandler|Public Delegate Function DataResponseHandler(str As String, RemotePort As Integer) As String|
|LocalPort|-|
|exHandler|-|


#### Dispose
```csharp
Microsoft.VisualBasic.Net.TcpSynchronizationServicesSocket.Dispose
```
Stop the server socket listening threads.(终止服务器Socket监听线程)

#### HandleRequest
```csharp
Microsoft.VisualBasic.Net.TcpSynchronizationServicesSocket.HandleRequest(System.Net.Sockets.Socket,Microsoft.VisualBasic.Net.Protocols.RequestStream)
```
All the data has been read from the client. Display it on the console.
 Echo the data back to the client.

|Parameter Name|Remarks|
|--------------|-------|
|handler|-|
|requestData|-|


#### IsServerInternalException
```csharp
Microsoft.VisualBasic.Net.TcpSynchronizationServicesSocket.IsServerInternalException(System.String)
```
SERVER_INTERNAL_EXCEPTION，Server encounter an internal exception during processing
 the data request from the remote device.
 (判断是否服务器在处理客户端的请求的时候，发生了内部错误)

|Parameter Name|Remarks|
|--------------|-------|
|replyData|-|


#### Run
```csharp
Microsoft.VisualBasic.Net.TcpSynchronizationServicesSocket.Run(System.Net.IPEndPoint)
```
This server waits for a connection and then uses asychronous operations to
 accept the connection, get data from the connected client,
 echo that data back to the connected client.
 It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)

#### Send
```csharp
Microsoft.VisualBasic.Net.TcpSynchronizationServicesSocket.Send(System.Net.Sockets.Socket,System.String)
```
Server reply the processing result of the request from the client.

|Parameter Name|Remarks|
|--------------|-------|
|handler|-|
|data|-|



### Properties

#### disposedValue
退出监听线程所需要的
#### LocalPort
The server services listening on this local port.(当前的这个服务器对象实例所监听的本地端口号)
#### Responsehandler
This function pointer using for the data request handling of the data request from the client socket. 
 [Public Delegate Function DataResponseHandler(str As @``T:System.String``, RemoteAddress As @``T:System.Net.IPEndPoint``) As @``T:System.String``]
 (这个函数指针用于处理来自于客户端的请求)
