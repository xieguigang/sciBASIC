# SchemaProvider
_namespace: [Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels](./index.md)_

从目标对象解析出来的Csv文件的结构组织数据



### Methods

#### ContainsField
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.SchemaProvider.ContainsField(System.String)
```
从域名称来判断

|Parameter Name|Remarks|
|--------------|-------|
|Name|-|


#### ContainsProperty
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.SchemaProvider.ContainsProperty(System.Reflection.PropertyInfo)
```
从所绑定的属性来判断

|Parameter Name|Remarks|
|--------------|-------|
|[Property]|-|

> 这个函数还需要进行一些绑定的映射

#### CopyReadDataFromObject
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.SchemaProvider.CopyReadDataFromObject
```
For write csv data file.(从目标类型对象之中可以读取这个属性的值将数据写入到文件之中)

#### CopyWriteDataToObject
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.SchemaProvider.CopyWriteDataToObject
```
For create object instance.(可以在读取Csv文件之中的数据之后将数据写入到这个属性之中从而将数据加载进入内存之中)

#### CreateObject
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.SchemaProvider.CreateObject(System.Type,System.Boolean)
```
Creates the data frame schema for the specific object type.

|Parameter Name|Remarks|
|--------------|-------|
|type|-|
|strict|-|


#### CreateObject``1
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.SchemaProvider.CreateObject``1(System.Boolean)
```
``CreateObject(GetType(T), Explicit)``

|Parameter Name|Remarks|
|--------------|-------|
|Explicit|-|


#### GetField
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.SchemaProvider.GetField(System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|Name|支持属性名称或者域名称|


#### GetMetaAttributeColumn
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.SchemaProvider.GetMetaAttributeColumn(System.Collections.Generic.Dictionary{System.Reflection.PropertyInfo,Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.StorageProvider},System.Boolean)
```
对于@``T:Microsoft.VisualBasic.ComponentModel.DataSourceModel.DynamicPropertyBase`1``的继承对象类型，也会自动解析出来的，假若@``T:Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.MetaAttribute``没有被定义的话

|Parameter Name|Remarks|
|--------------|-------|
|Properties|-|



### Properties

#### CollectionColumns
基本数据类型的数组形式的列
#### Columns
基本数据类型的列
#### MetaAttributes
一个类型之中只可以定义一个元数据存储对象
#### Raw
提供当前的schema数据的原始数据
