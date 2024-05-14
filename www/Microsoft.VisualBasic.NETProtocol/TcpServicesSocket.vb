#Region "Microsoft.VisualBasic::f6e277b662129bd97eb96b93cfb1045c, www\Microsoft.VisualBasic.NETProtocol\TcpServicesSocket.vb"

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

    '   Total Lines: 352
    '    Code Lines: 192
    ' Comment Lines: 103
    '   Blank Lines: 57
    '     File Size: 14.27 KB


    '     Class TcpServicesSocket
    ' 
    '         Properties: IsShutdown, LastError, LocalPort, ResponseHandler, Running
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: BeginListen, IsServerInternalException, LoopbackEndPoint, (+2 Overloads) Run, ToString
    ' 
    '         Sub: accept, acceptWorker, (+2 Overloads) Dispose, ForceCloseHandle, HandleRequest
    '              Send, WaitForStart
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language.Default
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

        ''' <summary>
        ''' 处理连接的线程池
        ''' </summary>
        ReadOnly _threadPool As New Threads.ThreadPool(8)

        Dim _exceptionHandle As ExceptionHandler
        Dim _maxAccepts As Integer = 4
        Dim _debugMode As Boolean = False
        Dim _socket As TcpListener

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
            Dim exitCode As Integer = 0

            If _debugMode Then
                Call VBDebugger.EchoLine("Start run socket...")
            End If

            Me._socket = New TcpListener(IPAddress.Any, localEndPoint.Port)
            Me._socket.Start(10240)
            Me._threadPool.Start()

            _Running = True

            While Not Me.disposedValue AndAlso Running
                If Not _threadPool.FullCapacity Then
                    Call _threadPool.RunTask(AddressOf accept)
                Else
                    Thread.Sleep(1)
                End If
            End While

            If _debugMode Then
                Call Console.WriteLine("Exit socket loop...")
                Call Console.WriteLine($"status code = {exitCode}")
            End If

            _Running = False

            If _debugMode Then
                Call Console.WriteLine("Release socket done!")
            End If

            Return exitCode
        End Function

        ''' <summary>
        ''' processing the request data
        ''' </summary>
        Private Sub accept()
            Try
                Call acceptWorker()
            Catch ex As Exception
                Call App.LogException(ex)
            End Try
        End Sub

        Private Sub acceptWorker()
            Dim s As TcpClient = _socket.AcceptTcpClient
            Dim request As New BufferedStream(s.GetStream)
            Dim response As Stream = s.GetStream()
            Dim received As New MemoryStream
            Dim chunk As Byte() = New Byte(4096 - 1) {}

            Do While Running
                Dim nreads As Integer = request.Read(chunk, Scan0, chunk.Length)

                If nreads > 0 Then
                    received.Write(chunk, Scan0, nreads)
                End If

                ' has no data reads
                ' start to processing the request 
                Dim requestData As New RequestStream(received.ToArray)

                If requestData.FullRead Then
                    Call HandleRequest(s.Client.RemoteEndPoint, response, requestData)
                    Exit Do
                Else

                End If

                Call Thread.Sleep(1)
            Loop

            Call response.Flush()
            Call response.Dispose()
            Call s.Close()
        End Sub

        Public Sub WaitForStart()
            Do While Running = False
                Call Thread.Sleep(10)
            Loop
        End Sub

        Private Sub ForceCloseHandle(RemoteEndPoint As EndPoint, ex As Exception)
            Call $"Connection was force closed by {RemoteEndPoint.ToString}, services thread abort!".__DEBUG_ECHO
            Call ex.PrintException
        End Sub

        ''' <summary>
        ''' All the data has been read from the client. Display it on the console.
        ''' Echo the data back to the client.
        ''' </summary>
        ''' <param name="remote"></param>
        ''' <param name="requestData"></param>
        Private Sub HandleRequest(remote As TcpEndPoint, response As Stream, requestData As RequestStream)
            Try
                Dim result As BufferPipe

                If requestData.IsPing Then
                    result = New DataPipe(NetResponse.RFC_OK)
                Else
                    result = Me.ResponseHandler()(requestData, remote)
                End If

                Call Send(response, result)
            Catch ex As Exception
                Call _exceptionHandle(ex)
                ' 错误可能是内部处理请求的时候出错了，则将SERVER_INTERNAL_EXCEPTION结果返回给客户端
                Try
                    Call Send(response, New DataPipe(NetResponse.RFC_INTERNAL_SERVER_ERROR))
                Catch ex2 As Exception
                    ' 这里处理的是可能是强制断开连接的错误
                    Call _exceptionHandle(ex2)
                End Try
            End Try
        End Sub

        ''' <summary>
        ''' Server reply the processing result of the request from the client.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Private Sub Send(response As Stream, data As BufferPipe)
            Call VBDebugger.EchoLine($"send stream: {data}")

            ' Convert the string data to byte data using ASCII encoding.
            For Each byteData As Byte() In data.GetBlocks
                For Each block As Byte() In byteData.Split(4096)
                    Call response.Write(block, Scan0, block.Length)
                    Call response.Flush()
                Next
            Next

            Call response.Flush()

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
                    Try
                        Call _threadPool.Dispose()
                        Call _socket.Stop()
                    Catch ex As Exception

                    End Try
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
