# EmitReflection
_namespace: [Microsoft.VisualBasic](./index.md)_

Some common used reflection operation extension at here.



### Methods

#### API
```csharp
Microsoft.VisualBasic.EmitReflection.API(System.Type,System.String,System.Boolean)
```


|Parameter Name|Remarks|
|--------------|-------|
|type|-|
|[nameOf]|-|


#### Collection2GenericIEnumerable
```csharp
Microsoft.VisualBasic.EmitReflection.Collection2GenericIEnumerable(System.Type,System.Boolean)
```
Try convert the type specific collection data type into a generic enumerable collection data type.(尝试将目标集合类型转换为通用的枚举集合类型)

|Parameter Name|Remarks|
|--------------|-------|
|Type|The type specific collection data type.(特定类型的集合对象类型，当然也可以是泛型类型)|


_returns: If the target data type is not a collection data type then the original data type will be returns and the function displays a warning message._

#### CreateObject``1
```csharp
Microsoft.VisualBasic.EmitReflection.CreateObject``1(System.Object[],System.Boolean,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|args|构造函数里面的参数信息|


#### Description
```csharp
Microsoft.VisualBasic.EmitReflection.Description(System.Type)
```
如果有@``T:System.ComponentModel.DescriptionAttribute``标记，则会返回该标记的字符串数据，假若没有则只会返回类型的名称

#### Description``1
```csharp
Microsoft.VisualBasic.EmitReflection.Description``1
```
如果有@``T:System.ComponentModel.DescriptionAttribute``标记，则会返回该标记的字符串数据，假若没有则只会返回类型的名称

#### Get``2
```csharp
Microsoft.VisualBasic.EmitReflection.Get``2(System.Collections.Generic.ICollection{``0},System.String,System.Boolean)
```
出错会返回空集合

|Parameter Name|Remarks|
|--------------|-------|
|collection|-|
|Name|使用System.NameOf()操作符来获取|


#### GetAttribute``1
```csharp
Microsoft.VisualBasic.EmitReflection.GetAttribute``1(System.Reflection.PropertyInfo)
```
Get the specific type of custom attribute from a property.
 (从一个属性对象中获取特定的自定义属性对象，找不到的话，就会返回空值)

|Parameter Name|Remarks|
|--------------|-------|
|Property|Target property object.(目标属性对象)|


#### GetDelegateInvokeEntryPoint
```csharp
Microsoft.VisualBasic.EmitReflection.GetDelegateInvokeEntryPoint(System.Object)
```
Get the method reflection entry point for a anonymous lambda expression.(当函数返回Nothing的时候说明目标对象不是一个函数指针)

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### GetFullName
```csharp
Microsoft.VisualBasic.EmitReflection.GetFullName(System.Reflection.MethodBase,System.Boolean)
```
Gets the full name of a method reflection meta data.

|Parameter Name|Remarks|
|--------------|-------|
|method|-|
|IncludeAssembly|-|


#### GetReadWriteProperties
```csharp
Microsoft.VisualBasic.EmitReflection.GetReadWriteProperties(System.Type)
```
Gets all of the can read and write access property from a type define.

|Parameter Name|Remarks|
|--------------|-------|
|type|-|


#### GetTypeElement
```csharp
Microsoft.VisualBasic.EmitReflection.GetTypeElement(System.Type,System.Boolean)
```
得到集合类型的对象之中的元素类型

|Parameter Name|Remarks|
|--------------|-------|
|type|-|
|strict|-|


#### GetValue
```csharp
Microsoft.VisualBasic.EmitReflection.GetValue(System.Type,System.Object,System.String)
```
只对属性有效，出错会返回空值

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|
|Name|-|


#### GetValue``1
```csharp
Microsoft.VisualBasic.EmitReflection.GetValue``1(System.Type,System.Object,System.String)
```
只对属性有效，出错会返回空值

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|
|Name|-|


#### GetVersion
```csharp
Microsoft.VisualBasic.EmitReflection.GetVersion(System.Reflection.Assembly)
```


|Parameter Name|Remarks|
|--------------|-------|
|assm|.NET EXE/DLL assembly|


#### IsInheritsFrom
```csharp
Microsoft.VisualBasic.EmitReflection.IsInheritsFrom(System.Type,System.Type,System.Boolean)
```
Is a inherits from b

|Parameter Name|Remarks|
|--------------|-------|
|a|继承类型继承自基本类型，具备有基本类型的所有特性|
|b|基本类型|
|strict|这个参数是为了解决比较来自不同的assembly文件之中的相同类型的比较，但是这个可能会在类型转换出现一些BUG|

> 假若两个类型是来自于不同的assembly文件的话，即使这两个类型是相同的对象，也会无法判断出来

#### IsNumericType
```csharp
Microsoft.VisualBasic.EmitReflection.IsNumericType(System.Object)
```
Determines whether the specified value is of numeric type.

|Parameter Name|Remarks|
|--------------|-------|
|o|The object to check.|


_returns: 
 true if o is a numeric type; otherwise, false.
 _

#### ModuleVersion
```csharp
Microsoft.VisualBasic.EmitReflection.ModuleVersion(System.Type)
```
Gets the @``T:System.Reflection.AssemblyFileVersionAttribute`` value from the type defined assembly.

|Parameter Name|Remarks|
|--------------|-------|
|type|-|


#### NamespaceEntry
```csharp
Microsoft.VisualBasic.EmitReflection.NamespaceEntry(System.Type)
```
Get the scripting namespace value from @``T:Microsoft.VisualBasic.CommandLine.Reflection.Namespace``

|Parameter Name|Remarks|
|--------------|-------|
|__nsType|-|


#### RunApp
```csharp
Microsoft.VisualBasic.EmitReflection.RunApp(System.String,System.String,System.Boolean)
```
Run external [.NET] Program from RAM Memory

|Parameter Name|Remarks|
|--------------|-------|
|app|-|
|CLI|-|
|cs|Going to running a c# program?|

> 
>  http://www.codeproject.com/Tips/1108105/Run-external-NET-Program-from-RAM-Memory
>  
>  Run external app directly from RAM. You can load the specific file into a ``Byte[]`` Array 
>  with a ``StreamReader()`` or even download it from WEB via a direct link provided. 
>  If you loaded the file from disk, you can delete it if you want after it has been loaded 
>  by a ``StreamReader()``.
>  


