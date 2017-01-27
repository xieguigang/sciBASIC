# IniFile
_namespace: [Microsoft.VisualBasic.ComponentModel.Settings.Inf](./index.md)_

Ini file I/O handler



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.ComponentModel.Settings.Inf.IniFile.#ctor(System.String)
```
Open a ini file handle.

|Parameter Name|Remarks|
|--------------|-------|
|INIPath|-|


#### GetPrivateProfileString
```csharp
Microsoft.VisualBasic.ComponentModel.Settings.Inf.IniFile.GetPrivateProfileString(System.String,System.String,System.String,System.Text.StringBuilder,System.Int32,System.String)
```
为初始化文件中指定的条目取得字串

|Parameter Name|Remarks|
|--------------|-------|
|section|
 String，欲在其中查找条目的小节名称。这个字串不区分大小写。如设为vbNullString，就在lpReturnedString
 缓冲区内装载这个ini文件所有小节的列表。
 |
|key|
 String，欲获取的项名或条目名。这个字串不区分大小写。如设为vbNullString，就在lpReturnedString
 缓冲区内装载指定小节所有项的列表
 |
|def|String，指定的条目没有找到时返回的默认值。可设为空（""）|
|retVal|String，指定一个字串缓冲区，长度至少为nSize|
|size|Long，指定装载到lpReturnedString缓冲区的最大字符数量|
|filePath|
 String，初始化文件的名字。如没有指定一个完整路径名，windows就在Windows目录中查找文件
 |


_returns: 
 Long，复制到lpReturnedString缓冲区的字节数量，其中不包括那些NULL中止字符。如lpReturnedString
 缓冲区不够大，不能容下全部信息，就返回nSize-1（若lpApplicationName或lpKeyName为NULL，则返回nSize-2）
 _

#### WritePrivateProfileString
```csharp
Microsoft.VisualBasic.ComponentModel.Settings.Inf.IniFile.WritePrivateProfileString(System.String,System.String,System.String,System.String)
```
Write a string value into a specific section in a specifc ini profile.(在初始化文件指定小节内设置一个字串)

|Parameter Name|Remarks|
|--------------|-------|
|section|
 @``T:System.String``，要在其中写入新字串的小节名称。这个字串不区分大小写
 |
|key|
 @``T:System.String``，要设置的项名或条目名。这个字串不区分大小写。
 用@``F:Microsoft.VisualBasic.Constants.vbNullString``可删除这个小节的所有设置项
 |
|val|
 @``T:System.String``，指定为这个项写入的字串值。用@``F:Microsoft.VisualBasic.Constants.vbNullString``表示删除这个项现有的字串
 |
|filePath|
 @``T:System.String``，初始化文件的名字。如果没有指定完整路径名，则windows会在windows目录查找文件。
 如果文件没有找到，则函数会创建它|


_returns: Long，非零表示成功，零表示失败。会设置@``M:Microsoft.VisualBasic.Win32.GetLastErrorAPI.GetLastError``_


