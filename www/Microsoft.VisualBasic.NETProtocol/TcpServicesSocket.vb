#Region "Microsoft.VisualBasic::9bfbc9550954786a17bba017682937bc, sciBASIC#\www\Microsoft.VisualBasic.NETProtocol\TcpServicesSocket.vb"

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

    '   Total Lines: 466
    '    Code Lines: 264
    ' Comment Lines: 126
    '   Blank Lines: 76
    '     File Size: 19.35 KB


    '     Class TcpServicesSocket
    ' 
    '         Properties: IsShutdown, LastError, LocalPort, ResponseHandler, Running
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: BeginListen, IsServerInternalException, LoopbackEndPoint, (+2 Overloads) Run, startSocket
    '                   ToString
    ' 
    '         Sub: AcceptCallback, (+2 Overloads) Dispose, ForceCloseHandle, HandleRequest, ReadCallback
    '              Send, WaitForStart
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net
Imports System.Net.Sockets
#If DEBUG Then
Imports System.Reflection
#End If
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Parallel
Imports TcpEndPoint = System.Net.IPEndPoint

Namespace Tcp

    ''' <summary>
    ''' Socket listening object which is running at the server side asynchronous able multiple threading.
    ''' (运行于服务器端上面的Socket监听对象，多线程模型)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TcpServicesSocket
        Implements IDisposable
        Implements ITaskDriver
        Implements IServicesSocket

#Region "INTERNAL FIELDS"

        Dim _threadEndAccept As Boolean = True
        Dim _exceptionHandle As ExceptionHandler
        Dim _maxAccepts As Integer = 4
        Dim _debugMode As Boolean = False

#End Region

        ''' <summary>
        ''' The server services listening on this local port.(当前的这个服务器对象实例所监听的本地端口号)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property LocalPort As Integer Implements IServicesSocket.LocalPort

        ''' <summary>
        ''' This function pointer using for the data request handling of the data request from the client socket.   
        ''' [Public Delegate Function DataResponseHandler(str As <see cref="String"/>, RemoteAddress As <see cref="TcpEndPoint"/>) As <see cref="String"/>]
        ''' (这个函数指针用于处理来自于客户端的请求)
        ''' </summary>
        ''' <remarks></remarks>
        Public Property ResponseHandler As DataRequestHandler Implements IServicesSocket.ResponseHandler
        Public ReadOnly Property Running As Boolean = False Implements IServicesSocket.IsRunning

        Public ReadOnly Property IsShutdown As Boolean Implements IServicesSocket.IsShutdown
            Get
                Return disposedValue
            End Get
        End Property

        Shared ReadOnly defaultHandler As New [Default](Of ExceptionHandler)(AddressOf VBDebugger.PrintException)

        Public ReadOnly Property LastError As String

        ''' <summary>
        ''' 消息处理的方法接口： Public Delegate Function DataResponseHandler(str As String, RemotePort As Integer) As String
        ''' </summary>
        ''' <param name="localPort">监听的本地端口号，假若需要进行端口映射的话，则可以在<see cref="Run"></see>方法之中设置映射的端口号</param>
        ''' <remarks></remarks>
        Sub New(Optional localPort As Integer = 11000,
                Optional exceptionHandler As ExceptionHandler = Nothing,
                Optional debug As Boolean = False)

            Me._LocalPort = localPort
            Me._exceptionHandle = exceptionHandler Or defaultHandler
            Me._debugMode = debug
        End Sub

        ''' <summary>
        ''' 短连接socket服务端
        ''' </summary>
        ''' <param name="requestEventHandler"></param>
        ''' <param name="localPort"></param>
        ''' <param name="exceptionHandler"></param>
        Sub New(requestEventHandler As DataRequestHandler, localPort%,
                Optional exceptionHandler As ExceptionHandler = Nothing,
                Optional debug As Boolean = False)

            Me._ResponseHandler = requestEventHandler
            Me._exceptionHandle = exceptionHandler Or defaultHandler
            Me._LocalPort = localPort
            Me._debugMode = debug
        End Sub

        ''' <summary>
        ''' 函数返回Socket的注销方法
        ''' </summary>
        ''' <param name="requestEventHandler">Public Delegate Function DataResponseHandler(str As String, RemotePort As Integer) As String</param>
        ''' <param name="localPort"></param>
        ''' <param name="exceptionHandler"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function BeginListen(requestEventHandler As DataRequestHandler,
                                           Optional localPort As Integer = 11000,
                                           Optional exceptionHandler As ExceptionHandler = Nothing) As Action

            With New TcpServicesSocket(requestEventHandler, localPort, exceptionHandler)
                Call New Action(AddressOf .Run).BeginInvoke(Nothing, Nothing)
                Return AddressOf .Dispose
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LoopbackEndPoint(Port As Integer) As TcpEndPoint
            Return New TcpEndPoint(System.Net.IPAddress.Loopback, Port)
        End Function

        Public Overrides Function ToString() As String
            Return $"{GetIPAddress()}:{LocalPort}"
        End Function

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to
        ''' accept the connection, get data from the connected client,
        ''' echo that data back to the connected client.
        ''' It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)
        ''' </summary>
        ''' <remarks></remarks>
        Public Function Run() As Integer Implements ITaskDriver.Run, IServicesSocket.Run
            ' Establish the local endpoint for the socket.
            Dim localEndPoint As New TcpEndPoint(System.Net.IPAddress.Any, _LocalPort)
            Return Run(localEndPoint)
        End Function

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to
        ''' accept the connection, get data from the connected client,
        ''' echo that data back to the connected client.
        ''' It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)
        ''' </summary>
        ''' <remarks></remarks>
        Public Function Run(localEndPoint As TcpEndPoint) As Integer Implements IServicesSocket.Run
            Dim callback As AsyncCallback
            Dim exitCode As Integer = 0
            Dim socket As Socket = Nothing

            If _debugMode Then
                Call Console.WriteLine("Start run socket...")
            End If

            For i As Integer = 0 To 10
                ' start or run retry?
                socket = startSocket(localEndPoint)

                ' 20210516 not sure why object is nothing
                ' on unix .NET 5 platform
                If Not socket Is Nothing Then
                    If _debugMode Then
                        Call Console.WriteLine($"Socket initialize success! ({socket.GetHashCode})")
                    End If

                    Exit For
                ElseIf _debugMode Then
                    Call Console.WriteLine("Services socket is nothing, retry...")
                End If
            Next

            While Not Me.disposedValue
                If _threadEndAccept Then
                    _threadEndAccept = False

                    callback = New AsyncCallback(AddressOf AcceptCallback)

                    If socket Is Nothing Then
                        If _debugMode Then
                            Call Console.WriteLine("socket initialize failured!")
                        End If

                        exitCode = -1

                        Exit While
                    End If

                    Try
                        ' Free 之后可能会出现空引用错误，则忽略掉这个错误，退出线程
                        Call socket.BeginAccept(callback, socket)
                    Catch ex As Exception
                        Call App.LogException(ex)
                    End Try
                End If

                Call Thread.Sleep(1)
            End While

            If _debugMode Then
                Call Console.WriteLine("Exit socket loop...")
                Call Console.WriteLine($"status code = {exitCode}")
            End If

            _Running = False

            Call socket.Dispose()

            If _debugMode Then
                Call Console.WriteLine("Release socket done!")
            End If

            Return exitCode
        End Function

        ''' <summary>
        ''' Create a TCP/IP socket.
        ''' </summary>
        ''' <param name="localEndPoint"></param>
        Private Function startSocket(localEndPoint As TcpEndPoint) As Socket
            Dim _servicesSocket As Socket

            If _debugMode Then
                Call Console.WriteLine($"Create socket object, bind to {localEndPoint.ToString}...")
            End If

            _LocalPort = localEndPoint.Port
            _servicesSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

            Try
                ' Bind the socket to the local endpoint and listen
                ' for incoming connections.
                Call _servicesSocket.Bind(localEndPoint)

                _servicesSocket.ReceiveBufferSize = (4096 * 1024 * 10)
                _servicesSocket.SendBufferSize = (4096 * 1024 * 10)

                Call _servicesSocket.Listen(backlog:=128)
            Catch ex As Exception
                Dim exMessage As String =
                    "Exception on try initialize the socket connection local_EndPoint=" & localEndPoint.ToString &
                    vbCrLf &
                    vbCrLf &
                    ex.ToString

                _exceptionHandle(New Exception(exMessage, ex))
                _LastError = $"[{ex.GetType.Name}] {ex.Message}"

                Throw
            Finally
#If DEBUG Then
                Call $"{MethodBase.GetCurrentMethod().GetFullName}  ==> {localEndPoint.ToString}".__DEBUG_ECHO
#End If
            End Try

            _threadEndAccept = True
            _Running = True

            If _debugMode Then
                Call Console.WriteLine(_servicesSocket.LocalEndPoint.ToString)
                Call Console.WriteLine(_servicesSocket.Handle.ToString)
            End If

            Return _servicesSocket
        End Function

        Public Sub WaitForStart()
            Do While Running = False
                Call Thread.Sleep(10)
            Loop
        End Sub

        Public Sub AcceptCallback(ar As IAsyncResult)
            ' Get the socket that handles the client request.
            Dim listener As Socket = DirectCast(ar.AsyncState, Socket)
            ' End the operation.
            Dim handler As Socket

            Try
                handler = listener.EndAccept(ar)
            Catch ex As Exception
                _threadEndAccept = True
                Return
            End Try

            ' Create the state object for the async receive.
            Dim state As New StateObject With {
                .workSocket = handler,
                .received = New MemoryStream
            }

            Try
                Call handler.BeginReceive(state.readBuffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReadCallback), state)
            Catch ex As Exception
                ' 远程强制关闭主机连接，则放弃这一条数据请求的线程
                Call ForceCloseHandle(handler.RemoteEndPoint, ex)
            End Try

            _threadEndAccept = True
        End Sub

        Private Sub ForceCloseHandle(RemoteEndPoint As EndPoint, ex As Exception)
            Call $"Connection was force closed by {RemoteEndPoint.ToString}, services thread abort!".__DEBUG_ECHO
            Call ex.PrintException
        End Sub

        Private Sub ReadCallback(ar As IAsyncResult)
            ' Retrieve the state object and the handler socket
            ' from the asynchronous state object.
            Dim state As StateObject = DirectCast(ar.AsyncState, StateObject)
            Dim handler As Socket = state.workSocket
            ' Read data from the client socket.
            Dim bytesRead As Integer = 0
            Dim closed As Boolean = False

            Try
                ' 在这里可能发生远程客户端主机强制断开连接，由于已经被断开了，
                ' 客户端已经放弃了这一次数据请求，所有在这里将这个请求线程放弃
                bytesRead = handler.EndReceive(ar)
            Catch ex As Exception
                ForceCloseHandle(handler.RemoteEndPoint, ex)
                closed = True
            End Try

            ' 有新的数据
            If bytesRead > 0 Then

                ' There  might be more data, so store the data received so far.
                state.received.Write(state.readBuffer.Takes(bytesRead).ToArray, Scan0, bytesRead)
                ' Check for end-of-file tag. If it is not there, read
                ' more data.
                state.readBuffer = DirectCast(state.received, MemoryStream).ToArray

                ' 得到的是原始的请求数据
                Dim requestData As New RequestStream(state.readBuffer)

                If requestData.FullRead Then
                    Call HandleRequest(handler, requestData)
                Else
                    Try
                        ' Not all data received. Get more.
                        Call handler.BeginReceive(state.readBuffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReadCallback), state)
                    Catch ex As Exception
                        Call ForceCloseHandle(handler.RemoteEndPoint, ex)
                    End Try
                End If
            ElseIf closed Then
                ' 得到的是原始的请求数据
                Dim data As Byte() = DirectCast(state.received, MemoryStream).ToArray
                Dim requestData As New RequestStream(data)

                Call HandleRequest(handler, requestData)
            End If
        End Sub

        ''' <summary>
        ''' All the data has been read from the client. Display it on the console.
        ''' Echo the data back to the client.
        ''' </summary>
        ''' <param name="handler"></param>
        ''' <param name="requestData"></param>
        Private Sub HandleRequest(handler As Socket, requestData As RequestStream)
            ' All the data has been read from the
            ' client. Display it on the console.
            ' Echo the data back to the client.
            Dim remoteEP = DirectCast(handler.RemoteEndPoint, TcpEndPoint)

            Try
                Dim result As BufferPipe

                If requestData.IsPing Then
                    result = New DataPipe(NetResponse.RFC_OK)
                Else
                    result = Me.ResponseHandler()(requestData, remoteEP)
                End If

                Call Send(handler, result)
            Catch ex As Exception
                Call _exceptionHandle(ex)
                ' 错误可能是内部处理请求的时候出错了，则将SERVER_INTERNAL_EXCEPTION结果返回给客户端
                Try
                    Call Send(handler, New DataPipe(NetResponse.RFC_INTERNAL_SERVER_ERROR))
                Catch ex2 As Exception
                    ' 这里处理的是可能是强制断开连接的错误
                    Call _exceptionHandle(ex2)
                End Try
            End Try
        End Sub

        ''' <summary>
        ''' Server reply the processing result of the request from the client.
        ''' </summary>
        ''' <param name="handler"></param>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Private Sub Send(handler As Socket, data As BufferPipe)
            ' Convert the string data to byte data using ASCII encoding.
            For Each byteData As Byte() In data.GetBlocks
                Call handler.Send(byteData)
            Next

            ' Complete sending the data to the remote device.
            Call handler.Shutdown(SocketShutdown.Both)
            Call handler.Close()

            ' release data
            If TypeOf data Is DataPipe Then
                Call DirectCast(data, DataPipe).Dispose()
            ElseIf TypeOf data Is StreamPipe Then
                Call DirectCast(data, StreamPipe).Dispose()
            End If
        End Sub

        ''' <summary>
        ''' SERVER_INTERNAL_EXCEPTION，Server encounter an internal exception during processing
        ''' the data request from the remote device.
        ''' (判断是否服务器在处理客户端的请求的时候，发生了内部错误)
        ''' </summary>
        ''' <param name="replyData"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function IsServerInternalException(replyData As String) As Boolean
            Return String.Equals(replyData, NetResponse.RFC_INTERNAL_SERVER_ERROR.GetUTF8String)
        End Function

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
                    _Running = False
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
