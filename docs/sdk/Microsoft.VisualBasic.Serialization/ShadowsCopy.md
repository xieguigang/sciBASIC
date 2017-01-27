# ShadowsCopy
_namespace: [Microsoft.VisualBasic.Serialization](./index.md)_





### Methods

#### __shadowsCopy
```csharp
Microsoft.VisualBasic.Serialization.ShadowsCopy.__shadowsCopy(System.Type,System.Object)
```
递归使用的，基本数据类型直接复制，引用类型则首先创建一个新的对象，在对该对象进行递归复制，假若目标对象没有可用的无参数的构造函数，则直接赋值

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### ShadowCopy``1
```csharp
Microsoft.VisualBasic.Serialization.ShadowsCopy.ShadowCopy``1(``0,``0@)
```
请使用这个函数来对CSV序列化的对象进行浅拷贝。将**`source`**之中的第一层的属性值拷贝到**`target`**对应的属性值之中，然后返回**`target`**

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|Target|-|


#### ShadowCopy``2
```csharp
Microsoft.VisualBasic.Serialization.ShadowsCopy.ShadowCopy``2(``0)
```
将第一层的属性值从基本类复制给继承类

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### ShadowsCopy
```csharp
Microsoft.VisualBasic.Serialization.ShadowsCopy.ShadowsCopy(System.Object)
```
将目标对象之中的属性按值复制

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|

> 对外函数接口，为了防止无限递归的出现

#### ShadowsCopy``1
```csharp
Microsoft.VisualBasic.Serialization.ShadowsCopy.ShadowsCopy``1(``0)
```
将目标对象之中的属性按值复制

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|

> 对外函数接口，为了防止无限递归的出现


