# BindProperty`1
_namespace: [Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps](./index.md)_

Schema for @``T:System.Attribute`` and its bind @``T:System.Reflection.PropertyInfo`` object target



### Methods

#### GetValue
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.BindProperty`1.GetValue(System.Object)
```
Returns the property value of a specified object with optional index values for
 indexed properties.

|Parameter Name|Remarks|
|--------------|-------|
|x|The object whose property value will be returned.|


_returns: The property value of the specified object._

#### SetValue
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.BindProperty`1.SetValue(System.Object,System.Object)
```
Sets the property value of a specified object with optional index values for
 index properties.

|Parameter Name|Remarks|
|--------------|-------|
|obj|The object whose property value will be set.|
|value|The new property value.|


#### ToString
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.BindProperty`1.ToString
```
Display this schema maps in Visualbasic style.


### Properties

#### Field
The flag for this field binding.
#### Identity
The map name or the @``P:System.Reflection.MemberInfo.Name``
#### IsNull
Is this map data is null on its attribute or property data?
#### IsPrimitive
Gets a value indicating whether the @``T:System.Type`` is one of the primitive types.
#### Property
The property object that bind with its custom attribute @``F:Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.BindProperty`1.Field`` of type **`T`**
#### Type
Gets the type of this property.
