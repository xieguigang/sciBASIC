# USER
_namespace: [Microsoft.VisualBasic.Net.Persistent.Application](./index.md)_

服务器也相当于一个USER，只不过服务器的UID为0，即最高级的用户



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.USER.#ctor(System.String,System.Int32,System.Int64,Microsoft.VisualBasic.Net.Persistent.PushMessage,Microsoft.VisualBasic.Net.Abstract.ExceptionHandler)
```


|Parameter Name|Remarks|
|--------------|-------|
|HostName|-|
|RemotePort|-|
|ID|-|
|DataRequestHandle|使用这个函数来获取外部发送过来的用户消息|
|ExceptionHandler|-|


#### __sendMessageToMe
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.USER.__sendMessageToMe(System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream,System.Net.IPEndPoint)
```


|Parameter Name|Remarks|
|--------------|-------|
|CA|-|
|request|-|
|remote|由于数据都是通过中心服务器转发的，所以这个已经没有存在的意义了，但是为了和短连接的socket的数据处理接口保持兼容，所以还保留这个参数|


#### BeginConnect
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.USER.BeginConnect(Microsoft.VisualBasic.Net.SSL.Certificate,System.Windows.Forms.MethodInvoker)
```
不会发生阻塞

|Parameter Name|Remarks|
|--------------|-------|
|ForceCloseConnection|远程主机强制关闭连接之后触发这个动作|


#### SendMessage
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.USER.SendMessage(System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream)
```
True标识发送成功，False标识用户离线

|Parameter Name|Remarks|
|--------------|-------|
|USER_ID|-|
|Message|在发送之前请对消息进行加密处理|



