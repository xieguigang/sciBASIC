# AsynInvoke
_namespace: [Microsoft.VisualBasic.Net](./index.md)_

The server socket should returns some data string to this client or this client will stuck at the @``M:Microsoft.VisualBasic.Net.AsynInvoke.SendMessage(Microsoft.VisualBasic.Net.Protocols.RequestStream)`` function.
 (服务器端@``T:Microsoft.VisualBasic.Net.TcpSynchronizationServicesSocket``必须要返回数据，否则本客户端会在@``M:Microsoft.VisualBasic.Net.AsynInvoke.SendMessage(Microsoft.VisualBasic.Net.Protocols.RequestStream)``函数位置一直处于等待的状态)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.AsynInvoke.#ctor(System.String,System.Int32,Microsoft.VisualBasic.Net.Abstract.ExceptionHandler)
```


|Parameter Name|Remarks|
|--------------|-------|
|remotePort|-|
|ExceptionHandler|Public Delegate Sub ExceptionHandler(ex As Exception)|


#### __send
```csharp
Microsoft.VisualBasic.Net.AsynInvoke.__send(System.Net.Sockets.Socket,System.Byte[])
```
????
 An exception of type 'System.Net.Sockets.SocketException' occurred in System.dll but was not handled in user code
 Additional information: A request to send or receive data was disallowed because the socket is not connected and
 (when sending on a datagram socket using a sendto call) no address was supplied

|Parameter Name|Remarks|
|--------------|-------|
|client|-|
|byteData|-|


#### LocalConnection
```csharp
Microsoft.VisualBasic.Net.AsynInvoke.LocalConnection(System.Int32,Microsoft.VisualBasic.Net.Abstract.ExceptionHandler)
```
初始化一个在本机进行进程间通信的Socket对象

|Parameter Name|Remarks|
|--------------|-------|
|LocalPort|-|
|ExceptionHandler|-|


#### OperationTimeOut
```csharp
Microsoft.VisualBasic.Net.AsynInvoke.OperationTimeOut(System.String)
```
判断服务器所返回来的数据是否为操作超时

|Parameter Name|Remarks|
|--------------|-------|
|str|-|


#### Receive
```csharp
Microsoft.VisualBasic.Net.AsynInvoke.Receive(System.Net.Sockets.Socket)
```
An exception of type '@``T:System.Net.Sockets.SocketException``' occurred in System.dll but was not handled in user code
 Additional information: A request to send or receive data was disallowed because the socket is not connected and
 (when sending on a datagram socket using a sendto call) no address was supplied

|Parameter Name|Remarks|
|--------------|-------|
|client|-|


#### SendMessage
```csharp
Microsoft.VisualBasic.Net.AsynInvoke.SendMessage(System.Byte[])
```
最底层的消息发送函数

|Parameter Name|Remarks|
|--------------|-------|
|Message|-|



### Properties

#### connectDone
' ManualResetEvent instances signal completion.
#### LocalIPAddress
Gets the IP address of this local machine.
 (获取本机对象的IP地址，请注意这个属性获取得到的仅仅是本机在局域网内的ip地址，假若需要获取得到公网IP地址，还需要外部服务器的帮助才行)
#### port
The port number for the remote device.
#### remoteEP
Remote End Point
#### response
The response from the remote device.
