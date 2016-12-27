# RDFType
_namespace: [Microsoft.VisualBasic.MIME.RDF.Serialization](./index.md)_

在申明RDF对象的时候所申明的Schema中的目标类型



### Methods

#### CreateTypeDefine
```csharp
Microsoft.VisualBasic.MIME.RDF.Serialization.RDFType.CreateTypeDefine(System.Type)
```
当目标类型不存在RDFType自定义属性的时候，进行创建的方法

|Parameter Name|Remarks|
|--------------|-------|
|TypeInfo|-|



### Properties

#### _BindElementTypeInfo
假若目标类型为一个数组类型，则本属性则为目标数组的元素的类型，但是不是的话，则本属性为空值
#### _BindTypeInfo
自己本身的类型属性
