# ProgramPathSearchTool
_namespace: [Microsoft.VisualBasic](./index.md)_

Search the path from a specific keyword.(通过关键词来推测路径)



### Methods

#### BaseName
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.BaseName(System.String)
```
Gets the name of the target file or directory, if the target is a file, then the name without the extension name.
 (获取目标文件夹的名称或者文件的不包含拓展名的名称)

#### BatchMd5Renamed
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.BatchMd5Renamed(System.String)
```
这个方法没得卵用

|Parameter Name|Remarks|
|--------------|-------|
|DIR|-|


#### BranchRule
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.BranchRule(System.String,System.String)
```
商标搜索规则

|Parameter Name|Remarks|
|--------------|-------|
|ProgramFiles|-|
|Keyword|-|


#### DIR
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.DIR(System.IO.DirectoryInfo,System.String)
```
Combine directory path.(这个主要是用于生成文件夹名称)
 
 ###### Example usage
 
 ```vbnet
 Dim images As Dictionary(Of String, String) =
 (ls - l - {"*.png", "*.jpg", "*.gif"} <= PlatformEngine.wwwroot.DIR("images")) _
 .ToDictionary(Function(file) file.StripAsId,
 AddressOf FileName)
 ```

|Parameter Name|Remarks|
|--------------|-------|
|d|-|
|name|-|


#### DirectoryExists
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.DirectoryExists(System.String)
```
Determine that the target directory is exists on the file system or not?(判断文件夹是否存在)

|Parameter Name|Remarks|
|--------------|-------|
|DIR|-|


#### FileCopy
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.FileCopy(System.String,System.String)
```
Safe file copy operation

|Parameter Name|Remarks|
|--------------|-------|
|source$|-|
|copyTo$|-|


#### FileExists
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.FileExists(System.String,System.Boolean)
```
Check if the target file object is exists on your file system or not.
 (这个函数也会自动检查目标**`path`**参数是否为空)

|Parameter Name|Remarks|
|--------------|-------|
|path|-|
|ZERO_Nonexists|将0长度的文件也作为不存在|


#### FileLength
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.FileLength(System.String)
```
Gets the file length, if the path is not exists, then returns -1.

|Parameter Name|Remarks|
|--------------|-------|
|path|-|


#### FileName
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.FileName(System.String)
```
返回``文件名称.拓展名``

|Parameter Name|Remarks|
|--------------|-------|
|path|-|


#### FileOpened
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.FileOpened(System.String)
```
Check if the file is opened by other code?(检测文件是否已经被其他程序打开使用之中)

|Parameter Name|Remarks|
|--------------|-------|
|FileName|目标文件|


#### GetBaseName
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.GetBaseName(System.String)
```
@``M:System.IO.Path.GetFileNameWithoutExtension(System.String)`` shortcuts extension.

|Parameter Name|Remarks|
|--------------|-------|
|path|-|


#### GetDirectoryFullPath
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.GetDirectoryFullPath(System.String)
```
Gets the full path of the specific directory.

|Parameter Name|Remarks|
|--------------|-------|
|dir|-|


#### GetFile
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.GetFile(System.String,System.String,System.String[])
```


|Parameter Name|Remarks|
|--------------|-------|
|DIR|-|
|keyword|-|
|ext|元素的排布是有顺序的|


#### GetFullPath
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.GetFullPath(System.String)
```
Gets the full path of the specific file.(为了兼容Linux，这个函数会自动替换路径之中的\为/符号)

|Parameter Name|Remarks|
|--------------|-------|
|file|-|


#### LoadEntryList
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.LoadEntryList(System.String,System.String[])
```
允许有重复的数据

|Parameter Name|Remarks|
|--------------|-------|
|DIR|-|
|exts|-|


#### LoadSourceEntryList
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.LoadSourceEntryList(System.String,System.String[])
```
可以使用本方法生成Entry列表；（在返回的结果之中，KEY为文件名，没有拓展名，VALUE为文件的路径）
 请注意，这个函数会搜索目标文件夹下面的所有的文件夹的

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|ext|文件类型的拓展名称|


#### Long2Short
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.Long2Short(System.String,System.String)
```
假设文件名过长发生在文件名和最后一个文件夹的路径之上

|Parameter Name|Remarks|
|--------------|-------|
|path|-|

> 
>  System.IO.PathTooLongException: The specified path, file name, or both are too long.
>  The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters.
>  

#### MkDIR
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.MkDIR(System.String)
```
Make directory

|Parameter Name|Remarks|
|--------------|-------|
|DIR|-|


#### NormalizePathString
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.NormalizePathString(System.String,System.Boolean)
```
将目标字符串之中的非法的字符替换为"_"符号以成为正确的文件名字符串

|Parameter Name|Remarks|
|--------------|-------|
|str|-|
|OnlyASCII|当本参数为真的时候，仅26个字母，0-9数字和下划线_以及小数点可以被保留下来|


#### ParentDirName
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.ParentDirName(System.String)
```
Gets the name of the file's parent directory, returns value is a name, not path.
 (获取目标文件的父文件夹的文件夹名称，是名称而非路径)

|Parameter Name|Remarks|
|--------------|-------|
|file|-|


#### ParentPath
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.ParentPath(System.String,System.Boolean)
```
这个函数是返回文件夹的路径而非名称，这个函数不依赖于系统的底层API，因为系统的底层API对于过长的文件名会出错

|Parameter Name|Remarks|
|--------------|-------|
|file|-|

> 这个函数不依赖于系统的底层API，因为系统的底层API对于过长的文件名会出错

#### PathIllegal
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.PathIllegal(System.String)
```
File path illegal?

|Parameter Name|Remarks|
|--------------|-------|
|path|-|


#### RelativePath
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.RelativePath(System.String,System.String)
```
Gets the relative path of file system object **`pcTo`** reference to the directory path **`pcFrom`**.
 (请注意，所生成的相对路径之中的字符串最后是没有文件夹的分隔符\或者/的)

|Parameter Name|Remarks|
|--------------|-------|
|pcFrom|生成相对路径的参考文件夹|
|pcTo|所需要生成相对路径的目标文件系统对象的绝对路径或者相对路径|


#### SafeCopyTo
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.SafeCopyTo(System.String,System.String)
```
进行安全的复制，出现错误不会导致应用程序崩溃，大文件不推荐使用这个函数进行复制

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|copyTo|-|


#### SearchDirectory
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.SearchDirectory(System.String,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|SpecificDrive|所制定进行搜索的驱动器，假若希望搜索整个硬盘，请留空字符串|


#### SearchProgram
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.SearchProgram(System.String,System.String)
```
Invoke the search session for the program file using a specific keyword string value.(使用某个关键词来搜索目标应用程序)

|Parameter Name|Remarks|
|--------------|-------|
|DIR|-|
|Keyword|-|


#### SearchScriptFile
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.SearchScriptFile(System.String,System.String,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|DIR|-|
|Keyword|-|
|withExtension|脚本文件的文件拓展名|


#### SourceCopy
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.SourceCopy(System.Collections.Generic.IEnumerable{System.String},System.String,System.Boolean)
```
将不同来源**`source`**的文件复制到目标文件夹**`copyto`**之中

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|copyto|-|


_returns: 返回失败的文件列表_

#### ToFileURL
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.ToFileURL(System.String)
```
Gets the URL type file path.(获取URL类型的文件路径)

|Parameter Name|Remarks|
|--------------|-------|
|Path|-|


#### TrimDIR
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.TrimDIR(System.String)
```
Removes the last \ and / character in a directory path string.
 (使用这个函数修剪文件夹路径之中的最后一个分隔符，以方便生成文件名)

|Parameter Name|Remarks|
|--------------|-------|
|DIR|-|


#### TrimSuffix
```csharp
Microsoft.VisualBasic.ProgramPathSearchTool.TrimSuffix(System.String)
```
Removes the file extension name from the file path.(去除掉文件的拓展名)

|Parameter Name|Remarks|
|--------------|-------|
|file|-|



### Properties

#### ILLEGAL_PATH_CHARACTERS_ENUMERATION
枚举所有非法的路径字符
