# wildcardsCompatible
_namespace: [Microsoft.VisualBasic.Language.UnixBash](./index.md)_

Using regular expression to find a match on the file name.



### Methods

#### IsMatch
```csharp
Microsoft.VisualBasic.Language.UnixBash.wildcardsCompatible.IsMatch(System.String)
```
Linux/Mac系统不支持Windows系统的通配符，所以在这里是用正则表达式来保持代码的兼容性

|Parameter Name|Remarks|
|--------------|-------|
|path|-|



### Properties

#### opt
Windows系统上面文件路径不区分大小写，但是Linux、Mac系统却区分大小写
 所以使用这个来保持对Windows文件系统的兼容性
