# FileSystemAPI
_namespace: [Microsoft.VisualBasic.Language.UnixBash](./index.md)_





### Methods

#### wildcards
```csharp
Microsoft.VisualBasic.Language.UnixBash.FileSystemAPI.wildcards(System.String[])
```
可以使用这个来限定文件或者文件夹对象的搜索范围

|Parameter Name|Remarks|
|--------------|-------|
|__wildcards|可以为文件拓展或者对文件名的通配符的表达式，假若这个是空的，则会默认搜索所有文件*.*|



### Properties

#### l
Long name(DIR+fiename), if not only file name.
#### ls
ls -l -ext("*.xml") <= DIR, The filesystem search proxy
#### lsDIR
Searching the directory, if this parameter is not presents, then returns search file.
#### r
递归的搜索
