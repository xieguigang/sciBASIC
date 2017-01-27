# JsonContract
_namespace: [Microsoft.VisualBasic.Serialization.JSON](./index.md)_

Only works on the Public visible type.
 (使用.NET系统环境之中自带的框架进行JSON序列化和反序列化)



### Methods

#### GetJson``1
```csharp
Microsoft.VisualBasic.Serialization.JSON.JsonContract.GetJson``1(``0,System.Boolean,System.Boolean)
```
Gets the json text value of the target object, the attribute @``T:System.Web.Script.Serialization.ScriptIgnoreAttribute`` 
 can be used for block the property which is will not serialize to the text.
 (使用@``T:System.Web.Script.Serialization.ScriptIgnoreAttribute``来屏蔽掉不想序列化的属性)

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|

> 
>  2016-11-9 对字典进行序列化的时候，假若对象类型是从字典类型继承而来的，则新的附加属性并不会被序列化，只会序列化字典本身
>  

#### GetObjectJson
```csharp
Microsoft.VisualBasic.Serialization.JSON.JsonContract.GetObjectJson(System.Object,System.Type,System.Boolean,System.Boolean)
```
使用@``T:System.Web.Script.Serialization.ScriptIgnoreAttribute``来屏蔽掉不想序列化的属性

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|
|type|-|


#### LoadObject
```csharp
Microsoft.VisualBasic.Serialization.JSON.JsonContract.LoadObject(System.String,System.Type,System.Boolean)
```


|Parameter Name|Remarks|
|--------------|-------|
|json|null -> Nothing|
|type|-|


#### LoadObject``1
```csharp
Microsoft.VisualBasic.Serialization.JSON.JsonContract.LoadObject``1(System.String,System.Boolean)
```
JSON反序列化

#### NamedProperty``1
```csharp
Microsoft.VisualBasic.Serialization.JSON.JsonContract.NamedProperty``1(Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{``0})
```
生成Json之中的动态属性

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### WriteLargeJson``1
```csharp
Microsoft.VisualBasic.Serialization.JSON.JsonContract.WriteLargeJson``1(``0,System.String,System.Boolean)
```
将目标对象保存为json文件

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|
|path|-|



