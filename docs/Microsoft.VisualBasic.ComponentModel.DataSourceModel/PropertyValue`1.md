# PropertyValue`1
_namespace: [Microsoft.VisualBasic.ComponentModel.DataSourceModel](./index.md)_

The @``T:System.Reflection.PropertyInfo`` like definition of the extension property.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.PropertyValue`1.#ctor
```
Tag property value.(默认是将数据写入到基本类型的值之中)

#### New``1
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.PropertyValue`1.New``1(``0,System.String)
```
Creates a new extension property for the target @``T:Microsoft.VisualBasic.Language.ClassObject``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|name|-|


#### op_Explicit
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.PropertyValue`1.op_Explicit(Microsoft.VisualBasic.ComponentModel.DataSourceModel.PropertyValue{`0})~`0
```
Property Get Value

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### Read``1
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.PropertyValue`1.Read``1(``0,System.String)
```
Gets the tag property value from the @``T:Microsoft.VisualBasic.Language.ClassObject``.(读取@``T:Microsoft.VisualBasic.Language.ClassObject``对象之中的一个拓展属性)

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|name|-|


#### SetValue
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.PropertyValue`1.SetValue(`0)
```
这个主要是应用于Linq表达式之中，将属性值设置之后返回宿主对象实例

|Parameter Name|Remarks|
|--------------|-------|
|value|-|


#### ToString
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.PropertyValue`1.ToString
```
@``P:Microsoft.VisualBasic.ComponentModel.DataSourceModel.PropertyValue`1.value`` -> @``M:Microsoft.VisualBasic.Serialization.JSON.JsonContract.GetObjectJson(System.Object,System.Type,System.Boolean,System.Boolean)``


### Properties

#### obj
The instance object for this extension property
#### value
The Extension property value.
