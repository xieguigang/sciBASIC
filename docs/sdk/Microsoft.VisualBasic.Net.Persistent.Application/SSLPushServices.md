# SSLPushServices
_namespace: [Microsoft.VisualBasic.Net.Persistent.Application](./index.md)_

消息都是经过加密操作了的



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.SSLPushServices.#ctor(System.Int32,System.Object,Microsoft.VisualBasic.Net.Persistent.OffLineMessageSendHandler,Microsoft.VisualBasic.Net.Abstract.ExceptionHandler)
```


|Parameter Name|Remarks|
|--------------|-------|
|LocalPort|-|
|OffLineMessageSendHandler|
 Public Delegate Sub @``T:Microsoft.VisualBasic.Net.Persistent.OffLineMessageSendHandler``(FromUSER_ID As @``T:System.Int64``, USER_ID As @``T:System.Int64``, Message As @``T:Microsoft.VisualBasic.Net.Protocols.RequestStream``)
 |
|exHandler|-|


#### Install
```csharp
Microsoft.VisualBasic.Net.Persistent.Application.SSLPushServices.Install(Microsoft.VisualBasic.Net.SSL.Certificate,System.Boolean,System.String)
```
安装新的用户私有密匙

|Parameter Name|Remarks|
|--------------|-------|
|CA|-|
|[overrides]|-|



### Properties

#### CA
共有密匙
#### PrivateKeys
连接到当前的这个服务器上面的客户端的私有密匙列表
