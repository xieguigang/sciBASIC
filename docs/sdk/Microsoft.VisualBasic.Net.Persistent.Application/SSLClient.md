# SSLClient
_namespace: [Microsoft.VisualBasic.Net.Persistent.Application](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.SSLClient.#ctor(System.Net.IPEndPoint,System.Int64,Microsoft.VisualBasic.Net.Persistent.PushMessage,Microsoft.VisualBasic.Net.Abstract.ExceptionHandler)
```


|Parameter Name|Remarks|
|--------------|-------|
|services|-|
|ID|-|
|DataRequestHandle|Public Delegate Function PushMessage(USER_ID As @``T:System.Int64``, Message As @``T:Microsoft.VisualBasic.Net.Protocols.RequestStream``) As @``T:Microsoft.VisualBasic.Net.Protocols.RequestStream``|
|ExceptionHandler|-|


#### Logon
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.SSLClient.Logon(Microsoft.VisualBasic.Net.SSL.Certificate)
```
使用已经拥有的用户证书登录服务器，这一步省略了握手步骤

|Parameter Name|Remarks|
|--------------|-------|
|UserToken|-|


#### SendMessage
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.SSLClient.SendMessage(System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream)
```
消息在这个函数之中自动被加密处理

|Parameter Name|Remarks|
|--------------|-------|
|USER_ID|-|
|request|-|



