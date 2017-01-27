# MMFSocket
_namespace: [Microsoft.VisualBasic.MMFProtocol](./index.md)_

MMFProtocol socket object for the inter-process communication on the localhost, this can be using for the data exchange between two process.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.MMFProtocol.MMFSocket.#ctor(System.String,Microsoft.VisualBasic.MMFProtocol.DataArrival)
```


|Parameter Name|Remarks|
|--------------|-------|
|uri|-|
|dataArrivals|
 Public Delegate Sub @``M:Microsoft.VisualBasic.MMFProtocol.MMFSocket.__dataArrival(System.Byte[])``(byteData As @``T:System.Byte``())
 会优先于事件@``M:Microsoft.VisualBasic.MMFProtocol.MMFSocket.__dataArrival(System.Byte[])``的发生|


#### ReadData
```csharp
Microsoft.VisualBasic.MMFProtocol.MMFSocket.ReadData
```
直接从映射文件之中读取数据

#### SendMessage
```csharp
Microsoft.VisualBasic.MMFProtocol.MMFSocket.SendMessage(System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|s|@``P:System.Text.Encoding.UTF8``|



