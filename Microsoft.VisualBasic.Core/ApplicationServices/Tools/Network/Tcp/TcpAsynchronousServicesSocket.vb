#Region "Microsoft.VisualBasic::3edeb3097882714663b70c353bbe36be, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Tools\Network\Tcp\TcpAsynchronousServicesSocket.vb"

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

Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading

Imports Microsoft.VisualBasic

Namespace WrapperClassTools.Net

    ''' <summary>
    ''' Socket listening object which is running at the server side asynchronous able multiple threading.
    ''' (运行于服务器端上面的Socket监听对象，单线程模型)
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class TcpAsynchronousServicesSocket
        Implements System.IDisposable

#Region "INTERNAL FIELDS"

        ''' <summary>
        ''' Thread signal.
        ''' </summary>
        ''' <remarks></remarks>
        Dim _InternalThreadSignalAllDone As New ManualResetEvent(False)

        ''' <summary>
        ''' 这个函数指针用于处理来自于客户端的请求
        ''' </summary>
        ''' <remarks></remarks>
        Dim _InternalRequestHandler As DataResponseHandler

        ''' <summary>
        ''' Socket对象监听的端口号
        ''' </summary>
        ''' <remarks></remarks>
        Dim _LocalPort As Integer
        Dim _InternalExceptionHandle As ExceptionHandler
        'Dim _InternalpLogList As New List(Of PerformanceLog)
        Dim _InternalSocketListener As Socket

#End Region

        ''' <summary>
        ''' The server services listening on this local port.(当前的这个服务器对象实例所监听的本地端口号)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property LocalPort As Integer
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
            Dim Socket As New TcpAsynchronousServicesSocket(DataArrivalEventHandler, LocalPort, exHandler)
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
        Public Sub Run(localEndPoint As System.Net.IPEndPoint)

            _LocalPort = localEndPoint.Port

            ' Create a TCP/IP socket.
            _InternalSocketListener = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            '_InternalSocketListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, True)
            ' Bind the socket to the local endpoint and listen for incoming connections.

            Try
                Call _InternalSocketListener.Bind(localEndPoint)
                Call _InternalSocketListener.ReceiveBufferSize.SetValueMethod(4096)
                Call _InternalSocketListener.SendBufferSize.SetValueMethod(4096)
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

            While Not Me.disposedValue

                ' Set the event to nonsignaled state.
                Call _InternalThreadSignalAllDone.Reset()
                ' Start an asynchronous socket to listen for connections.
                'Call Console.WriteLine("[{0}] Server socket(local_port:={1}) waiting for a connection...", Now.ToString, Me._LocalPort)
                Call _InternalSocketListener.BeginAccept(New AsyncCallback(AddressOf AcceptCallback), _InternalSocketListener)
                ' Wait until a connection is made and processed before continuing.
                Call _InternalThreadSignalAllDone.WaitOne()
            End While
        End Sub

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to
        ''' accept the connection, get data from the connected client,
        ''' echo that data back to the connected client.
        ''' It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)
        ''' </summary>
        ''' <param name="PortMapping">是否需要对端口进行映射处理，假若值为小于100的数值则说明不需要</param>
        ''' <remarks></remarks>
        Public Sub Run(Optional PortMapping As Integer = -1)

            ' Establish the local endpoint for the socket.
            Dim localEndPoint As IPEndPoint = New IPEndPoint(System.Net.IPAddress.Any, _LocalPort)
            Call Run(localEndPoint)
        End Sub 'Main

        ' ''' <summary>
        ' ''' Returns the services request logs data and clear the buffer cache.(返回性能计数器的数据并清空缓存数据)
        ' ''' </summary>
        ' ''' <returns></returns>
        ' ''' <remarks></remarks>
        'Public Function ReadLogs() As PerformanceLog()
        '    Dim ChunkBuffer = _InternalpLogList.ToArray
        '    Call _InternalpLogList.Clear()
        '    Return ChunkBuffer
        'End Function

        Private Sub AcceptCallback(ar As IAsyncResult)

            ' Get the socket that handles the client request.
            Dim listener As Socket = CType(ar.AsyncState, Socket)

            If disposedValue = True Then
                Return
            End If

            ' End the operation.
            Dim handler As Socket = listener.EndAccept(ar)
            ' Create the state object for the async receive.
            Dim state As New StateObject
            state.workSocket = handler
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReadCallback), state)
        End Sub 'AcceptCallback

        Private Sub ReadCallback(ar As IAsyncResult)

            Dim content As String = String.Empty
            ' Retrieve the state object and the handler socket
            ' from the asynchronous state object.
            Dim state As StateObject = CType(ar.AsyncState, StateObject)
            Dim handler As Socket = state.workSocket
            ' Read data from the client socket.
            Dim bytesRead As Integer = handler.EndReceive(ar)
            If bytesRead > 0 Then

                ' There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead))
                ' Check for end-of-file tag. If it is not there, read
                ' more data.
                content = state.sb.ToString()
                If content.IndexOf(EOF) > -1 Then

                    ' All the data has been read from the
                    ' client. Display it on the console.
                    ' Echo the data back to the client.

                    Dim RequestProcessResult As String
                    Dim sw As Stopwatch = Stopwatch.StartNew

                    Try
                        content = Mid(content, 1, Len(content) - 5)

                        'If String.Equals(content, Ping.PING_REQUEST) Then
                        'RequestProcessResult = New Ping() With {.ServerLoad = ProcessUsage()}.GetXml
                        'Else
                        Dim remoteEP = DirectCast(handler.RemoteEndPoint, IPEndPoint)
                        RequestProcessResult = _InternalRequestHandler(str:=content, RemoteAddress:=remoteEP)
                        'End If

                        'Call _InternalpLogList.Add(New PerformanceLog With {.Request = content, .Ticks = sw.ElapsedMilliseconds, .Time = Now.ToString})
                        Call Send(handler, RequestProcessResult)
                    Catch ex As Exception
                        Call _InternalExceptionHandle(ex)
                        Call Send(handler, data:=TcpAsynchronousServicesSocket.SERVER_INTERNAL_EXCEPTION)
                    End Try
                Else
                    ' Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReadCallback), state)
                End If
            End If
        End Sub 'ReadCallback

#Region "PUBLIC CONSTANT"

        ''' <summary>
        ''' Server encounter an internal exception during processing the data request from the remote device.
        ''' (服务器在处理外部远程设备的数据请求的时候发生内部错误)
        ''' </summary>
        ''' <remarks></remarks>
        Public Const SERVER_INTERNAL_EXCEPTION As String = "SERVER_INTERNAL_EXCEPTION"
        ''' <summary>
        ''' 字符串流串的结束标识符
        ''' </summary>
        ''' <remarks></remarks>
        Public Const EOF As String = "<EOF>"
#End Region

        ''' <summary>
        ''' SERVER_INTERNAL_EXCEPTION，Server encounter an internal exception during processing
        ''' the data request from the remote device.
        ''' (判断是否服务器在处理客户端的请求的时候，发生了内部错误)
        ''' </summary>
        ''' <param name="replyData"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function IsServerInternalException(replyData As String) As Boolean
            Return String.Equals(replyData, SERVER_INTERNAL_EXCEPTION)
        End Function

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
            handler.BeginSend(byteData, 0, byteData.Length, 0, New AsyncCallback(AddressOf SendCallback), handler)
        End Sub 'Send

        Private Sub SendCallback(ar As IAsyncResult)

            ' Retrieve the socket from the state object.
            Dim handler As Socket = CType(ar.AsyncState, Socket)
            ' Complete sending the data to the remote device.
            Dim bytesSent As Integer = handler.EndSend(ar)
            'Console.WriteLine("Sent {0} bytes to client.", bytesSent)
            handler.Shutdown(SocketShutdown.Both)
            handler.Close()
            ' Signal the main thread to continue.
            Call _InternalThreadSignalAllDone.Set()
        End Sub 'SendCallback

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

    End Class 'AsynchronousSocketListener
End Namespace
