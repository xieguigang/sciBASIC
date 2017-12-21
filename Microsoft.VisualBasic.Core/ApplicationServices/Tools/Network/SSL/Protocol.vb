#Region "Microsoft.VisualBasic::a9d3b4643cdebfd30e5c7ed4825f5ef4, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Tools\Network\SSL\Protocol.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Win32

Namespace Net.SSL

    Public Module SSLProtocols

        ''' <summary>
        ''' 客户端与服务器之间初始化加密连接
        ''' </summary>
        ''' <param name="CA">客户端的证书，这个是服务器来进行客户端程序的完整性验证的</param>
        ''' <returns></returns>
        Public Function Handshaking(CA As SSL.Certificate, services As System.Net.IPEndPoint) As SSL.Certificate
            Dim request As RequestStream =
                New RequestStream(RequestStream.SYS_PROTOCOL,
                                  RequestStream.Protocols.SSLHandshake,
                                  New Byte() {}) With {
                    .uid = CA.uid
            }
            request = New Net.AsynInvoke(services).SendMessage(request, CA, isPublicToken:=True) '这个函数会把用户的账号和当前的客户端的数字证书发送给服务器
#If DEBUG Then
            Call $"[{MethodBase.GetCurrentMethod.GetFullName}] Handshaking {NameOf(CA)} hash:={CA.uid}".__DEBUG_ECHO
#End If
            '服务器验证数字证书通过之后就会返回动态的客户端的私有密匙，一般是对随机数做MD5得到私有密匙
            Dim PrivateKey As New SSL.Certificate(request)
#If DEBUG Then
            Call $"[{MethodBase.GetCurrentMethod.GetFullName}] Handshaking {NameOf(PrivateKey)} hash:={PrivateKey.uid}".__DEBUG_ECHO
#End If
            Return PrivateKey
        End Function

        Public Function Handshaking(PublicToken As SSL.Certificate, uid As String, services As System.Net.IPEndPoint) As SSL.Certificate
            Dim CA = SSL.Certificate.CopyFrom(PublicToken, uid)
#If DEBUG Then
            Call $"[{MethodBase.GetCurrentMethod.GetFullName}] Handshaking {NameOf(CA)} hash:={CA.uid}".__DEBUG_ECHO
#End If
            Dim privateKey As SSL.Certificate = Handshaking(CA, services)
#If DEBUG Then
            Call $"[{MethodBase.GetCurrentMethod.GetFullName}] Handshaking {NameOf(privateKey)} hash:={privateKey.uid}".__DEBUG_ECHO
#End If
            Return privateKey
        End Function

        Public Function Handshaking(CA As SSL.Certificate, services As System.Net.IPEndPoint, Install As InstallCertificates) As SSL.Certificate
            Dim request As RequestStream =
                New RequestStream(RequestStream.SYS_PROTOCOL,
                                  RequestStream.Protocols.SSLHandshake,
                                  New Byte() {}) With {
                    .uid = CA.uid
            }
            request = New Net.AsynInvoke(services).SendMessage(request, CA, isPublicToken:=True) '这个函数会把用户的账号和当前的客户端的数字证书发送给服务器
#If DEBUG Then
            Call $"[{MethodBase.GetCurrentMethod.GetFullName}] Handshaking {NameOf(CA)} hash:={CA.uid}".__DEBUG_ECHO
#End If
            '服务器验证数字证书通过之后就会返回动态的客户端的私有密匙，一般是对随机数做MD5得到私有密匙
            Dim PrivateKey As SSL.Certificate = Install(request.GetUTF8String, CA.uid)
#If DEBUG Then
            Call $"[{MethodBase.GetCurrentMethod.GetFullName}] Handshaking {NameOf(PrivateKey)} hash:={PrivateKey.uid}".__DEBUG_ECHO
#End If
            Return PrivateKey
        End Function

        ''' <summary>
        ''' 抽象SSL服务器
        ''' </summary>
        Public Interface ISSLServices

            ''' <summary>
            ''' 告诉SSL层如何安装数字证书
            ''' </summary>
            ''' <returns></returns>
            Property InstallCertificates As InstallCertificates
            ''' <summary>
            ''' 有新的客户端请求进行连接
            ''' </summary>
            ''' <returns></returns>
            Property RaiseHandshakingEvent As HandshakingEvent
            ''' <summary>
            ''' 对于某些应用出于安全性的考虑，会将这里设置为False，则服务器就会全部拒绝后面的所有的握手请求，只接受来自于从外部导入的用户证书的数据请求
            ''' </summary>
            ''' <returns></returns>
            Property RefuseHandshake As Boolean

            ''' <summary>
            ''' 公共密匙
            ''' </summary>
            ''' <returns></returns>
            ReadOnly Property CA As SSL.Certificate
            ''' <summary>
            ''' 客户端的私有密匙
            ''' </summary>
            ''' <returns></returns>
            ReadOnly Property PrivateKeys As Dictionary(Of Long, SSL.Certificate)
            ''' <summary>
            ''' 处理私有密匙的数据请求
            ''' </summary>
            ''' <returns></returns>
            ReadOnly Property ResponseHandler As Net.Abstract.DataRequestHandler
            ReadOnly Property DeclaringModule As Object

            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="CA"></param>
            ''' <param name="[overrides]">当证书的哈希值有冲突的时候，新安装的证书<paramref name="ca"/>可不可以将旧的证书覆盖掉</param>
            Function Install(CA As Certificate, [overrides] As Boolean, Optional trace As String = "") As Boolean

        End Interface

        Public Delegate Sub HandshakingEvent(uid As Long, CA As SSL.Certificate, remoteDev As System.Net.IPEndPoint)
        Public Delegate Function InstallCertificates(privateKey As String, uid As Long) As Certificate

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ssl"></param>
        ''' <param name="CA">解密使用的证书凭据，这个用来鉴别客户端身份是否被伪造</param>
        ''' <param name="request"></param>
        ''' <param name="remoteDev"></param>
        ''' <param name="InstallCertificates"></param>
        ''' <returns></returns>
        Public Function SSLServicesResponseHandler(ssl As ISSLServices, CA As Long,
                                                   request As RequestStream,
                                                   remoteDev As System.Net.IPEndPoint,
                                                   InstallCertificates As InstallCertificates) As RequestStream
            Dim uid As Long

            If request.IsSSL_PublicToken Then
                uid = request.uid
                request = ssl.CA.Decrypt(request)
            End If

            If request.IsSSLHandshaking Then  '客户端与服务器之间进行连接的初始化，服务器会在这里为客户端动态的生成一个密匙
                request = __sslHandshake(uid, ssl, request, remoteDev, InstallCertificates)
                Return request
            ElseIf request.IsSSLProtocol Then
                uid = request.uid

                If Not ssl.PrivateKeys.ContainsKey(uid) Then  ' 不存在的数字证书
                    ' 记录进系统日志
                    If WindowsServices.Initialized Then
                        Call ServicesLogs.WriteEntry({$"Remote socket {remoteDev.ToString} try send request with an not authorised certificates, and ssl server refused this request!",
                                                     $"{NameOf(CA)} (not_authorised)   {CA}",
                                                     $"{NameOf(remoteDev)}:  {remoteDev.ToString}"},
                                                     $"{ssl.DeclaringModule.GetType.FullName} [{Scripting.ToString(ssl.DeclaringModule)}]  ==> {MethodBase.GetCurrentMethod}",
                                                     EventLogEntryType.Warning)
                    End If

                    Return New RequestStream(RequestStream.SYS_PROTOCOL,
                                             RequestStream.Protocols.InvalidCertificates,
                                             NameOf(RequestStream.Protocols.InvalidCertificates))
                End If

                Dim PrivateCertificate As SSL.Certificate = ssl.PrivateKeys(uid)
                request = PrivateCertificate.Decrypt(request)   '使用用户的私有密匙进行加密
                request = ssl.ResponseHandler(CA, request, remoteDev)  ' CA应该是用户客户端的数字证书编号
                request = PrivateCertificate.Encrypt(request)
                Return request
            End If

            Return NetResponse.RFC_NO_CERT
        End Function

        ''' <summary>
        ''' 客户端与服务器之间进行连接的初始化，服务器会在这里为客户端动态的生成一个密匙
        ''' </summary>
        ''' <returns></returns>
        Private Function __sslHandshake(uid As Long, ssl As ISSLServices,
                                        request As RequestStream,
                                        remoteDev As System.Net.IPEndPoint,
                                        InstallCertificates As InstallCertificates) As RequestStream
            If ssl.RefuseHandshake Then
                Return New RequestStream(RequestStream.SYS_PROTOCOL,
                                         RequestStream.Protocols.InvalidCertificates,
                                         "Services Refused!")
            End If

            Dim key As String = Guid.NewGuid.ToString
            key = SecurityString.MD5Hash.GetMd5Hash(key)

            If uid <> request.uid Then
                Return New RequestStream(RequestStream.SYS_PROTOCOL,
                                         RequestStream.Protocols.InvalidCertificates,
                                         NameOf(RequestStream.Protocols.InvalidCertificates))
            Else
                request = New RequestStream(RequestStream.SYS_PROTOCOL,
                                            RequestStream.Protocols.SSLHandshake, key) With {
                                            .uid = uid
                }
            End If

            If ssl.PrivateKeys.ContainsKey(uid) Then  ' 哈希函数设计不正确，有重复的哈希值，则当前的握手用户不能够使用这个哈希值，需要重新握手
                Call $"{NameOf(SSLServicesResponseHandler)} ==> {uid} was duplicated!".__DEBUG_ECHO
                Return New RequestStream(RequestStream.SYS_PROTOCOL, RequestStream.Protocols.InvalidCertificates, "Duplicated hash value!")
            End If

#If DEBUG Then
            Call $"[{MethodBase.GetCurrentMethod.GetFullName}] Handshaking hash:={uid}".__DEBUG_ECHO
#End If

            Dim PrivateKey As SSL.Certificate = InstallCertificates(key, uid)
            Call ssl.PrivateKeys.Add(uid, PrivateKey)
            Call ssl.RaiseHandshakingEvent()(uid, PrivateKey, remoteDev)

            request = Net.SSL.Certificate.CopyFrom(ssl.CA, uid).Encrypt(request)

            Return request
        End Function

    End Module
End Namespace
