# Extensions
_namespace: [Microsoft.VisualBasic.Data.csv](./index.md)_

The shortcuts operation for the common csv document operations.



### Methods

#### AsDataSource``1
```csharp
Microsoft.VisualBasic.Data.csv.Extensions.AsDataSource``1(System.Collections.Generic.IEnumerable{System.String},System.String,System.Boolean)
```
Convert the string collection as the type specific collection, please make sure the first element
 in this collection is stands for the title row.
 (将字符串数组转换为数据源对象，注意：请确保第一行为标题行)

|Parameter Name|Remarks|
|--------------|-------|
|strDataLines|-|
|Delimiter|-|
|explicit|-|


#### AsLinq``1
```csharp
Microsoft.VisualBasic.Data.csv.Extensions.AsLinq``1(System.String,System.Boolean)
```
As query source for the LINQ or PLINQ, this function is much save time for the large data set query!

|Parameter Name|Remarks|
|--------------|-------|
|path|-|


#### DataFrame
```csharp
Microsoft.VisualBasic.Data.csv.Extensions.DataFrame(Microsoft.VisualBasic.Data.csv.DocumentStream.File)
```
Create a dynamics data frame object from a csv document object.(从Csv文件之中创建一个数据框容器)

|Parameter Name|Remarks|
|--------------|-------|
|data|-|


#### GetLocusMapName
```csharp
Microsoft.VisualBasic.Data.csv.Extensions.GetLocusMapName(System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|path|Csv file path|


#### LoadCsv``1
```csharp
Microsoft.VisualBasic.Data.csv.Extensions.LoadCsv``1(System.String,System.Boolean,System.Text.Encoding,System.Boolean,System.Collections.Generic.Dictionary{System.String,System.String})
```
Load a csv data file document using a specific object type.(将某一个Csv数据文件加载仅一个特定类型的对象集合中，空文件的话会返回一个空集合，这是一个安全的函数，不会返回空值)

|Parameter Name|Remarks|
|--------------|-------|
|path|The csv document file path.(目标Csv数据文件的文件路径)|
|explicit|-|
|encoding|-|
|maps|``Csv.Field -> @``P:System.Reflection.MemberInfo.Name````|


#### LoadDblVector
```csharp
Microsoft.VisualBasic.Data.csv.Extensions.LoadDblVector(System.String)
```
Load the data from the csv document as a double data type vector.

|Parameter Name|Remarks|
|--------------|-------|
|path|-|


#### LoadStream``1
```csharp
Microsoft.VisualBasic.Data.csv.Extensions.LoadStream``1(System.Collections.Generic.IEnumerable{System.String},System.Boolean,System.Boolean)
```
Load object data set from the text lines stream.(从文本行之中加载数据集)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|explicit|-|


#### SaveTo
```csharp
Microsoft.VisualBasic.Data.csv.Extensions.SaveTo(System.Collections.Generic.IEnumerable{System.Double},System.String,Microsoft.VisualBasic.Text.Encodings)
```
Save the data collection vector as a csv document.

|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|path|-|


#### SaveTo``1
```csharp
Microsoft.VisualBasic.Data.csv.Extensions.SaveTo``1(System.Collections.Generic.IEnumerable{``0},System.String,System.Boolean,System.Text.Encoding,System.String,System.Boolean,System.Collections.Generic.Dictionary{System.String,System.String})
```
Save the object collection data dump into a csv file.(将一个对象数组之中的对象保存至一个Csv文件之中，请注意，这个方法仅仅会保存简单的基本数据类型的属性值)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|path|-|
|explicit|
 If true then all of the simple data type property its value will be save to the data file,
 if not then only save the property with the @``T:Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.ColumnAttribute``
 |
|encoding|-|
|maps|``{meta_define -> custom}``|


#### ToCsvDoc``1
```csharp
Microsoft.VisualBasic.Data.csv.Extensions.ToCsvDoc``1(System.Collections.Generic.IEnumerable{``0},System.Boolean,System.Collections.Generic.Dictionary{System.String,System.String},System.String)
```
Generate a csv document from a object collection.(从一个特定类型的数据集合之中生成一个Csv文件，非并行化的以保持数据原有的顺序)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|explicit|默认导出所有的可用属性|



