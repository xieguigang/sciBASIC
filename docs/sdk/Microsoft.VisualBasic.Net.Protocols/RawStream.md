# RawStream
_namespace: [Microsoft.VisualBasic.Net.Protocols](./index.md)_

原始串流的基本模型，这个流对象应该具备有两个基本的方法：
 1. 从原始的字节流之中反序列化构造出自身的构造函数
 2. 将自身序列化为字节流的@``M:Microsoft.VisualBasic.Net.Protocols.ISerializable.Serialize``序列化方法



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.Protocols.RawStream.#ctor(System.Byte[])
```
You should overrides this constructor to generate a stream object.(必须要有一个这个构造函数来执行反序列化)

|Parameter Name|Remarks|
|--------------|-------|
|rawStream|-|


#### GetRawStream``1
```csharp
Microsoft.VisualBasic.Net.Protocols.RawStream.GetRawStream``1(System.Byte[])
```
按照类型的定义进行反序列化操作

|Parameter Name|Remarks|
|--------------|-------|
|rawStream|-|


#### op_LessThanOrEqual
```csharp
Microsoft.VisualBasic.Net.Protocols.RawStream.op_LessThanOrEqual(System.String,Microsoft.VisualBasic.Net.Protocols.RawStream)
```


|Parameter Name|Remarks|
|--------------|-------|
|addr|IPEndPoint string value likes 127.0.0.1:8080|
|raw|-|


#### Serialize
```csharp
Microsoft.VisualBasic.Net.Protocols.RawStream.Serialize
```
@``M:Microsoft.VisualBasic.Net.Protocols.ISerializable.Serialize``序列化方法


### Properties

#### DblFloat
System.Double
#### INT32
Single/Integer
