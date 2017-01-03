# CodeDOMExpressions
_namespace: [Microsoft.VisualBasic.Emit.CodeDOM_VBC](./index.md)_





### Methods

#### Call
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.Call(System.Type,System.String,System.Object[])
```
Call a statics function from a specific type with a known function name

|Parameter Name|Remarks|
|--------------|-------|
|type|-|
|Name|-|
|parametersValue|-|


#### CType
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.CType(System.CodeDom.CodeExpression,System.Type)
```
Ctype

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|
|type|-|


#### DeclareFunc
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.DeclareFunc(System.String,System.Collections.Generic.Dictionary{System.String,System.Type},System.Type,System.CodeDom.MemberAttributes)
```
声明一个函数

|Parameter Name|Remarks|
|--------------|-------|
|name|-|
|args|-|
|returns|-|
|control|-|


#### Field
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.Field(System.String,System.Type)
```
```
 Dim Name As <Type>
 ```
 
 Declare a field in the type

|Parameter Name|Remarks|
|--------------|-------|
|Name|-|
|Type|-|


#### FieldRef
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.FieldRef(System.String)
```
Reference of ``Me.Field``

|Parameter Name|Remarks|
|--------------|-------|
|Name|-|


#### GetType
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.GetType(System.Type)
```
System.Type.GetType(TypeName)

|Parameter Name|Remarks|
|--------------|-------|
|Type|-|


#### GetValue
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.GetValue(System.CodeDom.CodeExpression,System.Int32)
```
Gets the element value in a array object.

|Parameter Name|Remarks|
|--------------|-------|
|Array|-|
|index|-|


#### LocalsInit
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.LocalsInit(System.String,System.Type,System.Object)
```
Declare a local variable.

|Parameter Name|Remarks|
|--------------|-------|
|Name|-|
|Type|-|
|init|-|


#### LocalVariable
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.LocalVariable(System.String)
```
Reference to a local variable in a function body.(引用局部变量)

|Parameter Name|Remarks|
|--------------|-------|
|Name|-|


#### New
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.New(System.String,System.CodeDom.CodeExpression[])
```
New object

|Parameter Name|Remarks|
|--------------|-------|
|typeRef|-|
|parameters|-|


#### New``1
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.New``1(System.Object[])
```
Class object instance constructor.

|Parameter Name|Remarks|
|--------------|-------|
|parameters|-|


#### Reference
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.Reference(System.CodeDom.CodeExpression,System.String)
```
Reference to a instance field in the specific object instance.

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|
|Name|-|


#### Return
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.Return(System.CodeDom.CodeExpression)
```
Returns value in a function body

|Parameter Name|Remarks|
|--------------|-------|
|expression|-|


#### Value
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.Value(System.Object)
```
Variable value initializer

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### ValueAssign
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExpressions.ValueAssign(System.CodeDom.CodeExpression,System.CodeDom.CodeExpression)
```
``left = value``

|Parameter Name|Remarks|
|--------------|-------|
|LeftAssigned|-|
|value|-|



### Properties

#### EntryPoint
Public Shared Function Main(Argvs As String()) As Integer
#### PublicShared
```
 Public Shared Function xxx() As T
 Public Shared Property XXX As T
 ```
 
 Or declare a method in a standard Module type.
