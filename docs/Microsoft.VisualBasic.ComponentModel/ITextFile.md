# ITextFile
_namespace: [Microsoft.VisualBasic.ComponentModel](./index.md)_

Object model of the text file doucment.(文本文件的对象模型，这个文本文件对象在Disposed的时候会自动保存其中的数据)



### Methods

#### getPath
```csharp
Microsoft.VisualBasic.ComponentModel.ITextFile.getPath(System.String)
```
Automatically determine the path paramater: If the target path is empty, then return
 the file object path @``P:Microsoft.VisualBasic.ComponentModel.ITextFile.FilePath`` property, if not then return the
 **`path`** directly.
 (当**`path`**的值不为空的时候，本对象之中的路径参数将会被替换，反之返回本对象的路径参数)

|Parameter Name|Remarks|
|--------------|-------|
|path|用户所输入的文件路径|



### Properties

#### FilePath
The storage filepath of this text file.
