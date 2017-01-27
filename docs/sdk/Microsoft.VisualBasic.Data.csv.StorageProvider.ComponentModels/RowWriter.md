# RowWriter
_namespace: [Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels](./index.md)_

从.NET对象转换为Csv文件之中的行数据



### Methods

#### __buildRowMeta
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.RowWriter.__buildRowMeta(System.Object)
```
这里是含有动态属性的

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### __buildRowNullMeta
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.RowWriter.__buildRowNullMeta(System.Object)
```
这里是没有动态属性的

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### CacheIndex
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.RowWriter.CacheIndex(System.Collections.Generic.IEnumerable{System.Object},System.Int32)
```
在这个函数之中生成字典动态属性的表头

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|reorderKeys|
 + 0: 不排序
 + 1: 升序排序
 +-1: 降序排序
 |



### Properties

#### _metaBlank
填充不存在的动态属性的默认字符串
#### IsMetaIndexed
Has the meta field indexed?
