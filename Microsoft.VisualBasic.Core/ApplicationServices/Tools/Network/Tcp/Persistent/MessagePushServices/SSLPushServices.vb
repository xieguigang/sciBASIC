#Region "Microsoft.VisualBasic::207dc029de94e511c91be0be3ac1991a, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Network\Tcp\Persistent\MessagePushServices\SSLPushServices.vb"

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

    '     Class SSLPushServices
    ' 
    '         Properties: CA, Connections, DeclaringModule, InstallCertificates, IsRunning
    '                     IsShutdown, LocalPort, PrivateKeys, PushServices, RaiseHandshakingEvent
    '                     RefuseHandshake, Responsehandler
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: __redirect, __responsehandler, Install, (+2 Overloads) Run
    ' 
    '         Sub: (+2 Overloads) Dispose, Install, WaitForRunning
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Persistent.Socket
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.SSL

Namespace Net.Persistent.Application

    ''' <summary>
    ''' 消息都是经过加密操作了的
    ''' </summary>
    Public Class SSLPushServices : Implements Net.Abstract.IServicesSocket
        Implements ISSLServices

        Public ReadOnly Property PushServices As Net.Persistent.Application.MessagePushServer

        ''' <summary>
        ''' 共有密匙
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CA As Certificate Implements ISSLServices.CA
        ''' <summary>
        ''' 连接到当前的这个服务器上面的客户端的私有密匙列表
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PrivateKeys As Dictionary(Of Long, Certificate) Implements ISSLServices.PrivateKeys

        Public ReadOnly Property IsShutdown As Boolean Implements IServicesSocket.IsShutdown
            Get
                Return _PushServices.IsShutdown
            End Get
        End Property

        Public ReadOnly Property LocalPort As Integer Implements IServicesSocket.LocalPort
            Get
                Return _PushServices.LocalPort
            End Get
        End Property

        Dim _responsehandler As DataRequestHandler

        Private Property Responsehandler As DataRequestHandler Implements IDataRequestHandler.Responsehandler, ISSLServices.ResponseHandler
            Get
                Return _responsehandler
            End Get
            Set(value As DataRequestHandler)
                _PushServices.Responsehandler = AddressOf __responsehandler
                _responsehandler = value
            End Set
        End Property

        Private Function __responsehandler(CA As Long, request As RequestStream, remoteDev As System.Net.IPEndPoint) As RequestStream
            Return SSL.SSLProtocols.SSLServicesResponseHandler(Me, CA, request, remoteDev, InstallCertificates)
        End Function

        Public ReadOnly Property Connections As WorkSocket()
            Get
                Return _PushServices.Connections
            End Get
        End Property

        Public Property InstallCertificates As InstallCertificates =
            AddressOf Net.SSL.SSLSynchronizationServicesSocket.InstallCertificates Implements ISSLServices.InstallCertificates

        Public Property RaiseHandshakingEvent As HandshakingEvent =
            AddressOf SSL.SSLSynchronizationServicesSocket.HandShakingEventDoNothing Implements ISSLServices.RaiseHandshakingEvent

        Public Property RefuseHandshake As Boolean Implements ISSLServices.RefuseHandshake

        Public ReadOnly Property IsRunning As Boolean Implements IServicesSocket.IsRunning
            Get
                Return Me._PushServices.Running
            End Get
        End Property

        Public ReadOnly Property DeclaringModule As Object Implements ISSLServices.DeclaringModule

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="LocalPort"></param>
        ''' <param name="OffLineMessageSendHandler">
        ''' Public Delegate Sub <see cref="OffLineMessageSendHandler"/>(FromUSER_ID As <see cref="Long"/>, USER_ID As <see cref="Long"/>, Message As <see cref="RequestStream"/>)
        ''' </param>
        ''' <param name="exHandler"></param>
        Sub New(LocalPort As Integer,
                container As Object,
                Optional OffLineMessageSendHandler As OffLineMessageSendHandler = Nothing,
                Optional exHandler As Abstract.ExceptionHandler = Nothing)
            _PushServices = New MessagePushServer(LocalPort, OffLineMessageSendHandler, exHandler)
            _DeclaringModule = container
            Responsehandler = AddressOf __redirect
            PrivateKeys = New Dictionary(Of Long, Certificate)
            Call _PushServices.Install(Me)
        End Sub

        Private Function __redirect(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            request = _PushServices.ProtocolHandler.HandleRequest(CA, request, remote)
            Return request
        End Function

        ''' <summary>
        ''' 安装新的公有密匙
        ''' </summary>
        ''' <param name="CA"></param>
        Public Sub Install(CA As SSL.Certificate)
            _CA = CA
        End Sub

        ''' <summary>
        ''' 安装新的用户私有密匙
        ''' </summary>
        ''' <param name="CA"></param>
        ''' <param name="[overrides]"></param>
        ''' <returns></returns>
        Public Function Install(CA As Certificate, [overrides] As Boolean, <CallerMemberName> Optional trace As String = "") As Boolean Implements ISSLServices.Install
            Return CAExtensions.InstallCommon(PrivateKeys, CA, [overrides], trace, MethodBase.GetCurrentMethod)
        End Function

        Public Sub WaitForRunning()
            Call _PushServices.WaitForRunning()
        End Sub

        Public Function Run() As Integer Implements IServicesSocket.Run, ITaskDriver.Run
            Return _PushServices.Run
        End Function

        Public Function Run(localEndPoint As System.Net.IPEndPoint) As Integer Implements IServicesSocket.Run
            Call _PushServices.Run(localEndPoint)
            Return 0
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
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
    End Class
End Namespace
