# SSLProtocols
_namespace: [Microsoft.VisualBasic.Net.SSL](./index.md)_





### Methods

#### __sslHandshake
```csharp
Microsoft.VisualBasic.Net.SSL.SSLProtocols.__sslHandshake(System.Int64,Microsoft.VisualBasic.Net.SSL.SSLProtocols.ISSLServices,Microsoft.VisualBasic.Net.Protocols.RequestStream,System.Net.IPEndPoint,Microsoft.VisualBasic.Net.SSL.SSLProtocols.InstallCertificates)
```
客户端与服务器之间进行连接的初始化，服务器会在这里为客户端动态的生成一个密匙

#### Handshaking
```csharp
Microsoft.VisualBasic.Net.SSL.SSLProtocols.Handshaking(Microsoft.VisualBasic.Net.SSL.Certificate,System.Net.IPEndPoint)
```
客户端与服务器之间初始化加密连接

|Parameter Name|Remarks|
|--------------|-------|
|CA|客户端的证书，这个是服务器来进行客户端程序的完整性验证的|


#### SSLServicesResponseHandler
```csharp
Microsoft.VisualBasic.Net.SSL.SSLProtocols.SSLServicesResponseHandler(Microsoft.VisualBasic.Net.SSL.SSLProtocols.ISSLServices,System.Int64,Microsoft.VisualBasic.Net.Protocols.RequestStream,System.Net.IPEndPoint,Microsoft.VisualBasic.Net.SSL.SSLProtocols.InstallCertificates)
```


|Parameter Name|Remarks|
|--------------|-------|
|ssl|-|
|CA|解密使用的证书凭据，这个用来鉴别客户端身份是否被伪造|
|request|-|
|remoteDev|-|
|InstallCertificates|-|



