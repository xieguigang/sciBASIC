#Region "Microsoft.VisualBasic::0fab89507788ada7b43732bcda0dfbc1, www\Microsoft.VisualBasic.NETProtocol\Persistent\TcpSynchronizationServicesSocket.vb"

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

    '     Class TcpSynchronizationServicesSocket
    ' 
    '         Properties: ClientDecryptionHandle, Connections, IsShutdown, LocalPort, RequestHandler
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: BeginListen, LoopbackEndPoint
    ' 
    '         Sub: AcceptCallback, (+2 Overloads) Run, WaitForRunning
    '         Delegate Sub
    ' 
    '             Properties: AcceptCallbackHandleInvoke
    ' 
    '             Sub: (+2 Overloads) Dispose, ForceCloseHandle
    '         Class WorkSocket
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: ReadCallback, Send, SendCallback, SendMessage
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading

Imports Microsoft.VisualBasic

Namespace WrapperClassTools.Net.PersistentConnection


    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' 一、TCP长连接
    ''' 
    ''' 正常情况下，一条TCP连接建立后，只要双不提出关闭请求并且不出现异常情况，这条连接是一直存在的，
    ''' 操作系统不会自动去关闭它，甚至经过物理网络拓扑的改变之后仍然可以使用。
    ''' 所以一条连接保持几天、几个月、几年或者更长时间都有可能，只要不出现异常情况或由用户（应用层）主动关闭。
    ''' 在编程中， 往往需要建立一条TCP连接， 并且长时间处于连接状态。
    ''' 所谓的TCP长连接并没有确切的时间限制， 而是说这条连接需要的时间比较长。
    ''' 
    ''' 二、TCP连接的正常中断
    ''' 
    ''' TCP连接在事务处理完毕之后， 由一方提出关闭连接请求， 双方通过四次握手（建立连接是三次握手， 
    ''' 当然可以通过优化TCP / IP协议栈来减少握手的次数来提高性能， 但这样会形成不规范或者不优雅的通信）来正常关闭连接
    ''' 
    ''' 三、TCP连接的异常中断
    ''' 
    ''' 导致TCP连接异常中断的因素有： 物理连接被中断、操作系统down机、程序崩溃等等。
    ''' </remarks>
    Public Class TcpSynchronizationServicesSocket
        Implements System.IDisposable

#Region "INTERNAL FIELDS"

        Dim _ThreadEndAccept As Boolean = True
        ''' <summary>
        ''' 这个函数指针用于处理来自于客户端的请求
        ''' </summary>
        ''' <remarks></remarks>
        Dim _InternalRequestHandler As DataResponseHandler

        ''' <summary>
        ''' Socket对象监听的端口号
        ''' </summary>
        ''' <remarks></remarks>
        Protected _LocalPort As Integer
        Protected _InternalExceptionHandle As ExceptionHandler
        'Dim _InternalpLogList As New List(Of PerformanceLog)
        Dim _InternalSocketListener As Socket

#End Region

        ''' <summary>
        ''' The server services listening on this local port.(当前的这个服务器对象实例所监听的本地端口号)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property LocalPort As Integer
            Get
                Return _LocalPort
            End Get
        End Property

        ''' <summary>
        ''' This function pointer using for the data request handling of the data request from the client socket.
        ''' (这个函数指针用于处理来自于客户端的请求)
        ''' </summary>
        ''' <remarks></remarks>
        Public Property RequestHandler As DataResponseHandler
            Get
                Return _InternalRequestHandler
            End Get
            Set(value As DataResponseHandler)
                _InternalRequestHandler = value
            End Set
        End Property

        Public ReadOnly Property IsShutdown As Boolean
            Get
                Return disposedValue
            End Get
        End Property

        ''' <summary>
        ''' 消息处理的方法接口： Public Delegate Function DataResponseHandler(str As String, RemotePort As Integer) As String
        ''' </summary>
        ''' <param name="DataArrivalEventHandler">Public Delegate Function DataResponseHandler(str As String, RemotePort As Integer) As String</param>
        ''' <param name="LocalPort">监听的本地端口号，假若需要进行端口映射的话，则可以在<see cref="Run"></see>方法之中设置映射的端口号</param>
        ''' <remarks></remarks>
        Sub New(DataArrivalEventHandler As DataResponseHandler,
                Optional LocalPort As Integer = 11000,
                Optional exHandler As ExceptionHandler = Nothing)

            Me._InternalRequestHandler = DataArrivalEventHandler
            Me._LocalPort = LocalPort
            Me._InternalExceptionHandle = If(exHandler Is Nothing, Sub(ex As Exception) Call Console.WriteLine(ex.ToString), exHandler)
        End Sub

        Sub New(DataArrivalEventHandler As DataResponseHandler, exHandler As ExceptionHandler)
            Me._InternalRequestHandler = DataArrivalEventHandler
            Me._InternalExceptionHandle = If(exHandler Is Nothing, Sub(ex As Exception) Call Console.WriteLine(ex.ToString), exHandler)
        End Sub

        ''' <summary>
        ''' 函数返回Socket的注销方法
        ''' </summary>
        ''' <param name="DataArrivalEventHandler">Public Delegate Function DataResponseHandler(str As String, RemotePort As Integer) As String</param>
        ''' <param name="LocalPort"></param>
        ''' <param name="exHandler"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function BeginListen(DataArrivalEventHandler As DataResponseHandler,
                                           Optional LocalPort As Integer = 11000,
                                           Optional exHandler As ExceptionHandler = Nothing) As Action
            Dim Socket As New TcpSynchronizationServicesSocket(DataArrivalEventHandler, LocalPort, exHandler)
            Call (Sub() Call Socket.Run()).BeginInvoke(Nothing, Nothing)
            Return AddressOf Socket.Dispose
        End Function

        Public ReadOnly Property ClientDecryptionHandle As Func(Of String, String)
            Get
                Return AddressOf SecurityString.SHA256.CertificateSigned.DecryptString
            End Get
        End Property

        Public Function LoopbackEndPoint(Port As Integer) As System.Net.IPEndPoint
            Return New System.Net.IPEndPoint(System.Net.IPAddress.Loopback, Port)
        End Function

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to
        ''' accept the connection, get data from the connected client,
        ''' echo that data back to the connected client.
        ''' It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Run()

            ' Establish the local endpoint for the socket.
            Dim localEndPoint As System.Net.IPEndPoint = New System.Net.IPEndPoint(System.Net.IPAddress.Any, _LocalPort)
            Call Run(localEndPoint)
        End Sub 'Main

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to
        ''' accept the connection, get data from the connected client,
        ''' echo that data back to the connected client.
        ''' It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub Run(localEndPoint As System.Net.IPEndPoint)

            _LocalPort = localEndPoint.Port

            ' Create a TCP/IP socket.
            _InternalSocketListener = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            '_InternalSocketListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, True)
            ' Bind the socket to the local endpoint and listen for incoming connections.

            Try
                Call _InternalSocketListener.Bind(localEndPoint)
                Call _InternalSocketListener.ReceiveBufferSize.InvokeSet(4096)
                Call _InternalSocketListener.SendBufferSize.InvokeSet(4096)
                Call _InternalSocketListener.Listen(backlog:=100)
            Catch ex As Exception
                Dim exMessage As String =
                    "Exception on try initialize the socket connection local_EndPoint=" & localEndPoint.ToString &
                    vbCrLf &
                    vbCrLf &
                    ex.ToString
                Call Me._InternalExceptionHandle(New Exception(exMessage))
                Throw
            End Try

            'If SelfMapping Then  '端口转发映射设置
            '    Call Console.WriteLine("Self port mapping @wan_port={0} --->@lan_port", _LocalPort)
            '    If Microsoft.VisualBasic.PortMapping.SetPortsMapping(_LocalPort, _LocalPort) = False Then
            '        Call Console.WriteLine("Ports mapping is not successful!")
            '    End If
            'Else
            '    If Not PortMapping < 100 Then
            '        Call Console.WriteLine("Ports mapping wan_port={0}  ----->  lan_port={1}", PortMapping, LocalPort)
            '        If False = SetPortsMapping(PortMapping, _LocalPort) Then
            '            Call Console.WriteLine("Ports mapping is not successful!")
            '        End If
            '    End If
            'End If

            _ThreadEndAccept = True
            running = True

            While Not Me.disposedValue

                If _ThreadEndAccept Then
                    _ThreadEndAccept = False

                    Dim Callback As AsyncCallback = New AsyncCallback(AddressOf AcceptCallback)
                    Call _InternalSocketListener.BeginAccept(Callback, _InternalSocketListener)
                End If

                Call Thread.Sleep(1)
            End While
        End Sub

        Dim running As Boolean = False

        Public Sub WaitForRunning()
            Do While Not running
                Call Thread.Sleep(10)
            Loop
        End Sub

        Public Sub AcceptCallback(ar As IAsyncResult)

            ' Get the socket that handles the client request.
            Dim listener As Socket = CType(ar.AsyncState, Socket)

            ' End the operation.
            Dim handler As Socket

            Try
                handler = listener.EndAccept(ar)
            Catch ex As Exception
                _ThreadEndAccept = True
                Return
            End Try

            ' Create the state object for the async receive.
            Dim state As StateObject = New StateObject With {.workSocket = handler}
            Dim socket As New WorkSocket(state) With
                {
                    .RequestHandler = _InternalRequestHandler,
                    .ExceptionHandle = _InternalExceptionHandle,
                    .ForceCloseHandle = AddressOf Me.ForceCloseHandle
            }
            Try
                Call handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf socket.ReadCallback), state)
                Call AcceptCallbackHandleInvoke()(socket)
                Call _Connections.Add(socket.GetHashCode, socket)
                Call Thread.Sleep(500)
                Call socket.SendMessage(ServicesProtocol.SendServerHash(socket.GetHashCode))

            Catch ex As Exception
                ' 远程强制关闭主机连接，则放弃这一条数据请求的线程
                Call ForceCloseHandle(socket)
            End Try

            _ThreadEndAccept = True
        End Sub 'AcceptCallback

        Protected _Connections As New Dictionary(Of Integer, WorkSocket)

        Public ReadOnly Property Connections As WorkSocket()
            Get
                Try
                    Return Me._Connections.Values.ToArray
                Catch ex As Exception
                    Call Console.WriteLine(ex.ToString)
                    Return New WorkSocket() {}
                End Try
            End Get
        End Property

        Public Delegate Sub AcceptCallbackHandle(socket As WorkSocket)

        Public Property AcceptCallbackHandleInvoke As AcceptCallbackHandle

        Protected Sub ForceCloseHandle(socket As WorkSocket)
            On Error Resume Next
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Connection was force closed by {socket.workSocket.RemoteEndPoint.ToString}, services thread abort!")
            Call Me._Connections.Remove(socket.GetHashCode)
            Call socket.Dispose()
        End Sub

        Public Class WorkSocket : Inherits StateObject

            Public RequestHandler As DataResponseHandler
            Public ExceptionHandle As ExceptionHandler
            Public ForceCloseHandle As TCPExtensions.ForceCloseHandle

            Public ReadOnly ConnectTime As Date = Now

            Public TotalBytes As Double

            Sub New(Socket As StateObject)
                Me.sb = Socket.sb
                Me.buffer = Socket.buffer
                Me.workSocket = Socket.workSocket
            End Sub

            Friend Sub ReadCallback(ar As IAsyncResult)

                Dim content As String = String.Empty
                ' Retrieve the state object and the handler socket
                ' from the asynchronous state object.
                Dim state As StateObject = CType(ar.AsyncState, StateObject)
                Dim handler As Socket = state.workSocket
                ' Read data from the client socket.
                Dim bytesRead As Integer

                Try
                    bytesRead = handler.EndReceive(ar)  '在这里可能发生远程客户端主机强制断开连接，由于已经被断开了，客户端已经放弃了这一次数据请求，所有在这里讲这个请求线程放弃
                Catch ex As Exception
                    Call ForceCloseHandle(Me)
                    Return
                End Try

                If bytesRead > 0 Then '继续读取数据

                    Me.TotalBytes += bytesRead

                    ' There  might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead))
                    ' Check for end-of-file tag. If it is not there, read
                    ' more data.
                    content = state.sb.ToString()

                    Dim p As Integer = InStr(content, Net.TcpSynchronizationServicesSocket.EOF)

                    If p > 0 Then

                        ' All the data has been read from the
                        ' client. Display it on the console.
                        ' Echo the data back to the client.

                        Dim RequestProcessResult As String
                        Dim sw As Stopwatch = Stopwatch.StartNew


                        Dim temp As String = content '.Replace("__<EOF>", "")
#If DEBUG Then
                        Call Console.WriteLine($"  & [DEBUG {NameOf(ReadCallback)}]>>> " & content)
#End If
                        content = Mid(content, 1, p - 1)

                        If Not String.IsNullOrEmpty(content) Then
                            Try

                                Call state.sb.Clear()
                                Call state.sb.Append(Mid(temp, p + Net.TcpSynchronizationServicesSocket.EOF_Mark_Length + 1))
                                Call state.sb.Replace("__<EOF>", "")

                                If String.Equals(content, TCPExtensions.PING_REQUEST) Then
                                    Call Send(handler, TCPExtensions.PING_REQUEST)
                                    Return
                                End If

                                Dim remoteEP = DirectCast(handler.RemoteEndPoint, System.Net.IPEndPoint)
                                RequestProcessResult = RequestHandler(str:=content, RemoteAddress:=remoteEP)
                                'End If

                                'Call _InternalpLogList.Add(New PerformanceLog With {.Request = content, .Ticks = sw.ElapsedMilliseconds, .Time = Now.ToString})
                                Call Send(handler, RequestProcessResult)
                            Catch ex As Exception
                                Call ExceptionHandle(ex)
                                '错误可能是内部处理请求的时候出错了，则将SERVER_INTERNAL_EXCEPTION结果返回给客户端
                                Try
                                    Call Send(handler, data:=Net.TcpSynchronizationServicesSocket.SERVER_INTERNAL_EXCEPTION)
                                Catch ex2 As Exception '这里处理的是可能是强制断开连接的错误
                                    Call ForceCloseHandle(Me)
                                End Try

                            End Try

                        End If

                    Else

                    End If
                End If

                Try
                    If Me.disposedValue Then
                        Return
                    End If
                    Call Thread.Sleep(1)
                    ' Not all data received. Get more.
                    Call handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReadCallback), state)
                Catch ex As Exception
                    Call ForceCloseHandle(Me)
                    Return
                End Try
            End Sub 'ReadCallback

            Public Sub SendMessage(Message As String)
#If DEBUG Then
                Call Console.WriteLine($"&Send(Me.workSocket, Message) => {Message}")
#End If
                Call Send(Me.workSocket, Message & Net.TcpSynchronizationServicesSocket.EOF)
            End Sub

            ''' <summary>
            ''' Server reply the processing result of the request from the client.
            ''' </summary>
            ''' <param name="handler"></param>
            ''' <param name="data"></param>
            ''' <remarks></remarks>
            Private Sub Send(handler As Socket, data As String)

                ' Convert the string data to byte data using ASCII encoding.
                Dim byteData As Byte() = Encoding.ASCII.GetBytes(data)
                ' Begin sending the data to the remote device.
                Call handler.Send(byteData, byteData.Length, SocketFlags.None)
            End Sub 'Send

            Private Sub SendCallback(ar As IAsyncResult)

                ' Retrieve the socket from the state object.
                Dim handler As Socket = CType(ar.AsyncState, Socket)
                ' Complete sending the data to the remote device.
                Dim bytesSent As Integer = handler.EndSend(ar)
                'Console.WriteLine("Sent {0} bytes to client.", bytesSent)
                '  Call handler.Shutdown(SocketShutdown.Both)
                '  Call handler.Close()
            End Sub 'SendCallback

        End Class

#Region "IDisposable Support"

        ''' <summary>
        ''' 退出监听线程所需要的
        ''' </summary>
        ''' <remarks></remarks>
        Private disposedValue As Boolean = False  ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then

                    Call _InternalSocketListener.Dispose()
                    Call _InternalSocketListener.Free()
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.

        ''' <summary>
        ''' Stop the server socket listening threads.(终止服务器Socket监听线程)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace
