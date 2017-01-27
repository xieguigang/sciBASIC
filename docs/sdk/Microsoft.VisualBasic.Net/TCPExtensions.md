# TCPExtensions
_namespace: [Microsoft.VisualBasic.Net](./index.md)_





### Methods

#### ConnectSocket
```csharp
Microsoft.VisualBasic.Net.TCPExtensions.ConnectSocket(System.String,System.Int32)
```
假若不能成功的建立起连接的话，则会抛出错误

|Parameter Name|Remarks|
|--------------|-------|
|server|-|
|port|-|


#### GetFirstAvailablePort
```csharp
Microsoft.VisualBasic.Net.TCPExtensions.GetFirstAvailablePort(System.Int32)
```
Get the first available TCP port on this local machine.
 (获取第一个可用的端口号，请注意，在高并发状态下可能会出现端口被占用的情况，
 所以这时候建议将**`BEGIN_PORT`**设置为-1，则本函数将会尝试使用随机数来分配可用端口，从而避免一些系统崩溃的情况产生)

|Parameter Name|Remarks|
|--------------|-------|
|BEGIN_PORT|Check the local port available from this port value.(从这个端口开始检测)|


#### Ping
```csharp
Microsoft.VisualBasic.Net.TCPExtensions.Ping(Microsoft.VisualBasic.Net.AsynInvoke,System.Int32)
```
-1 ping failure

|Parameter Name|Remarks|
|--------------|-------|
|invoke|-|
|timeout|-|


#### PortIsAvailable
```csharp
Microsoft.VisualBasic.Net.TCPExtensions.PortIsAvailable(System.Int32)
```
检查指定端口是否已用

|Parameter Name|Remarks|
|--------------|-------|
|port|-|


#### PortIsUsed
```csharp
Microsoft.VisualBasic.Net.TCPExtensions.PortIsUsed
```
获取操作系统已用的端口号


