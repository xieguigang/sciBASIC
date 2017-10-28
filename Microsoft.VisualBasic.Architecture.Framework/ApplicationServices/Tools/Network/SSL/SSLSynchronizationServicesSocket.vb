#Region "Microsoft.VisualBasic::21658a0aca1119ab3cd31c6a84e96494, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Tools\Network\SSL\SSLSynchronizationServicesSocket.vb"

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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Protocols

Namespace Net.SSL

    Public Class SSLSynchronizationServicesSocket
        Implements IDisposable
        Implements ITaskDriver
        Implements IServicesSocket
        Implements SSL.SSLProtocols.ISSLServices

        Dim _ServicesSocket As Net.TcpSynchronizationServicesSocket

        ''' <summary>
        ''' 这个数字证书是当前版本下的服务器的客户端的数字签名，服务器会使用这个证书来验证客户端的文件是否被恶意破解，相当于公有密匙
        ''' </summary>
        Public ReadOnly Property CA As Certificate Implements ISSLServices.CA
        ''' <summary>
        ''' A table stores the certificates of the current connected clients on this server.
        ''' (连接上来的客户端的私有证书列表)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PrivateKeys As Dictionary(Of Long, Certificate) Implements ISSLServices.PrivateKeys

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="LocalPort"></param>
        ''' <param name="CA">服务器在部署的时候向对应版本您的客户端的数字签名</param>
        ''' <param name="exHandler">Public Delegate Sub <see cref="Abstract.ExceptionHandler"/>(ex As <see cref="Exception"/>)</param>
        Sub New(LocalPort As Integer,
                CA As SSL.Certificate,
                container As Object,
                Optional exHandler As Abstract.ExceptionHandler = Nothing)

            _DeclaringModule = container
            _ServicesSocket = New TcpSynchronizationServicesSocket(LocalPort, exHandler)
            _CA = CA
            _PrivateKeys = New Dictionary(Of Long, Certificate)
        End Sub

        Public ReadOnly Property IsShutdown As Boolean Implements IServicesSocket.IsShutdown
            Get
                If _ServicesSocket Is Nothing Then
                    Return True
                End If

                Return _ServicesSocket.IsShutdown
            End Get
        End Property

        Public ReadOnly Property IsRunning As Boolean Implements IServicesSocket.IsRunning
            Get
                If _ServicesSocket Is Nothing Then
                    Return False
                End If

                Return _ServicesSocket.Running
            End Get
        End Property

        ''' <summary>
        ''' 底层工作socket所监听的端口号
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LocalPort As Integer Implements IServicesSocket.LocalPort
            Get
                Return _ServicesSocket.LocalPort
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me._ServicesSocket.ToString
        End Function

#Region "Responsehandler"

        ''' <summary>
        ''' <see cref="DataRequestHandler"/>: 
        ''' Public Delegate Function <see cref="DataRequestHandler"/>(CA As <see cref="System.int64"/>, request As <see cref="RequestStream"/>, 
        ''' RemoteAddress As <see cref="System.Net.IPEndPoint"/>) As <see cref="System.Net.IPEndPoint"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property Responsehandler As DataRequestHandler Implements IServicesSocket.Responsehandler, ISSLServices.ResponseHandler
            Get
                Return _responsehandler
            End Get
            Set(value As DataRequestHandler)
                _ServicesSocket.Responsehandler = New DataRequestHandler(AddressOf __responsehandler)
                _responsehandler = value
            End Set
        End Property

        ''' <summary>
        ''' 生成证书的方法
        ''' </summary>
        ''' <returns></returns>
        Public Property ISSLServices_InstallCertificates As InstallCertificates =
            AddressOf InstallCertificates Implements ISSLServices.InstallCertificates

        ''' <summary>
        ''' 客户端和服务器握手之后触发这个动作
        ''' </summary>
        ''' <returns></returns>
        Public Property RaiseHandshakingEvent As HandshakingEvent =
            AddressOf SSLSynchronizationServicesSocket.HandShakingEventDoNothing Implements ISSLServices.RaiseHandshakingEvent

        ''' <summary>
        ''' Does this ssl server accepts the handshaking from the user client or just allow the client connect to this server from manual imports their certificates by using method <see cref="Install(Certificate, Boolean, String)"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property RefuseHandshake As Boolean Implements ISSLServices.RefuseHandshake

        Public ReadOnly Property DeclaringModule As Object Implements ISSLServices.DeclaringModule

        Dim _responsehandler As DataRequestHandler

        Private Function __responsehandler(CA As Long, request As RequestStream, remoteDev As System.Net.IPEndPoint) As RequestStream
            Return SSL.SSLProtocols.SSLServicesResponseHandler(Me, CA, request, remoteDev, ISSLServices_InstallCertificates)
        End Function
#End Region

        ''' <summary>
        ''' 等待底层socket成功进入监听模式
        ''' </summary>
        Public Sub WaitForStart()
            Call _ServicesSocket.WaitForStart()
        End Sub

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to
        ''' accept the connection, get data from the connected client,
        ''' echo that data back to the connected client.
        ''' It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)
        ''' </summary>
        ''' <remarks></remarks>
        Public Function Run(localEndPoint As System.Net.IPEndPoint) As Integer Implements IServicesSocket.Run
            Return _ServicesSocket.Run(localEndPoint)
        End Function

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to
        ''' accept the connection, get data from the connected client,
        ''' echo that data back to the connected client.
        ''' It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)
        ''' </summary>
        ''' <remarks></remarks>
        Public Function Run() As Integer Implements IServicesSocket.Run, ITaskDriver.Run
            Return _ServicesSocket.Run
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call Me._ServicesSocket.Free
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region

        Public Shared Function InstallCertificates(privateKey As String, uid As Long) As SSL.Certificate
            Return New SSL.Certificate(privateKey, uid)
        End Function

        Public Shared Sub HandShakingEventDoNothing(uid As Long, CA As SSL.Certificate, remote As System.Net.IPEndPoint)
            ' DO NOTHING
        End Sub

        ''' <summary>
        ''' If the property of <see cref="SSLSynchronizationServicesSocket.RefuseHandshake"/> is set to TRUE, then no more new client can be connect to this server object.
        ''' The only way to add new client on this server is using this function to imports the client's certificates direct manually.
        ''' (假若ssl层关闭了握手协议，则不可能会再有新的客户端可以连接到这个服务器上面了，则这个时候就可以使用这个方法来手工的为新的客户端导入数字证书，从而可以只接受指定的客户端的连接操作
        ''' 假若是证书同步操作的话，则可以将app授权证书通过这个方法导入到服务器模块接收主节点的证书同步操作)
        ''' </summary>
        ''' <param name="CA"></param>
        ''' <param name="[overrides]"></param>
        ''' <returns></returns>
        Public Function Install(CA As Certificate, [overrides] As Boolean, <CallerMemberName> Optional trace As String = "") As Boolean Implements ISSLServices.Install
            Return CAExtensions.InstallCommon(PrivateKeys, CA, [overrides], trace, MethodBase.GetCurrentMethod)
        End Function
    End Class
End Namespace
