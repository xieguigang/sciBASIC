#Region "Microsoft.VisualBasic::375a4cb017640027d375913eac94e87e, Microsoft.VisualBasic.Core\Net\TcpClient.vb"

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
    ' 
    '         Delegate Sub
    ' 
    '             Properties: LocalIP
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '             Function: (+2 Overloads) SendMessage, ToString
    ' 
    '             Sub: ConnectCallback, Receive, ReceiveCallback, Send, SendCallback
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

Namespace DotNET_Wrapper.Tools.TCPSocket

    ''' <summary>
    ''' 服务器端<see cref="AsynchronousSocketListener"></see>必须要返回数据，否则本客户端会一直处于等待的状态
    ''' </summary>
    ''' <remarks></remarks>
    Public Class AsynchronousClient

        ''' <summary>
        ''' The port number for the remote device.  
        ''' </summary>
        ''' <remarks></remarks>
        Private port As Integer

        ''' <summary>
        ''' The response from the remote device.   
        ''' </summary>
        ''' <remarks></remarks>
        Private response As String

        ''' <summary>
        ''' ' ManualResetEvent instances signal completion.  
        ''' </summary>
        ''' <remarks></remarks>
        Dim connectDone As ManualResetEvent
        Dim sendDone As ManualResetEvent
        Dim receiveDone As ManualResetEvent
        Dim InternalExceptionHandler As ExceptionHandler
        Dim remoteHost As String

        Public Delegate Sub ExceptionHandler(ex As Exception)

        ''' <summary>
        ''' Gets the IP address of this local machine.(获取本机对象的IP地址)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared ReadOnly Property LocalIP As String
            Get
                Dim IP = Dns.Resolve(Dns.GetHostName).AddressList(0)
                Dim IPAddr As String = IP.ToString
                Return IPAddr
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return String.Format("Remote_connection={0}:{1},  local_host={2}", remoteHost, port, LocalIP)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="RemotePort"></param>
        ''' <param name="ExceptionHandler">Public Delegate Sub ExceptionHandler(ex As Exception)</param>
        ''' <remarks></remarks>
        Sub New(HostName As String, RemotePort As Integer, Optional ExceptionHandler As ExceptionHandler = Nothing)
            remoteHost = HostName
            port = RemotePort
            InternalExceptionHandler = If(ExceptionHandler Is Nothing, Sub(ex As Exception) Call Console.WriteLine(ex.ToString), ExceptionHandler)
        End Sub

        ''' <summary>
        ''' Returns the server reply.
        ''' </summary>
        ''' <param name="Message"></param>
        ''' <param name="OperationTimeOut">操作超时的时间长度，默认为30秒</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SendMessage(Message As String, Optional OperationTimeOut As Integer = 30 * 1000, Optional OperationTimeoutHandler As Action = Nothing) As String
            response = ""

            Dim p As Boolean = Microsoft.VisualBasic.OperationTimeOut.OperationTimeOut(Of String, String)(AddressOf SendMessage, [In]:=Message, Out:=response, TimeOut:=OperationTimeOut / 1000)

            If p AndAlso Not OperationTimeoutHandler Is Nothing Then
                Call OperationTimeoutHandler()  '操作超时了
                Call Console.WriteLine("[OPERATION_TIME_OUT] " & Message)
            End If

            Return response
        End Function 'Main 

        ''' <summary>
        ''' This function returns the server reply for this request <paramref name="Message"></paramref>.
        ''' </summary>
        ''' <param name="Message">The client request to the server.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SendMessage(Message As String) As String

            connectDone = New ManualResetEvent(False) ' ManualResetEvent instances signal completion.  
            sendDone = New ManualResetEvent(False)
            receiveDone = New ManualResetEvent(False)
            response = ""

            ' Establish the remote endpoint for the socket.  
            ' For this example use local machine.   
            'Dim ipHostInfo As IPHostEntry = Dns.Resolve(remoteHost)
            'Dim ipAddress As IPAddress = ipHostInfo.AddressList(0)
            Dim remoteEP As New IPEndPoint(IPAddress.Parse(remoteHost), port)
            ' Create a TCP/IP socket.  
            Dim client As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            client.Bind(New IPEndPoint(Net.IPAddress.Any, 0))
            ' Connect to the remote endpoint. 
            client.BeginConnect(remoteEP, New AsyncCallback(AddressOf ConnectCallback), client)
            ' Wait for connect.    
            connectDone.WaitOne()
            ' Send test data to the remote device.  
            Send(client, Message & AsynchronousSocketListener.EOF)
            sendDone.WaitOne()

            ' Receive the response from the remote device.    
            Receive(client)
            receiveDone.WaitOne()

            ' Release the socket. 
            client.Shutdown(SocketShutdown.Both)
            client.Close()

            Return response
        End Function 'Main 

        Private Sub ConnectCallback(ByVal ar As IAsyncResult)

            ' Retrieve the socket from the state object.    
            Dim client As Socket = CType(ar.AsyncState, Socket)

            ' Complete the connection.  
            Try
                client.EndConnect(ar)
                ' Signal that the connection has been made.    
                connectDone.Set()
            Catch ex As Exception
                Call InternalExceptionHandler(ex)
            End Try
        End Sub 'ConnectCallback  

        Private Sub Receive(ByVal client As Socket)

            ' Create the state object.     
            Dim state As New StateObject
            state.workSocket = client
            ' Begin receiving the data from the remote device.       
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReceiveCallback), state)
        End Sub 'Receive 

        Private Sub ReceiveCallback(ByVal ar As IAsyncResult)

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
                ' All the data has arrived; put it in response.   
                If state.sb.Length > 1 Then

                    response = state.sb.ToString()
                Else
EX_EXIT:            response = ""
                End If
                ' Signal that all bytes have been received.  
                Call receiveDone.Set()
            End If
        End Sub 'ReceiveCallback     

        Private Sub Send(ByVal client As Socket, ByVal data As String)

            ' Convert the string data to byte data using ASCII encoding.     
            Dim byteData As Byte() = Encoding.ASCII.GetBytes(data)
            ' Begin sending the data to the remote device.    
            client.BeginSend(byteData, 0, byteData.Length, 0, New AsyncCallback(AddressOf SendCallback), client)
        End Sub 'Send    

        Private Sub SendCallback(ByVal ar As IAsyncResult)

            ' Retrieve the socket from the state object.     
            Dim client As Socket = CType(ar.AsyncState, Socket)
            ' Complete sending the data to the remote device.   
            Dim bytesSent As Integer = client.EndSend(ar)
            'Console.WriteLine("Sent {0} bytes to server.", bytesSent)
            ' Signal that all bytes have been sent.    
            sendDone.Set()
        End Sub 'SendCallback
    End Class 'AsynchronousClient 
End Namespace
