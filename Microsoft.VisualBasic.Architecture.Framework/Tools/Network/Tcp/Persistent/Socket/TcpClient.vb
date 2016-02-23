Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Persistent.Application
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports System.Reflection
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Net.Persistent.Application.Protocols

Namespace Net.Persistent.Socket

    ''' <summary>
    ''' 请注意，这个对象是应用于客户端与服务器保持长连接所使用，并不会主动发送消息给服务器，而是被动的接受服务器的数据请求
    ''' </summary>
    ''' <remarks></remarks>
    Public Class PersistentClient : Implements System.IDisposable
        Implements Net.Abstract.IDataRequestHandler

#Region "Internal Fields"

        ''' <summary>
        ''' The port number for the remote device.  
        ''' </summary>
        ''' <remarks></remarks>
        Protected port As Integer

        Dim _ExceptionHandler As ExceptionHandler
        Protected remoteHost As String

        ''' <summary>
        ''' Remote End Point
        ''' </summary>
        ''' <remarks></remarks>
        Protected ReadOnly remoteEP As System.Net.IPEndPoint
#End Region

        Public Overrides Function ToString() As String
            Return $"Remote_connection={remoteHost}:{port}"
        End Function

        Const LocalIPAddress As String = "127.0.0.1"

        Sub New(remoteDevice As System.Net.IPEndPoint, Optional ExceptionHandler As ExceptionHandler = Nothing)
            Call Me.New(remoteDevice.Address.ToString, remoteDevice.Port, ExceptionHandler)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Client">Copy the TCP client connection profile data from this object.(从本客户端对象之中复制出连接配置参数以进行初始化操作)</param>
        ''' <param name="ExceptionHandler"></param>
        ''' <remarks></remarks>
        Sub New(Client As PersistentClient, Optional ExceptionHandler As ExceptionHandler = Nothing)
            remoteHost = Client.remoteHost
            port = Client.port
            _ExceptionHandler = If(ExceptionHandler Is Nothing, Sub(ex As Exception) Call ex.PrintException, ExceptionHandler)
            remoteEP = New System.Net.IPEndPoint(System.Net.IPAddress.Parse(remoteHost), port)
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
            _ExceptionHandler = If(ExceptionHandler Is Nothing, Sub(ex As Exception) Call ex.PrintException, ExceptionHandler)
            remoteEP = New System.Net.IPEndPoint(System.Net.IPAddress.Parse(remoteHost), port)
        End Sub

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
        Public Overridable Function BeginConnect() As Integer

            connectDone = New ManualResetEvent(False) ' ManualResetEvent instances signal completion.  

            ' Establish the remote endpoint for the socket.  
            ' For this example use local machine.   
            ' Create a TCP/IP socket.  
            Dim client As New System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
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

            Return 0
        End Function 'Main 

        Public Class StateObject : Inherits Net.StateObject
            Public Stack As Integer
        End Class

        Public Sub WaitForConnected()
            Do While Me._MyLocalPort = 0
                Call Threading.Thread.Sleep(1)
            Loop
        End Sub

        Protected _EndReceive As Boolean

        ''' <summary>
        ''' Retrieve the socket from the state object.    
        ''' </summary>
        ''' <param name="ar"></param>
        Protected Sub ConnectCallback(ar As IAsyncResult)
            Dim client As System.Net.Sockets.Socket = CType(ar.AsyncState, System.Net.Sockets.Socket)

            ' Complete the connection.  
            Try
                client.EndConnect(ar)
                ' Signal that the connection has been made.    
                Call connectDone.Set()
            Catch ex As Exception
                Call _ExceptionHandler(ex)
            End Try
        End Sub 'ConnectCallback  

        ''' <summary>
        ''' 本客户端socket在服务器上面的哈希句柄值
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property OnServerHashCode As Integer = 0

        Public Sub WaitForHash()
            Do While OnServerHashCode = 0
                Call Thread.Sleep(1)
            Loop
        End Sub

        ''' <summary>
        ''' An exception of type '<see cref="System.Net.Sockets.SocketException"/>' occurred in System.dll but was not handled in user code
        ''' Additional information: A request to send or receive data was disallowed because the socket is not connected and 
        ''' (when sending on a datagram socket using a sendto call) no address was supplied
        ''' </summary>
        ''' <param name="client"></param>
        Protected Sub Receive(client As StateObject)
            ' Begin receiving the data from the remote device. 
            Try
                _EndReceive = False
                Call __receive(client)
            Catch ex As Exception ' 现有的连接被强制关闭  
                Call Me._ExceptionHandler(ex)

                If Not Me.RemoteServerShutdown Is Nothing Then
                    Call RemoteServerShutdown()()
                End If

                Try
                    Call client.workSocket.Shutdown(SocketShutdown.Both)
                Catch exc As Exception
                    Call App.LogException(exc, MethodBase.GetCurrentMethod.GetFullName)
                End Try
            End Try
        End Sub 'Receive 

        Private Sub __receive(client As StateObject)
            Dim ReceiveHandle = New AsyncCallback(AddressOf ReceiveCallback)

            Call client.workSocket.BeginReceive(client.readBuffer, 0, StateObject.BufferSize, 0, ReceiveHandle, client)
            Call waitReceive()
            Call client.Stack.MoveNext

            If client.Stack > 1000 Then
                _EndReceive = True
                Return
            Else
                Call Thread.Sleep(1)
            End If

            If Not Me.disposedValue Then
                '还没有结束
                Call Receive(client)
            End If
        End Sub

        Private Sub waitReceive()
            Do While Not _EndReceive
                Call Thread.Sleep(1)
            Loop
        End Sub

        ''' <summary>
        ''' 远程主机强制关闭连接之后触发这个动作
        ''' </summary>
        ''' <returns></returns>
        Public Property RemoteServerShutdown As MethodInvoker
        Public Property Responsehandler As DataRequestHandler Implements IDataRequestHandler.Responsehandler

        ''' <summary>
        ''' Read data from the remote device.     
        ''' </summary>
        ''' <param name="state"></param>
        ''' <param name="ar"></param>
        ''' <returns></returns>
        Private Function readDataBuffer(state As StateObject, ar As IAsyncResult) As Byte()
            Dim client As System.Net.Sockets.Socket = state.workSocket
            Dim bytesRead As Integer

            Try
                bytesRead = client.EndReceive(ar)
            Catch ex As Exception
                Call _ExceptionHandler(ex)
                Call _EndReceive.InvokeSet(True)
                Return Nothing
            Finally
                _EndReceive = True
            End Try

            Dim bytesBuffer = state.readBuffer.Takes(bytesRead)
            Return bytesBuffer
        End Function

        ''' <summary>
        ''' Retrieve the state object and the client socket from the asynchronous state object. 
        ''' </summary>
        ''' <param name="ar"></param>
        Private Sub ReceiveCallback(ar As IAsyncResult)
            Dim state As StateObject = DirectCast(ar.AsyncState, StateObject)
            Dim bytesBuffer = readDataBuffer(state, ar)

            If bytesBuffer.IsNullOrEmpty Then Return

            Call state.ChunkBuffer.AddRange(bytesBuffer)

            Dim TempBuffer = state.ChunkBuffer.ToArray
            Dim request = New RequestStream(TempBuffer)

            If Not request.FullRead Then Return

            Call state.ChunkBuffer.Clear()

            If TempBuffer.Length > request.TotalBytes Then
                TempBuffer = TempBuffer.Skip(request.TotalBytes).ToArray
                Call state.ChunkBuffer.AddRange(TempBuffer) '含有剩余的剪裁后的数据
            End If

            Try
                Call requestHandle(request)
            Catch ex As Exception  '客户端处理数据的时候发生了内部错误
                Call ex.PrintException
                Call App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName)
            End Try
        End Sub 'ReceiveCallback     

        Private Sub requestHandle(request As RequestStream)
            If ServicesProtocol.Protocols.ServerHash = request.Protocol Then
                Me._OnServerHashCode = Scripting.CTypeDynamic(Of Integer)(request.GetUTF8String)
            Else
                Call RunTask(Sub() Me.Responsehandler()(request.uid, request, Nothing))
            End If
        End Sub

        ''' <summary>
        ''' ????
        ''' An exception of type 'System.Net.Sockets.SocketException' occurred in System.dll but was not handled in user code
        ''' Additional information: A request to send or receive data was disallowed because the socket is not connected and 
        ''' (when sending on a datagram socket using a sendto call) no address was supplied
        ''' </summary>
        ''' <param name="client"></param>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Private Sub __send(client As System.Net.Sockets.Socket, data As String)
            ' Convert the string data to byte data using ASCII encoding.     
            Dim byteData As Byte() = Encoding.ASCII.GetBytes(data)
            ' Begin sending the data to the remote device.    
            Try
                Call client.Send(byteData, byteData.Length, SocketFlags.None)
            Catch ex As Exception
                Call Me._ExceptionHandler(ex)
                Call App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName)
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

