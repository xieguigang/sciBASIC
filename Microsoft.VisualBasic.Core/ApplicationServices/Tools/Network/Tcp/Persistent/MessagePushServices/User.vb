#Region "Microsoft.VisualBasic::b3e9fc10f1f2408c56e6cb5242725b75, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Network\Tcp\Persistent\MessagePushServices\User.vb"

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

    '     Class USER
    ' 
    '         Properties: USER_ID
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: __receiveBroadcastMessage, (+3 Overloads) __sendMessage, __sendMessageToMe, IsUserOnLine, (+4 Overloads) SendMessage
    '                   ToString
    ' 
    '         Sub: (+2 Overloads) BeginConnect, (+2 Overloads) BroadCastMessage, (+2 Overloads) Dispose, SetDisconnectHandle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Net.Persistent.Application.Protocols
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Parallel

Namespace Net.Persistent.Application

    ''' <summary>
    ''' 服务器也相当于一个USER，只不过服务器的UID为0，即最高级的用户
    ''' </summary>
    ''' 
    <Protocol(GetType(ServicesProtocol.Protocols))>
    Public Class USER : Implements System.IDisposable

        Public ReadOnly Property USER_ID As Long

        Friend remotePort As Integer, remoteHost As String
        Dim __exceptionHandler As Abstract.ExceptionHandler
        Dim _requestHandler As ProtocolHandler

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="HostName"></param>
        ''' <param name="RemotePort"></param>
        ''' <param name="ID"></param>
        ''' <param name="DataRequestHandle">使用这个函数来获取外部发送过来的用户消息</param>
        ''' <param name="ExceptionHandler"></param>
        Sub New(HostName As String,
                RemotePort As Integer,
                ID As Long,
                DataRequestHandle As PushMessage,
                Optional ExceptionHandler As Abstract.ExceptionHandler = Nothing)

            Me.remoteHost = HostName
            Me.USER_ID = ID
            Me.remotePort = RemotePort
            Me.__dataRequestHandle = DataRequestHandle
            Me._requestHandler = New ProtocolHandler(Me)
        End Sub

        Sub New(services As System.Net.IPEndPoint, ID As Long, DataRequestHandle As PushMessage, Optional ExceptionHandler As Abstract.ExceptionHandler = Nothing)
            Call Me.New(New IPEndPoint(services), ID, DataRequestHandle, ExceptionHandler)
        End Sub

        Sub New(services As IPEndPoint, ID As Long, DataRequestHandle As PushMessage, Optional ExceptionHandler As Abstract.ExceptionHandler = Nothing)
            Call Me.New(services.IPAddress, services.Port, ID, DataRequestHandle, ExceptionHandler)
        End Sub

        Sub New(post As UserId, DataRequestHandle As PushMessage, Optional ExceptionHandler As Abstract.ExceptionHandler = Nothing)
            Call Me.New(post.Remote.IPAddress, post.Remote.Port, post.uid, DataRequestHandle, ExceptionHandler)
        End Sub

        Dim __dataRequestHandle As PushMessage
        Dim _pcnnSocket As Socket.PersistentClient

        Public Sub SetDisconnectHandle(handle As MethodInvoker)
            Try
                _pcnnSocket.RemoteServerShutdown = handle
            Catch ex As Exception
                ' 可能是在socket还没有启动的时候就设置句柄了，导致空引用，不过这个没有太多的影响，忽略这个错误
            End Try
        End Sub

        ''' <summary>
        ''' 请注意，线程会在这里阻塞
        ''' </summary>
        ''' <param name="ForceCloseConnection">远程主机强制关闭连接之后触发这个动作</param>
        Public Sub BeginConnect(Optional ForceCloseConnection As MethodInvoker = Nothing, Optional CA As Net.SSL.Certificate = Nothing)
            Dim remoteEp As System.Net.IPEndPoint = New System.Net.IPEndPoint(System.Net.IPAddress.Parse(Me.remoteHost), Me.remotePort)
            Dim request As RequestStream = ServicesProtocol.GetServicesConnection

            Call $"Begin connect to {remoteEp.ToString}".__DEBUG_ECHO
            If CA Is Nothing Then
                request = New Net.AsynInvoke(remoteEp).SendMessage(request)
            Else
                request = New Net.AsynInvoke(remoteEp).SendMessage(request, CA)
            End If

            Dim port As Integer = CInt(Val(request.GetUTF8String))

            Me._pcnnSocket = New Socket.PersistentClient(Me.remoteHost, port, Me.__exceptionHandler)
            Me._pcnnSocket.RemoteServerShutdown = ForceCloseConnection
            Me._pcnnSocket.Responsehandler = AddressOf Me._requestHandler.HandleRequest

            Call RunTask(AddressOf Me._pcnnSocket.BeginConnect)
            Call Me._pcnnSocket.WaitForConnected()
            Call Thread.Sleep(1000)
            Call Me._pcnnSocket.WaitForHash()

            request = ServicesProtocol.LogOnRequest(Me.USER_ID, Me._pcnnSocket.OnServerHashCode)
            If CA Is Nothing Then
                request = __sendMessage(request)
            Else
                request = CA.Encrypt(request)
                request = __sendMessage(request)
                request = CA.Decrypt(request)
            End If

            If Not request.Protocol = HTTP_RFC.RFC_OK Then
                '连接不成功
                Throw New Exception(NetResponse.RFC_BAD_REQUEST.GetUTF8String)
            End If

            Do While Not Me.disposedValue
                Call Thread.Sleep(1000)
            Loop
        End Sub

        ''' <summary>
        ''' 不会发生阻塞
        ''' </summary>
        ''' <param name="ForceCloseConnection">远程主机强制关闭连接之后触发这个动作</param>
        Public Sub BeginConnect(CA As Net.SSL.Certificate, Optional ForceCloseConnection As MethodInvoker = Nothing)
            Call RunTask(Sub() Call BeginConnect(ForceCloseConnection, CA))
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="CA"></param>
        ''' <param name="request"></param>
        ''' <param name="remote">由于数据都是通过中心服务器转发的，所以这个已经没有存在的意义了，但是为了和短连接的socket的数据处理接口保持兼容，所以还保留这个参数</param>
        ''' <returns></returns>
        <Protocol(ServicesProtocol.Protocols.SendMessage)>
        Private Function __sendMessageToMe(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Dim post As New SendMessagePost(request.ChunkBuffer)
            Return Me.__dataRequestHandle(post.FROM, post.Message)
        End Function

        <Protocol(ServicesProtocol.Protocols.Broadcast)>
        Private Function __receiveBroadcastMessage(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Return Me.__dataRequestHandle(CA, request)
        End Function

        Private Function __sendMessage(Message As String) As String
#If DEBUG Then
            Call Console.WriteLine($" * >> {NameOf(__sendMessage)}   {Message}")
#End If
            Dim reply As String = New Net.AsynInvoke(Me.remoteHost, Me.remotePort, Me.__exceptionHandler).SendMessage(Message)
            Return reply
        End Function

        Private Function __sendMessage(Message As Byte()) As Byte()
            Dim reply = New Net.AsynInvoke(Me.remoteHost, Me.remotePort, Me.__exceptionHandler).SendMessage(Message)
            Return reply
        End Function

        Private Function __sendMessage(request As RequestStream) As RequestStream
            Return New RequestStream(__sendMessage(request.Serialize))
        End Function

        ''' <summary>
        ''' True标识发送成功，False标识用户离线
        ''' </summary>
        ''' <param name="USER_ID"></param>
        ''' <param name="Message">在发送之前请对消息进行加密处理</param>
        ''' <returns></returns>
        Public Function SendMessage(USER_ID As Long, Message As RequestStream) As Boolean
            Dim request = ServicesProtocol.SendMessageRequest(Me.USER_ID, USER_ID, Message)
            Return SendMessage(request)
        End Function

        Public Function SendMessage(USER_ID As Long, Message As RequestStream, CA As SSL.Certificate) As Boolean
            Dim request = ServicesProtocol.SendMessageRequest(Me.USER_ID, USER_ID, Message)
            request = CA.Encrypt(request)
            Return SendMessage(request)
        End Function

        Public Function SendMessage(Message As RequestStream) As Boolean
            Dim bytesData = Message.Serialize
            bytesData = __sendMessage(bytesData)
            If RequestStream.IsAvaliableStream(bytesData) Then
                Message = New RequestStream(bytesData)
                Return Message.Protocol = HTTP_RFC.RFC_OK
            Else
                Return System.Text.Encoding.UTF8.GetString(bytesData).ParseBoolean
            End If
        End Function

        Public Function SendMessage(Message As RequestStream, CA As SSL.Certificate, Optional isPublicToken As Boolean = False) As Boolean
            Dim byteData = If(isPublicToken, CA.PublicEncrypt(Message), CA.Encrypt(Message)).Serialize
            byteData = __sendMessage(byteData)
            If RequestStream.IsAvaliableStream(byteData) Then
                Message = New RequestStream(byteData)

                If Message.IsSSLProtocol Then
                    Message = CA.Decrypt(Message)
                Else
                    Return CA.DecryptString(Message.GetUTF8String).ParseBoolean
                End If

                Return Message.GetUTF8String.ParseBoolean
            Else
                Return Encoding.UTF8.GetString(byteData).ParseBoolean
            End If
        End Function

        Public Sub BroadCastMessage(Message As RequestStream)
            Dim request As RequestStream = ServicesProtocol.BroadcastMessage(Me.USER_ID, Message)
            request = __sendMessage(request)
        End Sub

        Public Sub BroadCastMessage(Message As RequestStream, CA As SSL.Certificate)
            Dim request As RequestStream = ServicesProtocol.BroadcastMessage(Me.USER_ID, Message)
            request = CA.Encrypt(request)
            Call SendMessage(request)
        End Sub

        Public Function IsUserOnLine(USER_ID As Long) As Boolean
            Dim request As RequestStream = ServicesProtocol.IsUserOnlineRequest(USER_ID)
            request = __sendMessage(request)
            Return request.Protocol = HTTP_RFC.RFC_OK
        End Function

        Public Overrides Function ToString() As String
            Return USER_ID
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
