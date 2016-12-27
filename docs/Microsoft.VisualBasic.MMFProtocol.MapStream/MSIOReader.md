# MSIOReader
_namespace: [Microsoft.VisualBasic.MMFProtocol.MapStream](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.MMFProtocol.MapStream.MSIOReader.#ctor(System.String,Microsoft.VisualBasic.MMFProtocol.DataArrival,System.Int64)
```


|Parameter Name|Remarks|
|--------------|-------|
|uri|-|
|callback|-|
|ChunkSize|内存映射文件的数据块的预分配大小|


#### ReadBadge
```csharp
Microsoft.VisualBasic.MMFProtocol.MapStream.MSIOReader.ReadBadge
```
由于考虑到可能会传递很大的数据块，所以在这里检测数据更新的话只读取头部的8个字节的数据


### Properties

#### _udtBadge
内存映射文件的更新标识符
