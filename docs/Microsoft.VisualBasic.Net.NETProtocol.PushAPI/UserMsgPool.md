# UserMsgPool
_namespace: [Microsoft.VisualBasic.Net.NETProtocol.PushAPI](./index.md)_

用来缓存消息信息的用户的消息池



### Methods

#### Allocation
```csharp
Microsoft.VisualBasic.Net.NETProtocol.PushAPI.UserMsgPool.Allocation(System.Int64)
```
为新的用户分配存储空间

|Parameter Name|Remarks|
|--------------|-------|
|uid|-|


#### Pop
```csharp
Microsoft.VisualBasic.Net.NETProtocol.PushAPI.UserMsgPool.Pop(System.Int64)
```
读取一条数据

|Parameter Name|Remarks|
|--------------|-------|
|uid|-|


#### Push
```csharp
Microsoft.VisualBasic.Net.NETProtocol.PushAPI.UserMsgPool.Push(System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream)
```
想用户消息池之中写入数据缓存

|Parameter Name|Remarks|
|--------------|-------|
|uid|-|
|msg|-|



### Properties

#### __msgs
按照先后顺序排列的用户消息队列
