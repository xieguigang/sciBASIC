#Region "Microsoft.VisualBasic::b3c16530c6ece0cbc005818fb9d6f240, www\Microsoft.VisualBasic.NETProtocol\TcpRequest\TcpRequest.vb"

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

    '   Total Lines: 397
    '    Code Lines: 202 (50.88%)
    ' Comment Lines: 137 (34.51%)
    '    - Xml Docs: 67.88%
    ' 
    '   Blank Lines: 58 (14.61%)
    '     File Size: 16.07 KB


    '     Class TcpRequest
    ' 
    '         Constructor: (+5 Overloads) Sub New
    ' 
    '         Function: doWait, getSocket, LocalConnection, (+4 Overloads) SendMessage, SetTimeOut
    '                   ToString
    ' 
    '         Sub: ConnectCallback, (+2 Overloads) Dispose, doSend, Receive, ReceiveCallback
    '              RequestToStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Parallel
Imports IPEndPoint = Microsoft.VisualBasic.Net.IPEndPoint
Imports TcpEndPoint = System.Net.IPEndPoint

Namespace Tcp

    ''' <summary>
    ''' The server socket should returns some data string to this client or this client 
    ''' will stuck at the <see cref="SendMessage"></see> function.
    ''' (服务器端``TcpServicesSocket``必须要返回数据， 
    ''' 否则本客户端会在<see cref="SendMessage"></see>函数位置一直处于等待的状态)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TcpRequest : Implements IDisposable

#Region "Internal Fields"

        ''' <summary>
        ''' The port number for the remote device.
        ''' </summary>
        ''' <remarks></remarks>
        Dim port As Integer

        ''' <summary>
        ''' ' ManualResetEvent instances signal completion.
        ''' </summary>
        ''' <remarks></remarks>
        Dim connectDone As ManualResetEvent
        Dim sendDone As ManualResetEvent
        Dim receiveDone As ManualResetEvent
        Dim exceptionHandler As ExceptionHandler
        Dim remoteHost As String

        ''' <summary>
        ''' A System.TimeSpan that represents the number of milliseconds to wait, or a System.TimeSpan
        ''' that represents -1 milliseconds to wait indefinitely.
        ''' </summary>
        Dim timeout As TimeSpan = TimeSpan.FromSeconds(60)

        ''' <summary>
        ''' Remote End Point
        ''' </summary>
        ''' <remarks></remarks>
        Protected ReadOnly remoteEP As TcpEndPoint
#End Region

        Public Function SetTimeOut(timespan As TimeSpan) As TcpRequest
            Me.timeout = timespan
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return $"Remote_connection={remoteHost}:{port},  local_host={LocalIPAddress}"
        End Function

        Sub New(localPort As Integer)
            Call Me.New(New IPEndPoint("127.0.0.1", localPort))
        End Sub

        Sub New(remoteDevice As TcpEndPoint, Optional exceptionHandler As ExceptionHandler = Nothing)
            Call Me.New(remoteDevice.Address.ToString, remoteDevice.Port, exceptionHandler)
        End Sub

        Sub New(remoteDevice As IPEndPoint, Optional exceptionHandler As ExceptionHandler = Nothing)
            Call Me.New(remoteDevice.ipAddress, remoteDevice.port, exceptionHandler)
        End Sub

        Shared ReadOnly defaultHandler As New [Default](Of ExceptionHandler)(AddressOf VBDebugger.PrintException)

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="client">
        ''' Copy the TCP client connection profile data from this object.
        ''' (从本客户端对象之中复制出连接配置参数以进行初始化操作)
        ''' </param>
        ''' <param name="exceptionHandler"></param>
        ''' <remarks></remarks>
        Sub New(client As TcpRequest, Optional exceptionHandler As ExceptionHandler = Nothing)
            Me.remoteHost = client.remoteHost
            Me.port = client.port
            Me.exceptionHandler = exceptionHandler Or defaultHandler
            Me.remoteEP = New TcpEndPoint(IPAddress.Parse(remoteHost), port)
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="remotePort"></param>
        ''' <param name="exceptionHandler">
        ''' Public <see cref="System.Delegate"/> Sub ExceptionHandler(ex As <see cref="Exception"/>)
        ''' </param>
        ''' <remarks></remarks>
        Sub New(hostName$, remotePort%, Optional exceptionHandler As ExceptionHandler = Nothing)
            remoteHost = hostName

            If String.Equals(remoteHost, "localhost", StringComparison.OrdinalIgnoreCase) Then
                remoteHost = "127.0.0.1" ' LocalIPAddress
            End If

            Me.port = remotePort
            Me.exceptionHandler = exceptionHandler Or defaultHandler
            Me.remoteEP = New TcpEndPoint(IPAddress.Parse(remoteHost), port)
        End Sub

        ''' <summary>
        ''' 初始化一个在本机进行进程间通信的Socket对象
        ''' </summary>
        ''' <param name="localPort"></param>
        ''' <param name="exceptionHandler"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LocalConnection(localPort%, Optional exceptionHandler As ExceptionHandler = Nothing) As TcpRequest
            Return New TcpRequest(LocalIPAddress, localPort, exceptionHandler)
        End Function

        Public Function SendMessage(Message As String, Callback As Action(Of String)) As IAsyncResult
            Dim SendMessageClient As New TcpRequest(Me, exceptionHandler:=Me.exceptionHandler)
            Return (Sub() Call Callback(SendMessageClient.SendMessage(Message))).BeginInvoke(Nothing, Nothing)
        End Function

        ''' <summary>
        ''' This function returns the server reply for this request <paramref name="Message"></paramref>.
        ''' </summary>
        ''' <param name="Message">The client request to the server.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SendMessage(Message As String) As String
            Dim byteData As Byte() = Encoding.UTF8.GetBytes(Message)
            byteData = SendMessage(byteData)
            Dim response As String = New RequestStream(byteData).GetUTF8String
            Return response
        End Function

        ''' <summary>
        ''' Send a request message to the remote server.
        ''' </summary>
        ''' <param name="message"></param>
        ''' <returns></returns>
        Public Function SendMessage(message As RequestStream) As RequestStream
            Dim byteData As Byte() = SendMessage(message.Serialize)
            Dim stream As RequestStream

            If RequestStream.IsAvaliableStream(byteData) Then
                stream = New RequestStream(byteData)
            Else
                stream = New RequestStream(0, 0, byteData)
            End If

            If ZipDataPipe.TestBufferMagic(stream.ChunkBuffer) Then
                stream.ChunkBuffer = ZipDataPipe.UncompressBuffer(stream.ChunkBuffer)
            End If

            Return stream
        End Function

        Public Sub RequestToStream(message As RequestStream, stream As Stream)
            Dim client As Socket = getSocket(message.Serialize)

            ' Receive the response from the remote device.
            Call Receive(client, stream)
            Call receiveDone.WaitOne()

            On Error Resume Next

            ' Release the socket.
            Call client.Shutdown(SocketShutdown.Both)
            Call client.Close()
        End Sub

        ''' <summary>
        ''' Blocks the current thread until the current instance receives a signal, using
        ''' a System.TimeSpan to specify the time interval.
        ''' </summary>
        ''' <param name="handler"></param>
        ''' <returns>true if the current instance receives a signal; otherwise, false.</returns>
        Private Function doWait(handler As ManualResetEvent) As Boolean
            If timeout.TotalMilliseconds > 0 Then
                Return handler.WaitOne(timeout)
            Else
                Return handler.WaitOne
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="message"></param>
        ''' <returns>
        ''' returns nothing means timeout
        ''' </returns>
        Private Function getSocket(message As Byte()) As Socket
            ' ManualResetEvent instances signal completion.
            connectDone = New ManualResetEvent(False)
            sendDone = New ManualResetEvent(False)
            receiveDone = New ManualResetEvent(False)

            ' Establish the remote endpoint for the socket.
            ' For this example use local machine.
            ' Create a TCP/IP socket.
            Dim client As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

            Call client.Bind(New TcpEndPoint(IPAddress.Any, 0))
            ' Connect to the remote endpoint.
            Call client.BeginConnect(remoteEP, New AsyncCallback(AddressOf ConnectCallback), client)

            ' Wait for connect.
            If Not doWait(connectDone) Then
                Return Nothing
            End If

            ' Send test data to the remote device.
            Call doSend(client, message)

            If Not doWait(sendDone) Then
                Return Nothing
            Else
                Return client
            End If
        End Function

        ''' <summary>
        ''' 最底层的消息发送函数
        ''' </summary>
        ''' <param name="message"></param>
        ''' <returns></returns>
        Public Function SendMessage(message As Byte()) As Byte()
            If Not RequestStream.IsAvaliableStream(message) Then
                message = New RequestStream(0, 0, message).Serialize
            End If

            Dim client As Socket = getSocket(message)
            Dim buffer As New MemoryStream

            If client Is Nothing Then
                ' operation timeout
                Return New RequestStream(-1, 500, "send message timeout").Serialize
            End If

            ' Receive the response from the remote device.
            Call Receive(client, buffer)

            If Not doWait(receiveDone) Then
                ' get part of the data package?
                If buffer.Length = 0 Then
                    Return New RequestStream(-1, 500, "receive message timeout").Serialize
                End If
            End If

            On Error Resume Next

            ' Release the socket.
            Call client.Shutdown(SocketShutdown.Both)
            Call client.Close()

            Return buffer.ToArray
        End Function

        Private Sub ConnectCallback(ar As IAsyncResult)
            ' Retrieve the socket from the state object.
            Dim client As Socket = DirectCast(ar.AsyncState, Socket)

            ' Complete the connection.
            Try
                client.EndConnect(ar)
                ' Signal that the connection has been made.
                connectDone.Set()
            Catch ex As Exception
                Call exceptionHandler(ex)
            End Try
        End Sub

        ''' <summary>
        ''' An exception of type '<see cref="SocketException"/>' occurred in System.dll but was not handled in user code
        ''' Additional information: A request to send or receive data was disallowed because the socket is not connected and
        ''' (when sending on a datagram socket using a sendto call) no address was supplied
        ''' </summary>
        ''' <param name="client"></param>
        Private Sub Receive(client As Socket, buffer As Stream)
            ' Create the state object.
            Dim state As New StateObject With {
                .workSocket = client,
                .received = buffer
            }

            ' Begin receiving the data from the remote device.
            Try
                Call client.BeginReceive(state.readBuffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReceiveCallback), state)
            Catch ex As Exception
                Call Me.exceptionHandler(ex)
            End Try
        End Sub

        ''' <summary>
        ''' Retrieve the state object and the client socket from the 
        ''' asynchronous state object.
        ''' </summary>
        ''' <param name="ar"></param>
        Private Sub ReceiveCallback(ar As IAsyncResult)
            Dim state As StateObject = DirectCast(ar.AsyncState, StateObject)
            Dim client As Socket = state.workSocket
            Dim bytesRead As Integer

            Try
                ' Read data from the remote device.
                bytesRead = client.EndReceive(ar)
            Catch ex As Exception
                Call exceptionHandler(ex)
                GoTo EX_EXIT
            End Try

            If bytesRead > 0 Then
                ' There might be more data, so store the data received so far.
                state.received.Write(state.readBuffer.Takes(bytesRead).ToArray, Scan0, bytesRead)
                ' Get the rest of the data.
                client.BeginReceive(state.readBuffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReceiveCallback), state)
            Else
                ' All the data has arrived; put it in response.
EX_EXIT:
                ' Signal that all bytes have been received.
                Call receiveDone.Set()
            End If
        End Sub

        ''' <summary>
        ''' ????
        ''' An exception of type 'System.Net.Sockets.SocketException' occurred in System.dll but was not handled in user code
        ''' Additional information: A request to send or receive data was disallowed because the socket is not connected and
        ''' (when sending on a datagram socket using a sendto call) no address was supplied
        ''' </summary>
        ''' <param name="client"></param>
        ''' <param name="byteData"></param>
        ''' <remarks></remarks>
        Private Sub doSend(client As Socket, byteData As Byte())
            ' Begin sending the data to the remote device.
            Try
                ' For Each block As Byte() In byteData.Split(512)
                ' Call client.Send(block, socketFlags:=SocketFlags.None)
                ' Next
                Call client.Send(byteData, socketFlags:=SocketFlags.None)

            Catch ex As Exception
                Call Me.exceptionHandler(ex)
            Finally
                ' Signal that all bytes have been sent.
                Call sendDone.Set()
            End Try
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call connectDone.Set()  ' ManualResetEvent instances signal completion.
                    Call sendDone.Set()
                    Call receiveDone.Set() '中断服务器的连接
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
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace
