#Region "Microsoft.VisualBasic::c53149948d7ae834100a4b32af27d747, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Tools\Network\Tcp\Persistent\MessagePushServices\MessagePushServer.vb"

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
Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Net.Persistent.Application.Protocols
Imports Microsoft.VisualBasic.Net.Persistent.Socket
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace Net.Persistent.Application

    ''' <summary>
    ''' 长连接模式的消息推送服务器
    ''' </summary>
    <Protocol(GetType(ServicesProtocol.Protocols))>
    Public Class MessagePushServer : Inherits ServicesSocket
        Implements IEnumerable(Of KeyValuePair(Of Long, WorkSocket))
        Implements ITaskDriver, IDataRequestHandler

        Public ReadOnly Property ProtocolHandler As ProtocolHandler

        Dim _socketList As New Dictionary(Of Long, WorkSocket)
        ''' <summary>
        ''' 客户端对这个服务器的端口号是自动配置的，只需要向客户端返回<see cref="_LocalPort"/>端口就可以了
        ''' </summary>
        Dim _workSocket As TcpSynchronizationServicesSocket
        Dim _offlineMessageSendHandler As OffLineMessageSendHandler
        ''' <summary>
        ''' 使用证书来加密发出去的消息
        ''' </summary>
        Dim _sslLayer As SSL.ISSLServices

        Public ReadOnly Property SSLMode As Boolean

        Public Sub Install(ssl As SSL.ISSLServices)
            _sslLayer = ssl
            _SSLMode = Not ssl Is Nothing
        End Sub

        ''' <summary>
        ''' 从这个端口号进行登录（协同长连接的socket正常工作的socket的端口号，可以看作为UserAPI）
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property LocalPort As Integer
            Get
                Return Me._workSocket.LocalPort
            End Get
        End Property

        Dim _responsehandler As DataRequestHandler

        Friend Property Responsehandler As DataRequestHandler Implements IDataRequestHandler.Responsehandler
            Get
                Return _responsehandler
            End Get
            Set(value As DataRequestHandler)
                _responsehandler = value
                Me._workSocket.Responsehandler = AddressOf __requestHandlerInterface
            End Set
        End Property

        ''' <summary>
        ''' 只要是为ssl服务设置的
        ''' </summary>
        ''' <param name="remote"></param>
        ''' <returns></returns>
        Private Function __requestHandlerInterface(CA As Long,
                                                   requestData As RequestStream,
                                                   remote As System.Net.IPEndPoint) As RequestStream
            requestData = _responsehandler(CA, requestData, remote)
            Return requestData
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="LocalPort"></param>
        ''' <param name="OffLineMessageSendHandler">Public Delegate Sub <see cref="OffLineMessageSendHandler"/>(FromUSER_ID As <see cref="Long"/>, USER_ID As <see cref="Long"/>, Message As <see cref="RequestStream"/>)</param>
        ''' <param name="exHandler"></param>
        Sub New(Optional LocalPort As Integer = 11000,
                Optional OffLineMessageSendHandler As OffLineMessageSendHandler = Nothing,
                Optional exHandler As Abstract.ExceptionHandler = Nothing)

            Call MyBase.New(GetFirstAvailablePort(5000), exHandler)

            Me._ProtocolHandler = New ProtocolHandler(Me)
            Me.AcceptCallbackHandleInvoke = AddressOf AcceptClient
            Me._workSocket = New TcpSynchronizationServicesSocket(AddressOf _ProtocolHandler.HandleRequest, LocalPort, Me.__exceptionHandle)
            Me._offlineMessageSendHandler = If(OffLineMessageSendHandler Is Nothing,
                Sub([from], USER_ID, MESSAGE) Call Console.WriteLine($" >>> [DEBUG {Now.ToString},  {from} => {USER_ID}]  {MESSAGE}"),
                OffLineMessageSendHandler)
        End Sub

        Public Overrides Sub Run(localEndPoint As System.Net.IPEndPoint)
            Call New Thread(AddressOf Me._workSocket.Run).Start()
            Call Thread.Sleep(1000)
            Call $"please logon server {Me.GetType.Name} at api_port={Me._workSocket.LocalPort}".__DEBUG_ECHO
            Call MyBase.Run(localEndPoint)
        End Sub

        Public Overrides Function Run() As Integer Implements ITaskDriver.Run
            Return MyBase.Run()
        End Function

        ''' <summary>
        ''' Disconnect user persistent connection who have the specific <paramref name="user_id"/> from this server.
        ''' (断开服务器与用户客户端的长连接)
        ''' </summary>
        ''' <param name="USER_ID">This user will be deleted from the server registry.</param>
        ''' <param name="removeCA">是否在删除socket句柄的时候还会删除相对应的ssl证书</param>
        Public Sub DisconnectUser(USER_ID As Long, removeCA As Boolean)
            If Me._socketList.ContainsKey(USER_ID) Then
                Dim socket As WorkSocket = _socketList(USER_ID)
                Call _socketList.Remove(USER_ID)
                Call $"Clean up connection for user {USER_ID} previous connection.".__DEBUG_ECHO
                Call socket.Free
            End If
            If removeCA AndAlso SSLMode Then
                If _sslLayer.PrivateKeys.ContainsKey(USER_ID) Then
                    Call _sslLayer.PrivateKeys.Remove(USER_ID)
                End If
            End If
        End Sub

        Protected Overrides Sub __socketCleanup(hash As Integer)
            Dim LQuery = (From usr In _socketList Where hash = usr.Value.GetHashCode Select usr.Key).FirstOrDefault
            Call DisconnectUser(LQuery, True)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="From"></param>
        ''' <param name="USER_ID"></param>
        ''' <param name="Message"></param>
        Public Sub SendMessage(From As Long, USER_ID As Long, Message As RequestStream)
            Dim request = ServicesProtocol.SendMessageRequest(From, USER_ID, Message)
            Call __sendMessage(From, USER_ID, request)
        End Sub

        ''' <summary>
        ''' 发送出去的数据需要进行加密，假若是ssl模式的话
        ''' </summary>
        ''' <param name="From"></param>
        ''' <param name="USER_ID"></param>
        ''' <param name="Message"></param>
        ''' <returns></returns>
        Private Function __sendMessage(From As Long, USER_ID As Long, Message As RequestStream) As RequestStream
#If DEBUG Then
            Call Console.Write($"{MethodBase.GetCurrentMethod.GetFullName}   {NameOf(USER_ID)}:{USER_ID} mappings to ")
#End If
            USER_ID = _UidMappings(USER_ID)
#If DEBUG Then
            Call Console.WriteLine(USER_ID) '衔接着上一个输出语句
#End If
            If Me._socketList.ContainsKey(USER_ID) Then
                Dim Socket As WorkSocket = Me._socketList(USER_ID)
                Return __sendMessage(Socket, From, USER_ID, Message)
            End If

#If DEBUG Then
            Call $"Unable to found user '{USER_ID}' on server...".__DEBUG_ECHO
            Call $"Current users: {String.Join("; ", _socketList.Keys.ToArray(Function(id) CStr(id)))}".__DEBUG_ECHO
#End If
            Call Me._offlineMessageSendHandler(From, USER_ID, Message)
            Return NetResponse.RFC_TEMP_REDIRECT
        End Function

        ''' <summary>
        ''' 将外部编号映射为内部的客户端句柄
        ''' 假若找不到，请返回-1
        ''' </summary>
        ''' <returns></returns>
        Public Property UidMappings As Func(Of Long, Long) = AddressOf __nonUidMappings
        Public Property UidMappingsBack As Func(Of Long, Long) = AddressOf __nonUidMappings

        Private Shared Function __nonUidMappings(USER_ID As Long) As Long
            Return USER_ID
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="socket"></param>
        ''' <param name="From">这个是这一条消息的源头，可能需要进行映射</param>
        ''' <param name="USER_ID"></param>
        ''' <param name="Message"></param>
        ''' <returns></returns>
        Private Function __sendMessage(socket As WorkSocket,
                                       From As Long,
                                       USER_ID As Long,
                                       Message As RequestStream) As RequestStream

            From = Me._UidMappingsBack(From)

            If socket Is Nothing OrElse socket.workSocket Is Nothing Then
                Call _socketList.Remove(USER_ID)
                Call Me._offlineMessageSendHandler(From, USER_ID, Message)
                Return NetResponse.RFC_TEMP_REDIRECT
            End If

#If DEBUG Then
            Call Console.WriteLine($"*Call {NameOf(__sendMessage)}  ===> {Message}")
#End If
            If SSLMode Then
                Dim CA As SSL.Certificate = _sslLayer.PrivateKeys(USER_ID)
                Dim post As New SendMessagePost(Message.ChunkBuffer)
                post.Message = CA.Encrypt(post.Message)
                post.FROM = From
                Message = New RequestStream(ServicesProtocol.ProtocolEntry,
                                            ServicesProtocol.Protocols.SendMessage,
                                            post.Serialize)
#If DEBUG Then
                Call $"Request encrypts from {CA.uid} job done!".__DEBUG_ECHO
#End If
            End If

            Call socket.SendMessage(Message) '原封不动的进行数据转发
            Return NetResponse.RFC_OK
        End Function

        ''' <summary>
        ''' 用户客户端请求发送消息至指定编号的用户的终端之上
        ''' </summary>
        ''' <param name="CA"></param>
        ''' <param name="request"></param>
        ''' <param name="remote"></param>
        ''' <returns></returns>
        <Protocol(ServicesProtocol.Protocols.SendMessage)>
        Private Function __usrInvokeSend(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Dim [From] As Long, USER_ID As Long
#If DEBUG Then
            Call $"{NameOf(From)}:{From} invoke send to {NameOf(USER_ID)}:{USER_ID}".__DEBUG_ECHO
#End If
            If ServicesProtocol.GetSendMessage(request, From, USER_ID) Then
                Return Me.__sendMessage(From, USER_ID, request)
            Else
                Return NetResponse.RFC_TOKEN_INVALID
            End If
        End Function

        <Protocol(ServicesProtocol.Protocols.Logon)>
        Private Function __Logon(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Dim USER_ID As Long, remoteEp As String = ""

            If Not ServicesProtocol.GetLogOnUSER(request.GetUTF8String, USER_ID, remoteEp) Then
                Return NetResponse.RFC_TOKEN_INVALID
            End If

            Dim hash As Integer = CInt(Val(remoteEp))

            If Not Me._Connections.ContainsKey(hash) Then
                Call Console.WriteLine($"No connection could be made! {hash} for socket hash is not exists in the hash table!")
                Return NetResponse.RFC_CONFLICT
            End If

            Dim SocketClient As WorkSocket = Nothing
            Call Me._Connections.TryGetValue(hash, SocketClient)

            If SocketClient Is Nothing Then
                Call Console.WriteLine("No connection could be made!")
                Return NetResponse.RFC_BAD_GATEWAY
            Else
                Call Console.WriteLine(" >> " & SocketClient.workSocket.RemoteEndPoint.ToString)
            End If

            Call DisconnectUser(USER_ID, False)
            Call _socketList.Add(USER_ID, SocketClient)

            Return NetResponse.RFC_OK
        End Function

        <Protocol(ServicesProtocol.Protocols.Broadcast)>
        Private Function __broadcastMessage(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            For Each cnn In Me.Connections
                Call cnn.SendMessage(request)
            Next
            Return NetResponse.RFC_OK
        End Function

        <Protocol(ServicesProtocol.Protocols.GetMyIPAddress)>
        Private Function __getMyIPAddress(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Return New RequestStream(0, HTTP_RFC.RFC_OK, remote.ToString.Split(":"c)(Scan0))
        End Function

        <Protocol(ServicesProtocol.Protocols.ConfigConnection)>
        Private Function __isGetSocketPortal(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Return New RequestStream(0, HTTP_RFC.RFC_OK, CStr(_LocalPort))
        End Function

        <Protocol(ServicesProtocol.Protocols.IsUserOnline)>
        Private Function __isUserOnlineQuery(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Dim USER_ID As Long = Scripting.CTypeDynamic(Of Long)(request.GetUTF8String)
            Dim result As String = CStr(Me._socketList.ContainsKey(USER_ID))
            Return New RequestStream(0, HTTP_RFC.RFC_OK, result)
        End Function

        Dim _freeCnnInfo As New List(Of String)

        ''' <summary>
        ''' 哈希值不存在于现有的登录用户列表之中就是空闲连接
        ''' </summary>
        Public Sub RemoveFreeConnections()
            Dim LQuery = (From Guid As String In _freeCnnInfo.AsParallel  '上一次刷新的时候的空闲连接
                          Let LowCnn = (From cnn In Me.Connections.AsParallel
                                        Where Guid.Equals(CStr(cnn.GetHashCode))
                                        Select cnn).FirstOrDefault
                          Where Not LowCnn Is Nothing AndAlso
                              (From cnn In Me._socketList
                               Where Guid.Equals(CStr(cnn.Value.GetHashCode))
                               Select cnn).ToArray.IsNullOrEmpty  '哈希值不存在的
                          Select LowCnn).ToArray  '对于上一次刷新的列表之中的连接而言，假若在这么长的一段时间间隔之中还是处于空闲状态，则服务器会将这些连接断开连接
            For Each cnn In LQuery
                Call Me.ForceCloseHandle(cnn)
                Call cnn.Free
            Next

            Call $"Clean up {LQuery.Length } free connections.....".__DEBUG_ECHO
            '获取新产生的空闲连接
            _freeCnnInfo = (From cnn In Me._Connections.AsParallel
                            Where (From item In Me._socketList Where item.Value.GetHashCode = cnn.GetHashCode Select 1).ToArray.IsNullOrEmpty
                            Select Guid = CStr(cnn.GetHashCode)).AsList
            Call $"{_freeCnnInfo.Count} free connections pending for clean up....".__DEBUG_ECHO

            For Each usr In Me._socketList.ToArray
                Call usr.Value.SendMessage(NetResponse.RFC_OK)
            Next
        End Sub

        ''' <summary>
        ''' 建立一个新的连接
        ''' </summary>
        ''' <param name="Client"></param>
        Private Sub AcceptClient(Client As WorkSocket)
            'Do Nothing
            Call $"{Client.workSocket.RemoteEndPoint.ToString} connection request accept!".__DEBUG_ECHO
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of Long, WorkSocket)) Implements IEnumerable(Of KeyValuePair(Of Long, WorkSocket)).GetEnumerator
            For Each entry In Me._socketList.ToArray
                Yield entry
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                For Each cnn In Me._socketList.ToArray
                    Call DisconnectUser(cnn.Key, True)
                Next
            End If
            Call MyBase.Dispose(disposing)
        End Sub
    End Class
End Namespace
