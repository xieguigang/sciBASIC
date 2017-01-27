# DynamicObjectLoader
_namespace: [Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels](./index.md)_

Data structure for high perfermence data loading.



### Methods

#### GetDataTypeName
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.DynamicObjectLoader.GetDataTypeName(System.Int32)
```
Gets the data type information for the specified field.

|Parameter Name|Remarks|
|--------------|-------|
|i|-|


#### GetKey
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.DynamicObjectLoader.GetKey(System.String)
```
将大小写敏感转换为大小写不敏感

|Parameter Name|Remarks|
|--------------|-------|
|key|-|


#### GetName
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.DynamicObjectLoader.GetName(System.Int32)
```
Gets the name for the field to find.

|Parameter Name|Remarks|
|--------------|-------|
|i|-|


#### TryCast``1
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.DynamicObjectLoader.TryCast``1
```
函数会尝试将目标对象的属性值按照名称进行赋值，前提是目标属性值的类型应该为基本的类型。假若类型转换不成功，则会返回空对象

#### TryGetMember
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.DynamicObjectLoader.TryGetMember(System.Dynamic.GetMemberBinder,System.Object@)
```
Provides the implementation for operations that get member values. Classes derived
 from the System.Dynamic.DynamicObject class can override this method to specify
 dynamic behavior for operations such as getting a value for a property.

|Parameter Name|Remarks|
|--------------|-------|
|binder|
 Provides information about the object that called the dynamic operation. The
 binder.Name property provides the name of the member on which the dynamic operation
 is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty)
 statement, where sampleObject is an instance of the class derived from the System.Dynamic.DynamicObject
 class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies
 whether the member name is case-sensitive.
 |
|result|The result of the get operation. For example, if the method is called for a property, you can assign the property value to result.|


_returns: 
 true if the operation is successful; otherwise, false. If this method returns
 false, the run-time binder of the language determines the behavior. (In most
 cases, a run-time exception is thrown.)
 _

#### TrySetMember
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.DynamicObjectLoader.TrySetMember(System.Dynamic.SetMemberBinder,System.Object)
```
Provides the implementation for operations that set member values. Classes derived
 from the System.Dynamic.DynamicObject class can override this method to specify
 dynamic behavior for operations such as setting a value for a property.

|Parameter Name|Remarks|
|--------------|-------|
|binder|
 Provides information about the object that called the dynamic operation. The
 binder.Name property provides the name of the member to which the value is being
 assigned. For example, for the statement sampleObject.SampleProperty = "Test",
 where sampleObject is an instance of the class derived from the System.Dynamic.DynamicObject
 class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies
 whether the member name is case-sensitive.
 |
|value|The value to set to the member. For example, for sampleObject.SampleProperty
 = "Test", where sampleObject is an instance of the class derived from the System.Dynamic.DynamicObject
 class, the value is "Test".|


_returns: 
 true if the operation is successful; otherwise, false. If this method returns
 false, the run-time binder of the language determines the behavior. (In most
 cases, a language-specific run-time exception is thrown.)
 _


### Properties

#### Attribute
Get or set the string value in the specific attribute name of current line.
#### FieldCount
Gets the number of columns in the current row.
