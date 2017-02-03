# PushServer
_namespace: [Microsoft.VisualBasic.Net.NETProtocol](./index.md)_

保持长连接，向用户客户端发送更新消息的服务器



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.NETProtocol.PushServer.#ctor(System.Int32,System.Int32,System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|services|长连接socket的端口|
|invoke|服务器模块工作端口|
|userAPI|用户端口|


#### GetMsg
```csharp
Microsoft.VisualBasic.Net.NETProtocol.PushServer.GetMsg(System.Int64)
```
得到某一个用户的消息

|Parameter Name|Remarks|
|--------------|-------|
|uid|-|


#### PushUpdate
```csharp
Microsoft.VisualBasic.Net.NETProtocol.PushServer.PushUpdate(Microsoft.VisualBasic.Net.Protocols.RequestStream)
```
其他的服务器模块通过API发送数据包来推送服务器上，通过这个方法写入数据缓存，然后发送消息更新

|Parameter Name|Remarks|
|--------------|-------|
|req|-|


#### Run
```csharp
Microsoft.VisualBasic.Net.NETProtocol.PushServer.Run
```
线程会在这里被阻塞

#### SendMessage
```csharp
Microsoft.VisualBasic.Net.NETProtocol.PushServer.SendMessage(System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream)
```
向用户socket发送消息

|Parameter Name|Remarks|
|--------------|-------|
|uid|-|
|msg|-|



### Properties

#### __invokeAPI
其他的服务器模块对消息推送模块进行操作更新的通道
#### __msgs
用户数据缓存池
#### __userAPI
客户端进行数据读取的通道
#### UserSocket
Push update notification to user client
