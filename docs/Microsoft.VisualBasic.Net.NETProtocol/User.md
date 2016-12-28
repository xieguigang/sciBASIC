# User
_namespace: [Microsoft.VisualBasic.Net.NETProtocol](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.NETProtocol.User.#ctor(Microsoft.VisualBasic.Net.IPEndPoint,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|remote|User API的接口|


#### __downloadMsg
```csharp
Microsoft.VisualBasic.Net.NETProtocol.User.__downloadMsg
```
可能会存在多条数据

#### __pushUpdate
```csharp
Microsoft.VisualBasic.Net.NETProtocol.User.__pushUpdate(System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream)
```
得到服务器端发送过来的更新推送的消息头

|Parameter Name|Remarks|
|--------------|-------|
|uid|-|
|args|-|


#### __register
```csharp
Microsoft.VisualBasic.Net.NETProtocol.User.__register(Microsoft.VisualBasic.Net.NETProtocol.Protocols.InitPOSTBack,Microsoft.VisualBasic.Net.NETProtocol.User)
```
在消息推送服务器上面注册自己的句柄


