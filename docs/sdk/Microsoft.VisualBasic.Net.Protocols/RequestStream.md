# RequestStream
_namespace: [Microsoft.VisualBasic.Net.Protocols](./index.md)_

Socket user client => Socket server data request &
 Socket server => Socket user client data response package.
 (Socket客户端 => Socket服务器所发送的数据请求以及从
 Socket服务器 => Socket客户端所返回数据的数据响应包)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.Protocols.RequestStream.#ctor(System.Byte[])
```
Deserialize (当还有剩余数据的时候会将数据进行剪裁)

|Parameter Name|Remarks|
|--------------|-------|
|rawStream|-|


#### CreatePackage
```csharp
Microsoft.VisualBasic.Net.Protocols.RequestStream.CreatePackage(System.Byte[])
```
服务器端返回数据所使用的，所有的标签为@``F:Microsoft.VisualBasic.Net.Http.HTTP_RFC.RFC_OK``

|Parameter Name|Remarks|
|--------------|-------|
|pack|-|


#### CreatePackage``1
```csharp
Microsoft.VisualBasic.Net.Protocols.RequestStream.CreatePackage``1(``0)
```
服务器端返回数据所使用的，默认使用json序列化，所有的标签为@``F:Microsoft.VisualBasic.Net.Http.HTTP_RFC.RFC_OK``

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### CreateProtocol``1
```csharp
Microsoft.VisualBasic.Net.Protocols.RequestStream.CreateProtocol``1(System.Int64,System.Int64,``0)
```
这个函数是使用json序列化参数信息的

|Parameter Name|Remarks|
|--------------|-------|
|cat|-|
|protocol|-|
|params|-|


#### GetRawStream``1
```csharp
Microsoft.VisualBasic.Net.Protocols.RequestStream.GetRawStream``1
```
从原始数据流@``P:Microsoft.VisualBasic.Net.Protocols.RequestStream.ChunkBuffer``之中进行反序列化得到一个嵌套的数据串流对象

#### GetUTF8String
```csharp
Microsoft.VisualBasic.Net.Protocols.RequestStream.GetUTF8String
```
默认是使用UTF8编码来编码字符串的

#### LoadObject``1
```csharp
Microsoft.VisualBasic.Net.Protocols.RequestStream.LoadObject``1
```
json

#### op_Equality
```csharp
Microsoft.VisualBasic.Net.Protocols.RequestStream.op_Equality(Microsoft.VisualBasic.Net.Protocols.RequestStream,Microsoft.VisualBasic.Net.Protocols.RequestStream)
```
简单的字符串等价

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|b|-|


#### Serialize
```csharp
Microsoft.VisualBasic.Net.Protocols.RequestStream.Serialize
```
执行序列化进行网络之间的数据传输


### Properties

#### BufferLength
Buffer length of the protocol request raw stream data @``P:Microsoft.VisualBasic.Net.Protocols.RequestStream.ChunkBuffer``.(协议数据的长度)
#### ChunkBuffer
The raw stream data of the details data request or the server response data.(协议的具体数据请求)
#### FullRead
@``P:Microsoft.VisualBasic.Net.Protocols.RequestStream.ChunkBuffer``部分的数据是否完整？
#### IsPlantText
这个数据仅仅是一个文本，没有包含有任何协议头数据
#### IsSSL_PublicToken
使用公共密匙
#### IsSSLHandshaking
这个请求数据是否为握手协议
#### IsSSLProtocol
这个请求数据是一个SSL加密数据（使用用户的私有密匙）
#### Protocol
This property indicates which the specifics protocol processor will be used for the incoming client request.
 (协议的头部)
#### ProtocolCategory
This property indicates the protocol processor module for the server object.
#### SYS_PROTOCOL
系统里面最基本的基本数据协议
#### uid
使用用户的账号信息唯一标识出来的对象，在服务器端用来查找用户证书的
 由于在服务器上面这个哈希值是和解密的密匙唯一对应的，所以服务器上面大多数情况下是直接通过这个哈希值来决定授权的
