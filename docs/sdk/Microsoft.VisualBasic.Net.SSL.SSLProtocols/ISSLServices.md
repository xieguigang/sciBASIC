# ISSLServices
_namespace: [Microsoft.VisualBasic.Net.SSL.SSLProtocols](./index.md)_

抽象SSL服务器



### Methods

#### Install
```csharp
Microsoft.VisualBasic.Net.SSL.SSLProtocols.ISSLServices.Install(Microsoft.VisualBasic.Net.SSL.Certificate,System.Boolean,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|CA|-|
|[overrides]|当证书的哈希值有冲突的时候，新安装的证书**`ca`**可不可以将旧的证书覆盖掉|



### Properties

#### CA
公共密匙
#### InstallCertificates
告诉SSL层如何安装数字证书
#### PrivateKeys
客户端的私有密匙
#### RaiseHandshakingEvent
有新的客户端请求进行连接
#### RefuseHandshake
对于某些应用出于安全性的考虑，会将这里设置为False，则服务器就会全部拒绝后面的所有的握手请求，只接受来自于从外部导入的用户证书的数据请求
#### ResponseHandler
处理私有密匙的数据请求
