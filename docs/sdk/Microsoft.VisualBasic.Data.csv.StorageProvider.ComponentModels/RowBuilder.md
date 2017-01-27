# RowBuilder
_namespace: [Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels](./index.md)_

这个是用于将Csv文件之中的行数据转换为.NET对象的



### Methods

#### Indexof
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.RowBuilder.Indexof(Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.ISchema)
```
从外部源之中获取本数据集的Schema的信息

|Parameter Name|Remarks|
|--------------|-------|
|schema|-|


#### SolveReadOnlyMetaConflicts
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.RowBuilder.SolveReadOnlyMetaConflicts
```
对于只读属性而言，由于没有写入的过程，所以在从文件加在csv数据到.NET对象的时候会被放进字典属性里面，从而会导致输出的时候出现重复的域的BUG
 故而需要在这里将字典属性之中的只读属性的名称移除掉


### Properties

#### Columns
总的列表
