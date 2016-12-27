# Extensions
_namespace: [Microsoft.VisualBasic.FileIO](./index.md)_



> 
>  @``T:System.Text.Encoding``会和@``T:Microsoft.VisualBasic.Text.Encodings``产生冲突，
>  使用这个单独的拓展模块，但是位于不同的命名空间来解决这个问题。
>  


### Methods

#### FlushAllLines``1
```csharp
Microsoft.VisualBasic.FileIO.Extensions.FlushAllLines``1(System.Collections.Generic.IEnumerable{``0},System.String,System.Text.Encoding)
```
Write all object into a text file by using its @``T:System.Object`` method.

|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|saveTo|-|
|encoding|-|


#### OpenWriter
```csharp
Microsoft.VisualBasic.FileIO.Extensions.OpenWriter(System.String,System.Text.Encoding,System.String)
```
Open text file writer, this function will auto handle all things.

|Parameter Name|Remarks|
|--------------|-------|
|path|假若路径是指向一个已经存在的文件，则原有的文件数据将会被清空|
|encoding|-|



