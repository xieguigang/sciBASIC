# DataFramework
_namespace: [Microsoft.VisualBasic.ComponentModel.DataSourceModel](./index.md)_

在目标对象中必须要具有一个属性有自定义属性@``T:Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.DataFrameColumnAttribute``



### Methods

#### __toStringInternal
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework.__toStringInternal(System.Object,System.String)
```
出现错误的时候总是会返回空字符串的

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### CreateObject``1
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework.CreateObject``1(System.Collections.Generic.IEnumerable{``0})
```
Convert target data object collection into a datatable for the data source of the @``T:System.Windows.Forms.DataGridView``>.
 (将目标对象集合转换为一个数据表对象，用作DataGridView控件的数据源)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### GetValue``1
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework.GetValue``1(System.Data.DataTable)
```
Retrive data from a specific datatable object.(从目标数据表中获取数据)

|Parameter Name|Remarks|
|--------------|-------|
|DataTable|-|


#### IsPrimitive
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework.IsPrimitive(System.Type)
```
Is one of the primitive type in the hash @``P:Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework.ToStrings``?

|Parameter Name|Remarks|
|--------------|-------|
|type|-|


#### Schema
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework.Schema(System.Type,Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework.PropertyAccess,System.Reflection.BindingFlags,System.Boolean)
```
请注意：对于VisualBasic的My.Resources.Resources类型而言，里面的属性都是Friend Shared访问类型的，
 所以在解析内部资源管理器对象的时候应该要特别注意**`binds`**参数值的设置，
 因为这个参数默认是@``F:Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework.PublicProperty``

|Parameter Name|Remarks|
|--------------|-------|
|type|-|
|flag|-|
|binds|-|
|nonIndex|@``M:System.Reflection.PropertyInfo.GetIndexParameters`` IsNullOrEmpty|


#### Schema``1
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework.Schema``1(Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework.PropertyAccess,System.Boolean)
```
获取类型之中的属性列表

|Parameter Name|Remarks|
|--------------|-------|
|flag|-|
|nonIndex|@``M:System.Reflection.PropertyInfo.GetIndexParameters`` IsNullOrEmpty|


#### ValueToString
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework.ValueToString(System.Object)
```
Call @``T:System.Object`` of the value types

|Parameter Name|Remarks|
|--------------|-------|
|x|Object should be @``T:System.Data.ValueType``|



### Properties

#### Flags
Controls for @``T:Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework.PropertyAccess`` on @``T:System.Reflection.PropertyInfo``
#### PrimitiveFromString
Converts the .NET primitive types from string.(将字符串数据类型转换为其他的数据类型)
#### PublicProperty
(instance) ``Public Property xxxxx As xxxxx``
#### PublicShared
(statics) ``Public Shared Property xxxx As xxxx``
#### ToStrings
Object @``T:System.Object`` methods.
