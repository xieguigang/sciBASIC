Imports System.Net.NetworkInformation
Imports System.Net
Imports System.Net.Sockets


Namespace DotNET_Wrapper.Tools.TCPSocket

    Public Module TCPExtensions

        Public Structure LoggingClient

            Private Logging As Microsoft.VisualBasic.Logging.LogFile
            Private Handle As AsynchronousSocketListener.DataResponseHandler
            Private ObjectID As String

            Sub New(ByRef Logging As Logging.LogFile, ServerHandle As AsynchronousSocketListener.DataResponseHandler, Optional ObjectId As String = "")
                Me.Logging = Logging
                Me.Handle = ServerHandle
                Me.ObjectID = ObjectId
            End Sub

            Private Const MSG_FORMAT As String = "[{0}] {1} ---> {2}"

            Public Function GetHandle() As AsynchronousSocketListener.DataResponseHandler
                Dim Client As Logging.LogFile = Logging
                Dim ServerHandle = Handle
                Dim obj As String = ObjectID

                Return Function(str As String, addr As IPEndPoint) As String

                           Dim Message As String = String.Format(MSG_FORMAT, Now.ToString, addr.ToString, str)
                           Call Client.WriteLine(Message, obj, Type:=VisualBasic.Logging.LogFile.MsgTypes.INF)

                           Return ServerHandle(str, addr)
                       End Function
            End Function
        End Structure

        ''' <summary>
        ''' 假若不能成功的建立起连接的话，则会抛出错误
        ''' </summary>
        ''' <param name="server"></param>
        ''' <param name="port"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ConnectSocket(server As String, port As Integer) As IPEndPoint

            ' Get host related information.
            Dim HostEntry As IPHostEntry = Dns.GetHostEntry(server)

            ' Loop through the AddressList to obtain the supported AddressFamily. This is to avoid 
            ' an exception that occurs when the host host IP Address is not compatible with the address family 
            ' (typical in the IPv6 case). 
            For Each Address As IPAddress In HostEntry.AddressList
                Dim endPoint As New IPEndPoint(Address, port)
                Dim Socket As Socket = New Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)

                Try
                    Call Socket.Connect(endPoint)
                Catch ex As Exception
                    Continue For
                End Try

                If Socket.Connected Then
                    Return endPoint
                End If
            Next

            Throw New Exception(String.Format("The target connection to {0}:{1} can not be made!", server, port))
        End Function

        ''' <summary>
        ''' Get the first available TCP port on this local machine.(获取第一个可用的端口号)
        ''' </summary>
        ''' <param name="BEGIN_PORT">Check the local port available from this port value.(从这个端口开始检测)</param>
        ''' <returns></returns>
        Public Function GetFirstAvailablePort(Optional BEGIN_PORT As Integer = 100) As Integer
            Dim MAX_PORT As Integer = 65535    '系统tcp/udp端口数最大是65535            

            For i As Integer = BEGIN_PORT To MAX_PORT - 1
                If PortIsAvailable(port:=i) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        ''' <summary>
        ''' 获取操作系统已用的端口号
        ''' </summary>
        ''' <returns></returns>
        Public Function PortIsUsed() As Integer()
            '获取本地计算机的网络连接和通信统计数据的信息
            Dim ipGlobalProperties__1 As IPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties()

            '返回本地计算机上的所有Tcp监听程序
            Dim ipsTCP As IPEndPoint() = ipGlobalProperties__1.GetActiveTcpListeners()

            '返回本地计算机上的所有UDP监听程序
            Dim ipsUDP As IPEndPoint() = ipGlobalProperties__1.GetActiveUdpListeners()

            '返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
            Dim tcpConnInfoArray As TcpConnectionInformation() = ipGlobalProperties__1.GetActiveTcpConnections()

            Dim allPorts As List(Of Integer) = New List(Of Integer)
            Call allPorts.AddRange((From ep As IPEndPoint In ipsTCP Select ep.Port).ToArray)
            Call allPorts.AddRange((From ep As IPEndPoint In ipsUDP Select ep.Port).ToArray)
            Call allPorts.AddRange((From conn As TcpConnectionInformation In tcpConnInfoArray Select conn.LocalEndPoint.Port).ToArray)

            Return allPorts.ToArray
        End Function

        ''' <summary>
        ''' 检查指定端口是否已用
        ''' </summary>
        ''' <param name="port"></param>
        ''' <returns></returns>
        Public Function PortIsAvailable(port As Integer) As Boolean
            Dim portUsed As Integer() = PortIsUsed()

            For Each p As Integer In portUsed
                If p = port Then
                    Return False
                End If
            Next

            Return True
        End Function
    End Module
End Namespace