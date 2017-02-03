# Extensions
_namespace: [Microsoft.VisualBasic.Data.IO](./index.md)_





### Methods

#### OpenBinaryReader
```csharp
Microsoft.VisualBasic.Data.IO.Extensions.OpenBinaryReader(System.String,Microsoft.VisualBasic.Text.Encodings,System.Int64)
```


|Parameter Name|Remarks|
|--------------|-------|
|path|-|
|encoding|-|
|buffered%|
 默认的缓存临界大小是10MB，当超过这个大小的时候则不会进行缓存，假若没有操作这个临界值，则程序会一次性读取所有数据到内存之中以提高IO性能
 |



