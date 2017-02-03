# CollectionAttribute
_namespace: [Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection](./index.md)_

This property is a array data type object.(并不建议使用本Csv属性来储存大量的文本字符串，极容易出错)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.CollectionAttribute.#ctor(System.String,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|Name|-|
|Delimiter|由于受正则表达式的解析速度的影响，因为CSV文件是使用逗号进行分隔的，假若使用逗号的话，正则表达式的解析速度会比较低，故在这里优先考虑使用分号来作为分隔符|


#### CreateObject``1
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.CollectionAttribute.CreateObject``1(System.Collections.Generic.IEnumerable{``0})
```
Collection of object into a cell string content.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|



