# SetValue`1
_namespace: [Microsoft.VisualBasic.Linq](./index.md)_

Set value linq expression helper



### Methods

#### GetSet
```csharp
Microsoft.VisualBasic.Linq.SetValue`1.GetSet(System.String)
```
Public Delegate Function IInvokeSetValue(x As T, value As Object) As T

|Parameter Name|Remarks|
|--------------|-------|
|name|Using NameOf|


#### InvokeSet``1
```csharp
Microsoft.VisualBasic.Linq.SetValue`1.InvokeSet``1(`0@,System.String,``0)
```
Assigning the value to the specific named property to the target object.
 (将**`value`**参数之中的值赋值给目标对象**`obj`**之中的指定的**`name`**属性名称的属性，
 如果发生错误，则原有的对象**`obj`**不会被修改)

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|
|Name|可以使用NameOf得到需要进行修改的属性名称|
|value|-|


#### InvokeSetValue
```csharp
Microsoft.VisualBasic.Linq.SetValue`1.InvokeSetValue(`0,System.String,System.Object)
```
Assigning the value to the specific named property to the target object.
 (将**`value`**参数之中的值赋值给目标对象**`x`**之中的指定的**`name`**属性名称的属性，
 如果发生错误，则原有的对象**`x`**不会被修改)

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|name|Using NameOf.(可以使用NameOf得到需要进行修改的属性名称)|
|value|-|


#### op_LessThanOrEqual
```csharp
Microsoft.VisualBasic.Linq.SetValue`1.op_LessThanOrEqual(Microsoft.VisualBasic.Linq.SetValue{`0},System.String)
```
@``M:Microsoft.VisualBasic.Linq.SetValue`1.GetSet(System.String)``

|Parameter Name|Remarks|
|--------------|-------|
|setValue|-|
|name|Using NameOf|



