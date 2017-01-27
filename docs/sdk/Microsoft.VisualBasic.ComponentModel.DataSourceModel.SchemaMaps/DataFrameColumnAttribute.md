# DataFrameColumnAttribute
_namespace: [Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps](./index.md)_

Represents a column of certain data frames. The mapping between to schema is also can be represent by this attribute. 
 (也可以使用这个对象来完成在两个数据源之间的属性的映射，由于对于一些列名称的属性值缺失的映射而言，
 其是使用属性名来作为列映射名称的，故而在修改这些没有预设的列名称的映射属性的属性名的时候，请注意
 要小心维护这种映射关系)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.DataFrameColumnAttribute.#ctor(System.String,System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|Name|列名称，假若本参数为空的话，则使用属性名称|
|index|从1开始的下标，表示为第几列|


#### LoadMapping
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.DataFrameColumnAttribute.LoadMapping(System.Type,System.String[],System.Boolean)
```
Load the mapping property, if the custom attribute @``T:Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.DataFrameColumnAttribute`` 
 have no name value, then the property name will be used as the mapping name.
 (这个函数会自动给空名称值进行属性名的赋值操作的)

|Parameter Name|Remarks|
|--------------|-------|
|typeInfo|The type should be a class type or its properties should have the 
 mapping option which was created by the custom attribute @``T:Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.DataFrameColumnAttribute``
 |
|ignores|这个是大小写敏感的|


#### LoadMapping``1
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.DataFrameColumnAttribute.LoadMapping``1(System.String[],System.Boolean)
```
没有名称属性的映射使用属性名来表述，请注意，字典的Key是属性的名称


### Properties

#### Index
Gets the index.
#### Name
Gets the name.
