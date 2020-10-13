#Region "Microsoft.VisualBasic::6bbf1c6413415d517ef1517de25bfa27, www\Microsoft.VisualBasic.NETProtocol\Persistent\TcpClient.vb"

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

    '     Class AsynchronousClient
    ' 
    '         Properties: MyLocalPort, RemoteServerShutdown, ServerHash
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: GetMyWANIP, LocalClient, ToString
    ' 
    '         Sub: BeginConnect, ConnectCallback, (+2 Overloads) Dispose, InternalSend, Receive
    '              ReceiveCallback, WaitForConnected, WaitForHash
    '         Class StateObject
    ' 
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
Imports System.Threading
Imports System.Text
Imports System.Text.RegularExpressions

Imports Microsoft.VisualBasic.WrapperClassTools.Net.AsynchronousClient

Namespace WrapperClassTools.Net.PersistentConnection

    ''' <summary>
    ''' 请注意，这个对象是应用于客户端与服务器保持长连接所使用，并不会主动发送消息给服务器，而是被动的接受服务器的数据请求
    ''' </summary>
    ''' <remarks></remarks>
    Public Class AsynchronousClient : Implements System.IDisposable

#Region "Internal Fields"

        ''' <summary>
        ''' The port number for the remote device.  
        ''' </summary>
        ''' <remarks></remarks>
        Protected port As Integer

        Dim InternalExceptionHandler As ExceptionHandler
        Protected remoteHost As String

        ''' <summary>
        ''' Remote End Point
        ''' </summary>
        ''' <remarks></remarks>
        Protected ReadOnly remoteEP As System.Net.IPEndPoint
#End Region

        ' ''' <summary>
        ' ''' 
        ' ''' </summary>
        ' ''' <param name="TimeOut">超时的时间长度默认为30秒</param>
        ' ''' <returns></returns>
        ' ''' <remarks></remarks>
        'Public Function Ping(Optional TimeOut As Integer = 30 * 1000) As Ping
        '    Dim Sw As Stopwatch = Stopwatch.StartNew
        '    Dim Response As String = SendMessage(TCPExtensions.Ping.PING_REQUEST, TimeOut)
        '    If OperationTimeOut(Response) Then
        '        Return New Ping With {.PingTime = -1, .ServerLoad = 100}
        '    Else
        '        Dim Result = Response.CreateObjectFromXml(Of Ping)().ServerLoad
        '        Return New Ping With {.PingTime = Sw.ElapsedMilliseconds, .ServerLoad = Result}
        '    End If
        'End Function

        Public Overrides Function ToString() As String
            Return String.Format("Remote_connection={0}:{1},  local_host={2}", remoteHost, port, LocalIPAddress)
        End Function

        Sub New(remoteDevice As System.Net.IPEndPoint, Optional ExceptionHandler As ExceptionHandler = Nothing)
            Call Me.New(remoteDevice.Address.ToString, remoteDevice.Port, ExceptionHandler)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Client">Copy the TCP client connection profile data from this object.(从本客户端对象之中复制出连接配置参数以进行初始化操作)</param>
        ''' <param name="ExceptionHandler"></param>
        ''' <remarks></remarks>
        Sub New(Client As AsynchronousClient, Optional ExceptionHandler As ExceptionHandler = Nothing)
            remoteHost = Client.remoteHost
            port = Client.port
            InternalExceptionHandler = If(ExceptionHandler Is Nothing, Sub(ex As Exception) Call Console.WriteLine(ex.ToString), ExceptionHandler)
            remoteEP = New System.Net.IPEndPoint(IPAddress.Parse(remoteHost), port)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="RemotePort"></param>
        ''' <param name="ExceptionHandler">Public Delegate Sub ExceptionHandler(ex As Exception)</param>
        ''' <remarks></remarks>
        Sub New(HostName As String, RemotePort As Integer, Optional ExceptionHandler As ExceptionHandler = Nothing)
            remoteHost = HostName

            If String.Equals(remoteHost, "localhost", StringComparison.OrdinalIgnoreCase) Then
                remoteHost = LocalIPAddress
            End If

            port = RemotePort
            InternalExceptionHandler = If(ExceptionHandler Is Nothing, Sub(ex As Exception) Call Console.WriteLine(ex.ToString), ExceptionHandler)
            remoteEP = New System.Net.IPEndPoint(IPAddress.Parse(remoteHost), port)
        End Sub

        ''' <summary>
        ''' 初始化一个在本机进行进程间通信的Socket对象
        ''' </summary>
        ''' <param name="LocalPort"></param>
        ''' <param name="ExceptionHandler"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function LocalClient(LocalPort As Integer, Optional ExceptionHandler As ExceptionHandler = Nothing) As AsynchronousClient
            Return New AsynchronousClient(LocalIPAddress, LocalPort, ExceptionHandler)
        End Function

        Public requestHandler As Net.DataResponseHandler
        Protected connectDone As ManualResetEvent

        Dim _MyLocalPort As Integer

        Public ReadOnly Property MyLocalPort As Integer
            Get
                Return _MyLocalPort
            End Get
        End Property

        ''' <summary>
        ''' 函数会想服务器上面的socket对象一样在这里发生阻塞
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub BeginConnect()

            connectDone = New ManualResetEvent(False) ' ManualResetEvent instances signal completion.  

            ' Establish the remote endpoint for the socket.  
            ' For this example use local machine.   
            ' Create a TCP/IP socket.  
            Dim client As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            Call client.Bind(New System.Net.IPEndPoint(System.Net.IPAddress.Any, 0))
            ' Connect to the remote endpoint. 
            Call client.BeginConnect(remoteEP, New AsyncCallback(AddressOf ConnectCallback), client)
            ' Wait for connect.    
            Call connectDone.WaitOne()
            Call Console.WriteLine(client.LocalEndPoint.ToString)

            _MyLocalPort = DirectCast(client.LocalEndPoint, System.Net.IPEndPoint).Port
            _EndReceive = True

            Dim state As New StateObject With {.workSocket = client}

            Do While Not Me.disposedValue
                Call Thread.Sleep(1)

                If _EndReceive Then
                    _EndReceive = False
                    state.Stack = 0
                    Call Receive(state)
                End If
            Loop

            '' Send test data to the remote device.  
            'Call InternalSend(client, Message & Net.TcpSynchronizationServicesSocket.EOF)
            'Call sendDone.WaitOne()

            '' Receive the response from the remote device.    
            'Call Receive(client)
            'Call receiveDone.WaitOne()

            'Return response
        End Sub 'Main 

        Public Class StateObject : Inherits WrapperClassTools.Net.StateObject
            Public Stack As Integer

        End Class

        Public Shared Function GetMyWANIP() As String
            Dim WebPage As String = "http://tool.114la.com/ip".Get_PageContent
            Dim IPAddress As String = Regex.Match(WebPage, "您的 IP 是：</th>.*?<td>.*?\d+(\.\d+){3}").Value
            IPAddress = Regex.Match(IPAddress, "\d+(\.\d+){3}").Value
            Return IPAddress
        End Function

        Public Sub WaitForConnected()
            Do While Me._MyLocalPort = 0
                Call Threading.Thread.Sleep(1)
            Loop
        End Sub

        Protected _EndReceive As Boolean

        Protected Sub ConnectCallback(ar As IAsyncResult)

            ' Retrieve the socket from the state object.    
            Dim client As Socket = CType(ar.AsyncState, Socket)

            ' Complete the connection.  
            Try
                client.EndConnect(ar)
                ' Signal that the connection has been made.    
                Call connectDone.Set()
            Catch ex As Exception
                Call InternalExceptionHandler(ex)
            End Try
        End Sub 'ConnectCallback  

        ''' <summary>
        ''' 本客户端socket在服务器上面的哈希句柄值
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ServerHash As Integer = 0

        Public Sub WaitForHash()
            Do While ServerHash = 0
                Call Thread.Sleep(1)
            Loop
        End Sub

        'An exception of type 'System.Net.Sockets.SocketException' occurred in System.dll but was not handled in user code
        'Additional information: A request to send or receive data was disallowed because the socket is not connected and 
        '(when sending on a datagram socket using a sendto call) no address was supplied

        Protected Sub Receive(client As StateObject)
            ' Begin receiving the data from the remote device. 
            Try
                Dim bytes = client.workSocket.Receive(client.buffer, SocketFlags.None)

                If bytes > 0 Then
                    Dim response As String = System.Text.Encoding.ASCII.GetString(client.buffer, 0, bytes)
                    Call client.sb.Append(response)

                    response = client.sb.ToString()

                    Dim p As Integer = InStr(response, Net.TcpSynchronizationServicesSocket.EOF)

                    If p > 0 Then
                        Dim temp As String = response
#If DEBUG Then
                        Call Console.WriteLine($"[DEBUG {Now.ToString}] {NameOf(AsynchronousClient)}:> {NameOf(temp)} ===> {temp}")
#End If
                        Call client.sb.Clear()
#If DEBUG Then
                        temp = Mid(temp, p + Net.TcpSynchronizationServicesSocket.EOF_Mark_Length + 1)
                        Call Console.WriteLine($"[DEBUG {Now.ToString}] {NameOf(AsynchronousClient)}:> {NameOf(temp)} ===> {temp}")
                        Call client.sb.Append(temp)
                        Call Console.WriteLine($"[DEBUG {Now.ToString}] {NameOf(AsynchronousClient)}:> {NameOf(client.sb)} ===> {client.sb.ToString }")
#End If
                        response = Mid(response, 1, p - 1)
                        Try
                            Dim hash As Integer

                            If ServicesProtocol.GetServerHash(response.Split, hash) Then
                                Me._ServerHash = hash
                            Else
                                Call New Thread(Sub() Call requestHandler(response, Nothing)).Start()
                            End If
                        Catch ex As Exception
                            '客户端处理数据的时候发生了内部错误
                            Call ex.PrintException
                        End Try

                        'Call InternalSend(client.workSocket, Net.TcpSynchronizationServicesSocket.EOF)

                        _EndReceive = True
                    Else


                    End If
                Else

                End If

                client.Stack += 1

                If client.Stack > 1000 Then
                    _EndReceive = True
                    Return
                Else
                    Call Thread.Sleep(1)
                End If

                If Me.disposedValue Then
                    Return
                End If

                '还没有结束
                Call Receive(client)

            Catch ex As Exception ' 现有的连接被强制关闭  
                Call Me.InternalExceptionHandler(ex)

                If Not Me.RemoteServerShutdown Is Nothing Then
                    Call RemoteServerShutdown()()
                End If

                Try
                    Call client.workSocket.Shutdown(SocketShutdown.Both)
                Catch exc As Exception

                End Try
            End Try
        End Sub 'Receive 

        ''' <summary>
        ''' 远程主机强制关闭连接之后触发这个动作
        ''' </summary>
        ''' <returns></returns>
        Public Property RemoteServerShutdown As MethodInvoker

        Private Sub ReceiveCallback(ar As IAsyncResult)

            ' Retrieve the state object and the client socket     
            ' from the asynchronous state object.   
            Dim state As StateObject = CType(ar.AsyncState, StateObject)
            Dim client As Socket = state.workSocket
            ' Read data from the remote device.     

            Dim bytesRead As Integer

            Try
                bytesRead = client.EndReceive(ar)
            Catch ex As Exception
                Call InternalExceptionHandler(ex)
                GoTo EX_EXIT
            End Try

            If bytesRead > 0 Then

                ' There might be more data, so store the data received so far.     
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead))
                ' Get the rest of the data.    
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReceiveCallback), state)
            Else
                Dim response As String

                ' All the data has arrived; put it in response.   
                If state.sb.Length > 1 Then

                    response = state.sb.ToString()
                Else
EX_EXIT:            response = ""
                End If

                ' Signal that all bytes have been received.  
                _EndReceive = True

                response = requestHandler(response, Nothing)
                Call InternalSend(client, response)
            End If
        End Sub 'ReceiveCallback     

        ''' <summary>
        ''' ????
        ''' An exception of type 'System.Net.Sockets.SocketException' occurred in System.dll but was not handled in user code
        ''' Additional information: A request to send or receive data was disallowed because the socket is not connected and 
        ''' (when sending on a datagram socket using a sendto call) no address was supplied
        ''' </summary>
        ''' <param name="client"></param>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Private Sub InternalSend(client As Socket, data As String)

            ' Convert the string data to byte data using ASCII encoding.     
            Dim byteData As Byte() = Encoding.ASCII.GetBytes(data)
            ' Begin sending the data to the remote device.    
            Try
                Call client.Send(byteData, byteData.Length, SocketFlags.None)
            Catch ex As Exception
                Call Me.InternalExceptionHandler(ex)
            End Try
        End Sub 'Send    

#Region "IDisposable Support"
        Protected disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call connectDone.Set()  ' ManualResetEvent instances signal completion.  
                    '    Call receiveDone.Set() '中断服务器的连接
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

    End Class 'AsynchronousClient 
End Namespace
