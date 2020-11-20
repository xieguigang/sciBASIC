#Region "Microsoft.VisualBasic::b0584f8ecab066252fddf83e4bcdaf65, Microsoft.VisualBasic.Core\Net\TcpServer.vb"

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

    '     Class StateObject
    ' 
    ' 
    ' 
    '     Class AsynchronousSocketListener
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Properties: LocalPort, ReadLogs, SelfMapping
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '             Function: BeginListen, IsServerInternalException
    ' 
    '             Sub: AcceptCallback, (+2 Overloads) Dispose, ReadCallback, Run, Send
    '                  SendCallback
    '         Structure PerformanceLog
    ' 
    '             Properties: Request, Ticks, Time
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

Namespace DotNET_Wrapper.Tools.TCPSocket

    ''' <summary>
    ''' State object for reading client data asynchronously 
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class StateObject

        ''' <summary>
        ''' Client  socket. 
        ''' </summary>
        ''' <remarks></remarks>
        Public workSocket As Socket
        ''' <summary>
        ''' Size of receive buffer.  
        ''' </summary>
        ''' <remarks></remarks>
        Public Const BufferSize As Integer = 1024
        ''' <summary>
        ''' Receive buffer. 
        ''' </summary>
        ''' <remarks></remarks>
        Public buffer(BufferSize) As Byte
        ''' <summary>
        ''' Received data string. 
        ''' </summary>
        ''' <remarks></remarks>
        Public sb As New StringBuilder

    End Class

    Public Class AsynchronousSocketListener
        Implements System.IDisposable

        ''' <summary>
        ''' Thread signal. 
        ''' </summary>
        ''' <remarks></remarks>
        Dim allDone As New ManualResetEvent(False)

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

        ''' <summary>
        ''' 消息处理的方法
        ''' </summary>
        ''' <param name="str">远程设备所发送过来的消息</param>
        ''' <param name="RemoteAddress">远程设备的网络连接参数</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Delegate Function DataResponseHandler(str As String, RemoteAddress As IPEndPoint) As String

        ''' <summary>
        ''' 当前的这个服务器对象实例所监听的本地端口号
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property LocalPort As Integer
            Get
                Return _LocalPort
            End Get
        End Property

        Public Structure PerformanceLog
            Public Property Time As Date
            Public Property Request As String

            ''' <summary>
            ''' 服务器完成一个请求的所需要花费的时间的长短，使用这个参数可以估计一段时间内的服务器的处理压力，压力越大，则某一个请求可能会需要花费更加长的时间来处理
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property Ticks As Double
        End Structure

        Dim _InternalExceptionHandle As AsynchronousClient.ExceptionHandler
        Dim _InternalpLogList As New List(Of PerformanceLog)
        Dim _InternalSocketListener As Socket

        ''' <summary>
        ''' 消息处理的方法接口： Public Delegate Function DataResponseHandler(str As String, RemotePort As Integer) As String
        ''' </summary>
        ''' <param name="DataArrivalEventHandler"></param>
        ''' <param name="LocalPort">监听的本地端口号，假若需要进行端口映射的话，则可以在<see cref="Run"></see>方法之中设置映射的端口号</param>
        ''' <remarks></remarks>
        Sub New(DataArrivalEventHandler As DataResponseHandler,
                Optional LocalPort As Integer = 11000,
                Optional exHandler As AsynchronousClient.ExceptionHandler = Nothing)

            Me._InternalRequestHandler = DataArrivalEventHandler
            Me._LocalPort = LocalPort
            Me._InternalExceptionHandle = If(exHandler Is Nothing, Sub(ex As Exception) Call Console.WriteLine(ex.ToString), exHandler)
        End Sub

        ''' <summary>
        ''' 函数返回Socket的注销方法
        ''' </summary>
        ''' <param name="DataArrivalEventHandler"></param>
        ''' <param name="LocalPort"></param>
        ''' <param name="exHandler"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function BeginListen(DataArrivalEventHandler As DataResponseHandler,
                                           Optional LocalPort As Integer = 11000,
                                           Optional exHandler As AsynchronousClient.ExceptionHandler = Nothing) As Action
            Dim Socket As New AsynchronousSocketListener(DataArrivalEventHandler, LocalPort, exHandler)
            Call (Sub() Call Socket.Run()).BeginInvoke(Nothing, Nothing)
            Return AddressOf Socket.Dispose
        End Function

        ''' <summary>
        ''' （全局变量）是否对自身进行在外网和内网之间进行端口映射
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Property SelfMapping As Boolean

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to  
        ''' accept the connection, get data from the connected client, 
        ''' echo that data back to the connected client.   
        ''' It then disconnects from the client and waits for another client.  
        ''' </summary>
        ''' <param name="PortMapping">是否需要对端口进行映射处理，假若值为小于100的数值则说明不需要</param>
        ''' <remarks></remarks>
        Public Sub Run(Optional PortMapping As Integer = -1)

            ' Data buffer for incoming data.     
            Dim bytes() As Byte = New [Byte](1023) {}
            ' Establish the local endpoint for the socket.    
            Dim ipHostInfo As IPHostEntry = Dns.Resolve(Dns.GetHostName())
            Dim ipAddress As IPAddress = ipHostInfo.AddressList(0)
            Dim localEndPoint As IPEndPoint = New IPEndPoint(Net.IPAddress.Any, _LocalPort)

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
                Call allDone.Reset()
                ' Start an asynchronous socket to listen for connections.     
                Call Console.WriteLine("[{0}] Server socket(local_port:={1}) waiting for a connection...", Now.ToString, Me._LocalPort)
                Call _InternalSocketListener.BeginAccept(New AsyncCallback(AddressOf AcceptCallback), _InternalSocketListener)
                ' Wait until a connection is made and processed before continuing.        
                Call allDone.WaitOne()
            End While
        End Sub 'Main 

        ''' <summary>
        ''' 返回性能计数器的数据并清空缓存数据
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ReadLogs As PerformanceLog()
            Get
                Dim ChunkBuffer = _InternalpLogList.ToArray
                Call _InternalpLogList.Clear()
                Return ChunkBuffer
            End Get
        End Property

        Public Sub AcceptCallback(ar As IAsyncResult)

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

        Public Const EOF As String = "<EOF>"

        Public Sub ReadCallback(ar As IAsyncResult)

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
                        RequestProcessResult = _InternalRequestHandler(str:=content.Replace(EOF, ""), RemoteAddress:=DirectCast(handler.RemoteEndPoint, IPEndPoint))

                        Call _InternalpLogList.Add(New PerformanceLog With {.Request = content, .Ticks = sw.ElapsedMilliseconds, .Time = Now.ToString})
                        Call Send(handler, RequestProcessResult)
                    Catch ex As Exception
                        Call _InternalExceptionHandle(ex)
                        Call Send(handler, data:=AsynchronousSocketListener.SERVER_INTERNAL_EXCEPTION)
                    End Try
                Else
                    ' Not all data received. Get more.     
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReadCallback), state)
                End If
            End If
        End Sub 'ReadCallback   

        ''' <summary>
        ''' 服务器在处理请求的时候发生内部错误
        ''' </summary>
        ''' <remarks></remarks>
        Public Const SERVER_INTERNAL_EXCEPTION As String = "SERVER_INTERNAL_EXCEPTION"

        ''' <summary>
        ''' SERVER_INTERNAL_EXCEPTION，服务器在处理客户端的请求的时候，发生了内部错误
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
            allDone.Set()
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

        ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.

        ''' <summary>
        ''' 终止服务器Socket监听线程
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
