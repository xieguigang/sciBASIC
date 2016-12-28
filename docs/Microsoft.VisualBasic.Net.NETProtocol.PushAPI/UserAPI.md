# UserAPI
_namespace: [Microsoft.VisualBasic.Net.NETProtocol.PushAPI](./index.md)_

对User client开放的协议接口，也就是用户的客户端是通过这个模块来发送消息或者读取自己的消息



### Methods

#### __getData
```csharp
Microsoft.VisualBasic.Net.NETProtocol.PushAPI.UserAPI.__getData(System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream,System.Net.IPEndPoint)
```
用户客户端尝试得到消息数据

|Parameter Name|Remarks|
|--------------|-------|
|CA|-|
|request|-|
|remote|-|


#### __userInitPOST
```csharp
Microsoft.VisualBasic.Net.NETProtocol.PushAPI.UserAPI.__userInitPOST(System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream,System.Net.IPEndPoint)
```
第一步，初始化哈希表

|Parameter Name|Remarks|
|--------------|-------|
|CA|-|
|request|user_id|
|remote|-|


#### IsValid
```csharp
Microsoft.VisualBasic.Net.NETProtocol.PushAPI.UserAPI.IsValid(Microsoft.VisualBasic.Net.NETProtocol.Protocols.UserId)
```
判断这个用户编号是否可用有效？

|Parameter Name|Remarks|
|--------------|-------|
|id|-|



### Properties

#### HashUser
反向查找@``P:Microsoft.VisualBasic.Net.NETProtocol.PushAPI.UserAPI.UserHash``
#### UserHash
用户编号转换为程序之中的唯一标识符
