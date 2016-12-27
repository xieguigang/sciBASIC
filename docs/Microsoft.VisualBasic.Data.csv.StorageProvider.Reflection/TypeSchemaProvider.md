# TypeSchemaProvider
_namespace: [Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection](./index.md)_





### Methods

#### __generateMask
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.TypeSchemaProvider.__generateMask(System.Reflection.PropertyInfo,System.String,System.Boolean,Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.ColumnAttribute)
```
这个函数是针对没有任何自定义属性标记的属性对象而言的

|Parameter Name|Remarks|
|--------------|-------|
|[Property]|-|
|[alias]|-|


#### GetInterfaces
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.TypeSchemaProvider.GetInterfaces(System.Reflection.PropertyInfo,System.Boolean,System.Boolean)
```
当目标属性上面没有任何自定义属性数据的时候，会检查是否为简单数据类型，假若是则会自动添加一个NullMask，
 假若不是，则会返回空集合，则说明这个属性不会被用于序列化和反序列化。
 假若返回来的是空值，则说明是复杂类型

|Parameter Name|Remarks|
|--------------|-------|
|[Property]|对于LINQ的Column属性也会接受的|


#### GetProperties
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.TypeSchemaProvider.GetProperties(System.Type,System.Boolean)
```
返回的字典对象之中的Value部分是自定义属性

#### GetThisElement
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.TypeSchemaProvider.GetThisElement(System.Type,System.Boolean)
```
获取集合类型的元素类型，假若获取不到，则会返回类型@``T:System.Void``

|Parameter Name|Remarks|
|--------------|-------|
|type|-|
|forcePrimitive|当本参数为False的时候，假若不是集合类型，函数会返回Nothing|


#### IsKeyValuePair
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.TypeSchemaProvider.IsKeyValuePair(System.Reflection.PropertyInfo)
```
这个属性的类型可以同时允许系统的内建的键值对类型，也可以是@``F:Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.TypeSchemaProvider.KeyValuePairObject``

|Parameter Name|Remarks|
|--------------|-------|
|Property|-|



