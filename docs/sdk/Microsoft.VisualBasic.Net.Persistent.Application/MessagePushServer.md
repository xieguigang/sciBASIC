# MessagePushServer
_namespace: [Microsoft.VisualBasic.Net.Persistent.Application](./index.md)_

长连接模式的消息推送服务器



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.MessagePushServer.#ctor(System.Int32,Microsoft.VisualBasic.Net.Persistent.OffLineMessageSendHandler,Microsoft.VisualBasic.Net.Abstract.ExceptionHandler)
```


|Parameter Name|Remarks|
|--------------|-------|
|LocalPort|-|
|OffLineMessageSendHandler|Public Delegate Sub @``T:Microsoft.VisualBasic.Net.Persistent.OffLineMessageSendHandler``(FromUSER_ID As @``T:System.Int64``, USER_ID As @``T:System.Int64``, Message As @``T:Microsoft.VisualBasic.Net.Protocols.RequestStream``)|
|exHandler|-|


#### __requestHandlerInterface
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.MessagePushServer.__requestHandlerInterface(System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream,System.Net.IPEndPoint)
```
只要是为ssl服务设置的

|Parameter Name|Remarks|
|--------------|-------|
|remote|-|


#### __sendMessage
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.MessagePushServer.__sendMessage(Microsoft.VisualBasic.Net.Persistent.Socket.WorkSocket,System.Int64,System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream)
```


|Parameter Name|Remarks|
|--------------|-------|
|socket|-|
|From|这个是这一条消息的源头，可能需要进行映射|
|USER_ID|-|
|Message|-|


#### __usrInvokeSend
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.MessagePushServer.__usrInvokeSend(System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream,System.Net.IPEndPoint)
```
用户客户端请求发送消息至指定编号的用户的终端之上

|Parameter Name|Remarks|
|--------------|-------|
|CA|-|
|request|-|
|remote|-|


#### AcceptClient
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.MessagePushServer.AcceptClient(Microsoft.VisualBasic.Net.Persistent.Socket.WorkSocket)
```
建立一个新的连接

|Parameter Name|Remarks|
|--------------|-------|
|Client|-|


#### DisconnectUser
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.MessagePushServer.DisconnectUser(System.Int64,System.Boolean)
```
Disconnect user persistent connection who have the specific **`user_id`** from this server.
 (断开服务器与用户客户端的长连接)

|Parameter Name|Remarks|
|--------------|-------|
|USER_ID|This user will be deleted from the server registry.|
|removeCA|是否在删除socket句柄的时候还会删除相对应的ssl证书|


#### RemoveFreeConnections
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.MessagePushServer.RemoveFreeConnections
```
哈希值不存在于现有的登录用户列表之中就是空闲连接

#### SendMessage
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.MessagePushServer.SendMessage(System.Int64,System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream)
```


|Parameter Name|Remarks|
|--------------|-------|
|From|-|
|USER_ID|-|
|Message|-|



### Properties

#### _sslLayer
使用证书来加密发出去的消息
#### _workSocket
客户端对这个服务器的端口号是自动配置的，只需要向客户端返回@``F:Microsoft.VisualBasic.Net.Persistent.Socket.ServicesSocket._LocalPort``端口就可以了
#### LocalPort
从这个端口号进行登录（协同长连接的socket正常工作的socket的端口号，可以看作为UserAPI）
#### UidMappings
将外部编号映射为内部的客户端句柄
 假若找不到，请返回-1
