# TypeInfo
_namespace: [Microsoft.VisualBasic.Scripting.MetaData](./index.md)_

The type reference information.(类型信息)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Scripting.MetaData.TypeInfo.#ctor(System.Type)
```
Creates type reference from the definition.

|Parameter Name|Remarks|
|--------------|-------|
|info|-|


#### GetType
```csharp
Microsoft.VisualBasic.Scripting.MetaData.TypeInfo.GetType(System.Boolean)
```
Get mapping type information.

#### LoadAssembly
```csharp
Microsoft.VisualBasic.Scripting.MetaData.TypeInfo.LoadAssembly(System.String)
```
Loads the assembly file which contains this type. If the is not a valid directory location, 
 then using the location @``P:Microsoft.VisualBasic.App.HOME`` as default.

#### op_Equality
```csharp
Microsoft.VisualBasic.Scripting.MetaData.TypeInfo.op_Equality(Microsoft.VisualBasic.Scripting.MetaData.TypeInfo,System.Type)
```
检查a是否是指向b的类型引用的

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|b|-|



### Properties

#### assm
The assembly file which contains this type definition.(模块文件)
#### FullIdentity
@``P:System.Type.FullName``.(类型源)
#### SystemKnownType
Is this type object is a known system type?(是否是已知的类型？)
