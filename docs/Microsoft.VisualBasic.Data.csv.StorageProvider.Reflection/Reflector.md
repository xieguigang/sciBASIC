# Reflector
_namespace: [Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection](./index.md)_

The dynamics reflection operations on Csv data source services.



### Methods

#### __save
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.Reflector.__save(System.Collections.IEnumerable,System.Type,System.Boolean,System.Collections.Generic.Dictionary{System.String,System.Type},System.String,System.Collections.Generic.Dictionary{System.String,System.String},System.Boolean)
```
Save the specifc type object collection into the csv data file.(将目标对象数据的集合转换为Csv文件已进行数据保存操作)

|Parameter Name|Remarks|
|--------------|-------|
|___source|-|
|explicit|-|
|schemaOut|请注意，Key是Csv文件之中的标题，不是属性名称了|

> 查找所有具备读属性的属性值

#### Convert``1
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.Reflector.Convert``1(Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame,System.Boolean)
```
从文件之中读取数据并转换为对象数据

|Parameter Name|Remarks|
|--------------|-------|
|DataFrame|-|
|explicit|-|

> 在这里查找所有具有写属性的属性对象即可

#### ExportAsPropertyAttributes``1
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.Reflector.ExportAsPropertyAttributes``1(System.Collections.Generic.IEnumerable{``0},System.Boolean)
```
将数据集合导出为键值对，以方便其他操作

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|Explicit|-|


#### GetDataFrameworkTypeSchema
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.Reflector.GetDataFrameworkTypeSchema(System.Type,System.Boolean)
```
Returns the type schema as ``{columnName, type}``, using for the cytoscape software

|Parameter Name|Remarks|
|--------------|-------|
|type|-|
|Explicit|-|


#### GetsRowData
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.Reflector.GetsRowData(System.Collections.Generic.IEnumerable{System.Object},System.Type,System.Boolean,System.Collections.Generic.Dictionary{System.String,System.String},System.Boolean,System.String)
```
Save the specifc type object collection into the csv data file.(将目标对象数据的集合转换为Csv文件已进行数据保存操作)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|Explicit|-|

> 查找所有具备读属性的属性值

#### Load``1
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.Reflector.Load``1(System.String,System.Boolean,System.Text.Encoding,System.Boolean,System.Collections.Generic.Dictionary{System.String,System.String})
```
Method for load a csv data file into a specific type of object collection.

|Parameter Name|Remarks|
|--------------|-------|
|Explicit|当本参数值为False的时候，所有的简单属性值都将被解析出来，而忽略掉其是否带有@``T:Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.ColumnAttribute``自定义属性|
|path|-|


#### LoadDataToObject
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.Reflector.LoadDataToObject(Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame,System.Type,System.Boolean)
```
将Csv文件加载至一个目标集合之中以完成数据从文件之中的读取操作

|Parameter Name|Remarks|
|--------------|-------|
|csv|-|
|type|-|
|explicit|-|


#### Save``1
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.Reflector.Save``1(System.Collections.Generic.IEnumerable{``0},System.Boolean,System.String,System.Collections.Generic.Dictionary{System.String,System.String},System.Boolean,System.Collections.Generic.Dictionary{System.String,System.Type}@)
```
Save the specifc type object collection into the csv data file.
 (将目标对象数据的集合转换为Csv文件已进行数据保存操作，非并行化的以保持数据原有的顺序)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|explicit|-|
|schemaOut|``ByRef``反向输出的Schema参数|

> 查找所有具备读属性的属性值


