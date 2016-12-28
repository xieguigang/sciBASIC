# ProtocolHandler
_namespace: [Microsoft.VisualBasic.Net.Protocols.Reflection](./index.md)_

这个模块只处理@``T:Microsoft.VisualBasic.Net.Abstract.DataRequestHandler``类型的接口



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.Protocols.Reflection.ProtocolHandler.#ctor(System.Object)
```
请注意，假若没有在目标的类型定义之中查找出入口点的定义，则这个构造函数会报错，
 假若需要安全的创建对象，可以使用@``M:Microsoft.VisualBasic.Net.Protocols.Reflection.ProtocolHandler.SafelyCreateObject``1(``0)``函数

|Parameter Name|Remarks|
|--------------|-------|
|obj|Protocol的实例|


#### HandleRequest
```csharp
Microsoft.VisualBasic.Net.Protocols.Reflection.ProtocolHandler.HandleRequest(System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream,System.Net.IPEndPoint)
```
Handle the data request from the client for socket events: @``P:Microsoft.VisualBasic.Net.TcpSynchronizationServicesSocket.Responsehandler`` or @``P:Microsoft.VisualBasic.Net.SSL.SSLSynchronizationServicesSocket.Responsehandler``

|Parameter Name|Remarks|
|--------------|-------|
|CA|-|
|request|The request stream object which contains the commands from the client|
|remoteDevcie|The IPAddress of the target incoming client data request.|


#### SafelyCreateObject``1
```csharp
Microsoft.VisualBasic.Net.Protocols.Reflection.ProtocolHandler.SafelyCreateObject``1(``0)
```
失败会返回空值

|Parameter Name|Remarks|
|--------------|-------|
|App|-|



### Properties

#### DeclaringType
这个类型建议一般为某种枚举类型
