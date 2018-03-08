#Region "Microsoft.VisualBasic::56424bbe3437e0f8e2bdd340bb3cb693, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Network\SSL\Certificate.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:

    '     Class Certificate
    ' 
    '         Properties: AppDomain, hash, IsPublicToken, PrivateKey, uid
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: __decrypt, __load, (+2 Overloads) CopyFrom, (+2 Overloads) Decrypt, DecryptString
    '                   (+2 Overloads) Encrypt, EncryptData, (+2 Overloads) Install, InstallPublicToken, PublicEncrypt
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.CodeDOM_VBC
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.SecurityString

Namespace Net.SSL

    ''' <summary>
    ''' 应用程序的完整性验证和用户身份的验证
    ''' </summary>
    Public Class Certificate : Implements SecurityString.SecurityStringModel.ISecurityStringModel

        Protected _SHA256 As SecurityString.SHA256

        ''' <summary>
        ''' 私有密匙
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property PrivateKey As String
            Get
                Return _SHA256.strPassphrase
            End Get
        End Property

        ''' <summary>
        ''' <see cref="Guid"/>计算出来的哈希值只能为负数，现在约定，当这个属性为0的时候就认为这个证书是公共密匙，
        ''' 这个一般是使用用户的账号所计算出来的哈希值
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property uid As Long
            Get
                Return _uid
            End Get
        End Property

        ''' <summary>
        ''' 初始化继承类所需要的
        ''' </summary>
        Protected _uid As Long

        Public ReadOnly Property IsPublicToken As Boolean
            Get
                Return uid = 0L
            End Get
        End Property

        ''' <summary>
        ''' 与<see cref="uid"/>属性所不同的是，这个属性是<see cref="privatekey"/>的哈希值，
        ''' 通常这个哈希值在请求resultful WebAPI的时候用来作为用户的唯一标识
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property hash As Long

        Public Overrides Function ToString() As String
            Return $"[{NameOf(Certificate)}] {uid} {_SHA256.Passphrase}"
        End Function

        ''' <summary>
        ''' 请注意这个构造方法会计算一遍密码的哈希值，假若需要直接进行初始化，请使用<see cref="Install"/>方法
        ''' </summary>
        ''' <param name="hash">用户的私有密匙</param>
        ''' <param name="uid">大小写无关的</param>
        Sub New(hash As String, uid As String)
            hash = SecurityString.MD5Hash.GetMd5Hash(hash)
            _SHA256 = New SecurityString.SHA256(hash, SALT)
            Me._uid = SecurityString.MD5Hash.ToLong(SecurityString.MD5Hash.GetMd5Hash(uid.ToLower))
            Me.hash = SecurityString.MD5Hash.ToLong(hash)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="hash">原始的密码，会在这个构造函数之中计算为哈希值产生新的密码。</param>
        ''' <param name="uid">客户端所发送过来的使用哈希值计算出来的唯一标识符</param>
        Sub New(hash As String, uid As Long)
            hash = SecurityString.MD5Hash.GetMd5Hash(hash)
            _SHA256 = New SecurityString.SHA256(hash, SALT)
            Me._uid = uid
            Me.hash = SecurityString.MD5Hash.ToLong(hash)
        End Sub

        ''' <summary>
        ''' 从服务器上面所返回来的握手数据
        ''' </summary>
        ''' <param name="handshakeData"></param>
        Sub New(handshakeData As RequestStream)
            Dim hash As String = SecurityString.MD5Hash.GetMd5Hash(handshakeData.GetUTF8String)
            _SHA256 = New SHA256(hash, SALT)
            Me._uid = handshakeData.uid
            Me.hash = SecurityString.MD5Hash.ToLong(hash)
        End Sub

        ''' <summary>
        ''' 这个构造函数不再计算哈希值而是直接初始化
        ''' </summary>
        ''' <param name="hash">必须是md5哈希值</param>
        Protected Sub New(hash As String)
            _SHA256 = New SHA256(hash, SALT)
            Me.hash = SecurityString.MD5Hash.ToLong(hash)
        End Sub

        Const SALT As String = "88888888"

        Public Shared Function CopyFrom(CA As SSL.Certificate, uid As String) As SSL.Certificate
            Dim hashCode As String = SecurityString.MD5Hash.ToLong(SecurityString.MD5Hash.GetMd5Hash(uid.ToLower))
            Return New SSL.Certificate(CA._SHA256.Passphrase) With {._uid = hashCode}
        End Function

        Public Shared Function CopyFrom(CA As SSL.Certificate, uid As Long) As SSL.Certificate
            Return New SSL.Certificate(CA._SHA256.Passphrase) With {._uid = uid}
        End Function

        ''' <summary>
        ''' 不计算密匙<paramref name="privateKey"/>哈希值而是直接安装
        ''' </summary>
        ''' <param name="privateKey"></param>
        ''' <param name="uid"></param>
        ''' <returns></returns>
        Public Shared Function Install(privateKey As String, uid As Long) As SSL.Certificate
            Return New SSL.Certificate(privateKey) With {._uid = uid}
        End Function

        Public Shared Function InstallPublicToken(publicKey As String) As SSL.Certificate
            Call $"Install public token ssl certificate.".__DEBUG_ECHO
            Return New SSL.Certificate(publicKey)
        End Function

        ''' <summary>
        ''' 函数会根据uid的值来设定协议为私有密匙还是公共密匙
        ''' </summary>
        ''' <param name="request"></param>
        ''' <returns></returns>
        Public Overridable Function Encrypt(request As RequestStream) As RequestStream
            Dim byteData As Byte() = request.Serialize
            Dim Protocol = If(IsPublicToken, RequestStream.Protocols.SSL_PublicToken, RequestStream.Protocols.SSL)
            byteData = _SHA256.Encrypt(byteData)
            request = New RequestStream(RequestStream.SYS_PROTOCOL, Protocol, byteData) With {.uid = uid}

            Return request
        End Function

        ''' <summary>
        ''' 强制将协议设定为公共密匙加密
        ''' </summary>
        ''' <param name="request"></param>
        ''' <returns></returns>
        Public Overridable Function PublicEncrypt(request As RequestStream) As RequestStream
            Dim byteData As Byte() = request.Serialize
            byteData = _SHA256.Encrypt(byteData)
            request = New RequestStream(RequestStream.SYS_PROTOCOL, RequestStream.Protocols.SSL_PublicToken, byteData) With {.uid = uid}

            Return request
        End Function

        ''' <summary>
        ''' <see cref="Decrypt(Byte())"/> <see cref="RequestStream.ChunkBuffer"/>
        ''' </summary>
        ''' <param name="request"></param>
        ''' <returns></returns>
        Public Overridable Function Decrypt(request As RequestStream) As RequestStream
            Return __decrypt(request, _SHA256)
        End Function

        Protected Shared Function __decrypt(request As RequestStream, sha256 As SecurityString.SHA256) As RequestStream
            Dim byteData As Byte() = request.ChunkBuffer
            byteData = sha256.Decrypt(byteData)
            request = New RequestStream(byteData)
            Return request
        End Function

        ''' <summary>
        ''' 检查应用程序的完整性
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property AppDomain As Certificate = Install(App.ExecutablePath, publicToken:=True)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="App">可执行程序的文件路径</param>
        ''' <returns></returns>
        Public Shared Function Install(App As String, Optional publicToken As Boolean = False) As Certificate
            Call Console.WriteLine()
            Call Console.WriteLine()
            Call $"**************************************************[ INSTALL {NameOf(Certificate)}:  {App.ToFileURL}]****************************************************************".__DEBUG_ECHO

            Dim Modules = (From [module] As String
                           In __load(App).AsParallel
                           Select [module], hash = SecurityString.MD5Hash.GetFileHashString([module])
                           Order By hash Ascending).ToArray '????已经排过序了，为什么顺序还是不一样
            For Each [module] In Modules
                Call $"   Installed Module ==> {[module].ToString}".__DEBUG_ECHO
            Next
            Dim caHashString As String = String.Join("+", (From [mod] In Modules Select [mod].hash).ToArray)
            Dim CA As Certificate = If(publicToken, Certificate.InstallPublicToken(SecurityString.GetMd5Hash(caHashString)), New Certificate(caHashString, NameOf(App)))

            Call Console.WriteLine()
            Call Console.WriteLine()
            Call $"**************************************************[ END OF INSTALL {NameOf(Certificate)} ==> {CA.ToString}]****************************************************************".__DEBUG_ECHO

            Return CA
        End Function

        Private Shared Function __load(path As String) As String()
            Dim assembly As System.Reflection.Assembly
            Try
                assembly = System.Reflection.Assembly.LoadFile(FileIO.FileSystem.GetFileInfo(path).FullName)
            Catch ex As Exception
                Throw New Exception(path.ToFileURL, ex)
            End Try
            Dim refListBuffer = GetReferences(assembly:=assembly, removeSystem:=True)

            Return refListBuffer
        End Function

        Public Overridable Function Decrypt(input() As Byte) As Byte() Implements SecurityStringModel.ISecurityStringModel.Decrypt
            Return _SHA256.Decrypt(input)
        End Function

        Public Overridable Function DecryptString(text As String) As String Implements SecurityStringModel.ISecurityStringModel.DecryptString
            Return _SHA256.DecryptString(text)
        End Function

        Public Overridable Function Encrypt(input() As Byte) As Byte() Implements SecurityStringModel.ISecurityStringModel.Encrypt
            Return _SHA256.Encrypt(input)
        End Function

        Public Overridable Function EncryptData(text As String) As String Implements SecurityStringModel.ISecurityStringModel.EncryptData
            Return _SHA256.EncryptData(text)
        End Function
    End Class
End Namespace
