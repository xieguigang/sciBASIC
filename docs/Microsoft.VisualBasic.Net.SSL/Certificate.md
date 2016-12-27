# Certificate
_namespace: [Microsoft.VisualBasic.Net.SSL](./index.md)_

应用程序的完整性验证和用户身份的验证



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.SSL.Certificate.#ctor(System.String)
```
这个构造函数不再计算哈希值而是直接初始化

|Parameter Name|Remarks|
|--------------|-------|
|hash|必须是md5哈希值|


#### Decrypt
```csharp
Microsoft.VisualBasic.Net.SSL.Certificate.Decrypt(Microsoft.VisualBasic.Net.Protocols.RequestStream)
```
@``M:Microsoft.VisualBasic.Net.SSL.Certificate.Decrypt(System.Byte[])`` @``P:Microsoft.VisualBasic.Net.Protocols.RequestStream.ChunkBuffer``

|Parameter Name|Remarks|
|--------------|-------|
|request|-|


#### Encrypt
```csharp
Microsoft.VisualBasic.Net.SSL.Certificate.Encrypt(Microsoft.VisualBasic.Net.Protocols.RequestStream)
```
函数会根据uid的值来设定协议为私有密匙还是公共密匙

|Parameter Name|Remarks|
|--------------|-------|
|request|-|


#### Install
```csharp
Microsoft.VisualBasic.Net.SSL.Certificate.Install(System.String,System.Boolean)
```


|Parameter Name|Remarks|
|--------------|-------|
|App|可执行程序的文件路径|


#### PublicEncrypt
```csharp
Microsoft.VisualBasic.Net.SSL.Certificate.PublicEncrypt(Microsoft.VisualBasic.Net.Protocols.RequestStream)
```
强制将协议设定为公共密匙加密

|Parameter Name|Remarks|
|--------------|-------|
|request|-|



### Properties

#### _uid
初始化继承类所需要的
#### AppDomain
检查应用程序的完整性
#### hash
与@``P:Microsoft.VisualBasic.Net.SSL.Certificate.uid``属性所不同的是，这个属性是@``P:Microsoft.VisualBasic.Net.SSL.Certificate.PrivateKey``的哈希值，
 通常这个哈希值在请求resultful WebAPI的时候用来作为用户的唯一标识
#### PrivateKey
私有密匙
#### uid
@``T:System.Guid``计算出来的哈希值只能为负数，现在约定，当这个属性为0的时候就认为这个证书是公共密匙，
 这个一般是使用用户的账号所计算出来的哈希值
