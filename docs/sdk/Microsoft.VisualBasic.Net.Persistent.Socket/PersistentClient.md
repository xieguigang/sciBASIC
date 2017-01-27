# PersistentClient
_namespace: [Microsoft.VisualBasic.Net.Persistent.Socket](./index.md)_

请注意，这个对象是应用于客户端与服务器保持长连接所使用，并不会主动发送消息给服务器，而是被动的接受服务器的数据请求



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.Persistent.Socket.PersistentClient.#ctor(System.String,System.Int32,Microsoft.VisualBasic.Net.Abstract.ExceptionHandler)
```


|Parameter Name|Remarks|
|--------------|-------|
|RemotePort|-|
|ExceptionHandler|Public Delegate Sub ExceptionHandler(ex As Exception)|


#### __send
```csharp
Microsoft.VisualBasic.Net.Persistent.Socket.PersistentClient.__send(System.Net.Sockets.Socket,System.String)
```
????
 An exception of type 'System.Net.Sockets.SocketException' occurred in System.dll but was not handled in user code
 Additional information: A request to send or receive data was disallowed because the socket is not connected and
 (when sending on a datagram socket using a sendto call) no address was supplied

|Parameter Name|Remarks|
|--------------|-------|
|client|-|
|data|-|


#### BeginConnect
```csharp
Microsoft.VisualBasic.Net.Persistent.Socket.PersistentClient.BeginConnect
```
函数会想服务器上面的socket对象一样在这里发生阻塞

#### ConnectCallback
```csharp
Microsoft.VisualBasic.Net.Persistent.Socket.PersistentClient.ConnectCallback(System.IAsyncResult)
```
Retrieve the socket from the state object.

|Parameter Name|Remarks|
|--------------|-------|
|ar|-|


#### readDataBuffer
```csharp
Microsoft.VisualBasic.Net.Persistent.Socket.PersistentClient.readDataBuffer(Microsoft.VisualBasic.Net.Persistent.Socket.PersistentClient.StateObject,System.IAsyncResult)
```
Read data from the remote device.

|Parameter Name|Remarks|
|--------------|-------|
|state|-|
|ar|-|


#### Receive
```csharp
Microsoft.VisualBasic.Net.Persistent.Socket.PersistentClient.Receive(Microsoft.VisualBasic.Net.Persistent.Socket.PersistentClient.StateObject)
```
An exception of type '@``T:System.Net.Sockets.SocketException``' occurred in System.dll but was not handled in user code
 Additional information: A request to send or receive data was disallowed because the socket is not connected and
 (when sending on a datagram socket using a sendto call) no address was supplied

|Parameter Name|Remarks|
|--------------|-------|
|client|-|


#### ReceiveCallback
```csharp
Microsoft.VisualBasic.Net.Persistent.Socket.PersistentClient.ReceiveCallback(System.IAsyncResult)
```
Retrieve the state object and the client socket from the asynchronous state object.

|Parameter Name|Remarks|
|--------------|-------|
|ar|-|



### Properties

#### OnServerHashCode
本客户端socket在服务器上面的哈希句柄值
#### port
The port number for the remote device.
#### remoteEP
Remote End Point
#### RemoteServerShutdown
远程主机强制关闭连接之后触发这个动作
