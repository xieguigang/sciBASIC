# StreamIO
_namespace: [Microsoft.VisualBasic.Data.csv.IO](./index.md)_





### Methods

#### GetType
```csharp
Microsoft.VisualBasic.Data.csv.IO.StreamIO.GetType(Microsoft.VisualBasic.Data.csv.IO.File,System.Type[])
```
根据文件的头部的定义，从**`types`**之中选取得到最合适的类型的定义

|Parameter Name|Remarks|
|--------------|-------|
|csv|-|
|types|-|


#### SaveDataFrame
```csharp
Microsoft.VisualBasic.Data.csv.IO.StreamIO.SaveDataFrame(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Data.csv.IO.RowObject},System.String,System.Text.Encoding)
```
Save this csv document into a specific file location **`path`**.

|Parameter Name|Remarks|
|--------------|-------|
|path|
 假若路径是指向一个已经存在的文件，则原有的文件数据将会被清空覆盖
 |

> 当目标保存路径不存在的时候，会自动创建文件夹


