# InputHandler
_namespace: [Microsoft.VisualBasic.Scripting](./index.md)_

转换从终端或者脚本文件之中输入的字符串的类型的转换



### Methods

#### CapabilityPromise
```csharp
Microsoft.VisualBasic.Scripting.InputHandler.CapabilityPromise(System.String,System.Type,System.Func{System.String,System.Object})
```
Dynamics updates the capability of function @``M:Microsoft.VisualBasic.Scripting.InputHandler.CTypeDynamic(System.String,System.Type)``, 
 @``M:Microsoft.VisualBasic.Scripting.InputHandler.CTypeDynamic``1(System.String)`` and 
 @``M:Microsoft.VisualBasic.Scripting.InputHandler.IsPrimitive(System.Type)``

|Parameter Name|Remarks|
|--------------|-------|
|briefName|-|
|stringConvertType|-|
|cast|-|


#### CastArray``1
```csharp
Microsoft.VisualBasic.Scripting.InputHandler.CastArray``1(System.Object)
```
The parameter **`obj`** should implements a @``T:System.Collections.IEnumerable`` interface on the type. and then DirectCast object to target type.

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### Convertible
```csharp
Microsoft.VisualBasic.Scripting.InputHandler.Convertible(System.Type,System.Type)
```
Does the **`inputtype`** type can be cast to type **`DefType`**.(主要为了方便减少脚本编程模块的代码)

|Parameter Name|Remarks|
|--------------|-------|
|inputType|-|
|DefType|-|


#### CTypeDynamic
```csharp
Microsoft.VisualBasic.Scripting.InputHandler.CTypeDynamic(System.String,System.Type)
```
Converts a string expression which was input from the console or script file to the specified type.
 (请注意，函数只是转换最基本的数据类型，转换错误会返回空值)

|Parameter Name|Remarks|
|--------------|-------|
|expression|The string expression to convert.|
|target|The type to which to convert the object.|


_returns: An object whose type at run time is the requested target type._

#### CTypeDynamic``1
```csharp
Microsoft.VisualBasic.Scripting.InputHandler.CTypeDynamic``1(System.String)
```
Converts a string expression which was input from the console or script file to the specified type.
 (请注意，函数只是转换最基本的数据类型，转换错误会返回空值)

|Parameter Name|Remarks|
|--------------|-------|
|Expression|The string expression to convert.|


_returns: An object whose type at run time is the requested target type._

#### DirectCast
```csharp
Microsoft.VisualBasic.Scripting.InputHandler.DirectCast(System.Object[],System.Type)
```


|Parameter Name|Remarks|
|--------------|-------|
|array|-|
|type|数组里面的元素的类型|


#### GetType
```csharp
Microsoft.VisualBasic.Scripting.InputHandler.GetType(System.String,System.Boolean)
```
类型获取失败会返回空值，大小写不敏感

|Parameter Name|Remarks|
|--------------|-------|
|name|类型的名称简写|
|ObjectGeneric|是否出错的时候返回Object类型，默认返回Nothing|


#### IsPrimitive
```csharp
Microsoft.VisualBasic.Scripting.InputHandler.IsPrimitive(System.Type)
```
Does this type can be cast from the @``T:System.String`` type?(目标类型能否由字符串转换过来??)

|Parameter Name|Remarks|
|--------------|-------|
|targetType|-|


#### ToString
```csharp
Microsoft.VisualBasic.Scripting.InputHandler.ToString(System.Object,System.String)
```
@``M:Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework.__toStringInternal(System.Object,System.String)``, 出现错误的时候总是会返回空字符串的

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|



### Properties

#### CasterString
Object为字符串类型，这个字典可以讲字符串转为目标类型
#### String
@``T:System.Type`` information for @``T:System.String`` type from GetType operator
#### Types
Enumerate all of the types that can be handled in this module. All of the key string is in lower case.(键值都是小写的)
