# IOExtensions
_namespace: [Microsoft.VisualBasic](./index.md)_

IO函数拓展



### Methods

#### FixPath
```csharp
Microsoft.VisualBasic.IOExtensions.FixPath(System.String@)
```
为了方便在linux上面使用，这里会处理一下file://这种情况，请注意参数是ByRef引用的

|Parameter Name|Remarks|
|--------------|-------|
|path$|-|


#### FlushAllLines``1
```csharp
Microsoft.VisualBasic.IOExtensions.FlushAllLines``1(System.Collections.Generic.IEnumerable{``0},System.String,Microsoft.VisualBasic.Text.Encodings)
```
Write all object into a text file by using its @``T:System.Object`` method.

|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|saveTo|-|
|encoding|-|


#### FlushStream
```csharp
Microsoft.VisualBasic.IOExtensions.FlushStream(System.Collections.Generic.IEnumerable{System.Byte},System.String)
```
Save the binary data into the filesystem.(保存二进制数据包值文件系统)

|Parameter Name|Remarks|
|--------------|-------|
|buf|The binary bytes data of the target package's data.(目标二进制数据)|
|path|The saved file path of the target binary data chunk.(目标二进制数据包所要进行保存的文件名路径)|


#### Open
```csharp
Microsoft.VisualBasic.IOExtensions.Open(System.String,System.IO.FileMode)
```
打开本地文件指针，这是一个安全的函数，会自动创建不存在的文件夹

|Parameter Name|Remarks|
|--------------|-------|
|path|文件的路径|
|mode|文件指针的打开模式|


#### ReadBinary
```csharp
Microsoft.VisualBasic.IOExtensions.ReadBinary(System.String)
```
@``M:System.IO.File.ReadAllBytes(System.String)``, if the file is not exists on the filesystem, then a empty array will be return.

|Parameter Name|Remarks|
|--------------|-------|
|path|-|



