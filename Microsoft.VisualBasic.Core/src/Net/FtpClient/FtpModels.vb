#Region "Microsoft.VisualBasic::2ee72d9813334f5634b766a4d44785c6, Microsoft.VisualBasic.Core\src\Net\FtpClient\FtpModels.vb"

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


    ' Code Statistics:

    '   Total Lines: 174
    '    Code Lines: 86 (49.43%)
    ' Comment Lines: 46 (26.44%)
    '    - Xml Docs: 73.91%
    ' 
    '   Blank Lines: 42 (24.14%)
    '     File Size: 5.92 KB


    '     Class FtpClientOptions
    ' 
    '         Properties: ConnectTimeout, DataChannelEncryption, EnableSsl, Encoding, ImplicitSsl
    '                     ValidateCertificate
    ' 
    '     Class FtpCredentials
    ' 
    '         Properties: Anonymous, Password, UserName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class FtpResponse
    ' 
    '         Properties: IsSuccess, RawLines, StatusCode, StatusText
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class FtpFileInfo
    ' 
    '         Properties: LastModified, Path, Size
    ' 
    '     Class FtpException
    ' 
    '         Properties: StatusCode
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class FtpConnectionException
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class FtpAuthenticationException
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class FtpProtocolException
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class FtpFileNotFoundException
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' FtpModels.vb - FTP 客户端辅助类型定义
'
' 包含: FtpClientOptions / FtpCredentials / FtpResponse
'       FtpDownloadProgress / FtpFileInfo
'       FtpException 及其子类
' ============================================================================

Imports System.Text

Namespace Net.FTP

    ' ===================== 配置选项 =====================

    ''' <summary>
    ''' FTP 客户端配置选项。
    ''' </summary>
    Public Class FtpClientOptions

        ''' <summary>连接超时时间，默认 30 秒。</summary>
        Public Property ConnectTimeout As TimeSpan = TimeSpan.FromSeconds(30)

        ''' <summary>控制连接的编码方式，默认 UTF-8。</summary>
        Public Property Encoding As Encoding = Encoding.UTF8

        ''' <summary>是否启用 SSL/TLS 加密 (FTPS)。</summary>
        Public Property EnableSsl As Boolean = False

        ''' <summary>是否使用隐式 SSL 模式 (端口 990)。</summary>
        Public Property ImplicitSsl As Boolean = False

        ''' <summary>是否验证服务器证书，设为 False 可接受自签名证书。</summary>
        Public Property ValidateCertificate As Boolean = True

        ''' <summary>是否加密数据通道 (PROT P)，仅在 EnableSsl=True 时生效。</summary>
        Public Property DataChannelEncryption As Boolean = True

    End Class

    ' ===================== 凭据 =====================

    ''' <summary>
    ''' FTP 登录凭据。
    ''' </summary>
    Public Class FtpCredentials

        Public Property UserName As String
        Public Property Password As String

        Public Sub New(userName As String, Optional password As String = "")
            Me.UserName = userName
            Me.Password = password
        End Sub

        ''' <summary>匿名登录凭据。</summary>
        Public Shared ReadOnly Property Anonymous As FtpCredentials
            Get
                Return New FtpCredentials("anonymous", "anonymous@localhost")
            End Get
        End Property

    End Class

    ' ===================== 响应 =====================

    ''' <summary>
    ''' FTP 服务器响应。
    ''' </summary>
    Public Class FtpResponse

        ''' <summary>三位状态码 (如 220, 331, 226)。</summary>
        Public ReadOnly Property StatusCode As Integer

        ''' <summary>状态文本 (不含状态码前缀)。</summary>
        Public ReadOnly Property StatusText As String

        ''' <summary>所有响应行 (含状态码前缀)，多行响应时有多条。</summary>
        Public ReadOnly Property RawLines As IReadOnlyList(Of String)

        Public Sub New(statusCode As Integer, statusText As String, rawLines As IReadOnlyList(Of String))
            _StatusCode = statusCode
            _StatusText = statusText
            _RawLines = rawLines
        End Sub

        ''' <summary>是否为成功响应 (2xx)。</summary>
        Public ReadOnly Property IsSuccess As Boolean
            Get
                Return StatusCode >= 200 AndAlso StatusCode < 300
            End Get
        End Property

        Public Overrides Function ToString() As String
            If RawLines.Count = 1 Then
                Return RawLines(0)
            End If
            Return StatusCode & " " & StatusText
        End Function

    End Class

    ' ===================== 文件信息 =====================

    ''' <summary>
    ''' 远程文件信息。
    ''' </summary>
    Public Class FtpFileInfo

        ''' <summary>文件路径。</summary>
        Public Property Path As String

        ''' <summary>文件大小 (字节)，-1 表示无法获取。</summary>
        Public Property Size As Long = -1

        ''' <summary>最后修改时间 (UTC)，DateTime.MinValue 表示无法获取。</summary>
        Public Property LastModified As DateTime = DateTime.MinValue

    End Class

    ' ===================== 异常 =====================

    ''' <summary>
    ''' FTP 异常基类。
    ''' </summary>
    Public Class FtpException
        Inherits Exception

        ''' <summary>FTP 状态码 (0 表示非协议错误)。</summary>
        Public ReadOnly Property StatusCode As Integer

        Public Sub New(message As String, Optional statusCode As Integer = 0)
            MyBase.New(message)
            _StatusCode = statusCode
        End Sub

        Public Sub New(message As String, innerException As Exception, Optional statusCode As Integer = 0)
            MyBase.New(message, innerException)
            _StatusCode = statusCode
        End Sub

    End Class

    ''' <summary>连接失败异常。</summary>
    Public Class FtpConnectionException
        Inherits FtpException
        Public Sub New(message As String, Optional innerException As Exception = Nothing)
            MyBase.New(message, innerException)
        End Sub
    End Class

    ''' <summary>认证失败异常。</summary>
    Public Class FtpAuthenticationException
        Inherits FtpException
        Public Sub New(message As String, statusCode As Integer)
            MyBase.New(message, statusCode)
        End Sub
    End Class

    ''' <summary>协议错误异常。</summary>
    Public Class FtpProtocolException
        Inherits FtpException
        Public Sub New(message As String, Optional statusCode As Integer = 0)
            MyBase.New(message, statusCode)
        End Sub
    End Class

    ''' <summary>文件未找到异常 (550)。</summary>
    Public Class FtpFileNotFoundException
        Inherits FtpException
        Public Sub New(message As String, statusCode As Integer)
            MyBase.New(message, statusCode)
        End Sub
    End Class
End Namespace
