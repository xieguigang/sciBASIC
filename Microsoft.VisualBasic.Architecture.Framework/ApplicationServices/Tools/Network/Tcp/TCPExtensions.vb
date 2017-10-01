#Region "Microsoft.VisualBasic::584fb44f7765ee40e2ad0e17f1eb82ae, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Tools\Network\Tcp\TCPExtensions.vb"

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

Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Net.Sockets
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Net

    Public Module TCPExtensions

        ''' <summary>
        ''' -1标识Ping不通
        ''' </summary>
        ''' <param name="operationTimeOut">ms</param>
        ''' <returns></returns>
        Public Function Ping(ep As System.Net.IPEndPoint, Optional operationTimeOut As Integer = 3 * 1000) As Double
            Return New AsynInvoke(ep).Ping(operationTimeOut)
        End Function

        ''' <summary>
        ''' -1 ping failure
        ''' </summary>
        ''' <param name="invoke"></param>
        ''' <param name="timeout"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Ping(invoke As AsynInvoke, Optional timeout As Integer = 3 * 1000) As Double
            Dim sw As Stopwatch = Stopwatch.StartNew
            Dim request As RequestStream = RequestStream.SystemProtocol(RequestStream.Protocols.Ping, PING_REQUEST)
            Dim response As RequestStream = invoke.SendMessage(request, timeOut:=timeout)

            If HTTP_RFC.RFC_REQUEST_TIMEOUT = response.Protocol Then
                Return -1
            End If

            Return sw.ElapsedMilliseconds
        End Function

        Public Const PING_REQUEST As String = "PING/TTL-78973"

        ''' <summary>
        ''' 假若不能成功的建立起连接的话，则会抛出错误
        ''' </summary>
        ''' <param name="server"></param>
        ''' <param name="port"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ConnectSocket(server As String, port As Integer) As System.Net.IPEndPoint

            ' Get host related information.
            Dim HostEntry As IPHostEntry = Dns.GetHostEntry(server)

            ' Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
            ' an exception that occurs when the host host IP Address is not compatible with the address family
            ' (typical in the IPv6 case).
            For Each Address As IPAddress In HostEntry.AddressList
                Dim endPoint As New System.Net.IPEndPoint(Address, port)
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

        Const MAX_PORT As Integer = 65535    '系统tcp/udp端口数最大是65535

        ''' <summary>
        ''' Get the first available TCP port on this local machine.
        ''' (获取第一个可用的端口号，请注意，在高并发状态下可能会出现端口被占用的情况，
        ''' 所以这时候建议将<paramref name="BEGIN_PORT"/>设置为-1，则本函数将会尝试使用随机数来分配可用端口，从而避免一些系统崩溃的情况产生)
        ''' </summary>
        ''' <param name="BEGIN_PORT">Check the local port available from this port value.(从这个端口开始检测)</param>
        ''' <returns></returns>
        Public Function GetFirstAvailablePort(Optional BEGIN_PORT As Integer = 100) As Integer
            If BEGIN_PORT <= 0 Then
                BEGIN_PORT = Rnd() * (MAX_PORT - 1)  ' 为了避免高并发的时候出现端口占用的情况，在这里使用随机数来解决一些问题
            End If

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
            Dim ipsTCP As System.Net.IPEndPoint() = ipGlobalProperties__1.GetActiveTcpListeners()

            '返回本地计算机上的所有UDP监听程序
            Dim ipsUDP As System.Net.IPEndPoint() = ipGlobalProperties__1.GetActiveUdpListeners()

            '返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
            Dim tcpConnInfoArray As TcpConnectionInformation() = ipGlobalProperties__1.GetActiveTcpConnections()

            Dim allPorts As List(Of Integer) = New List(Of Integer)
            Call allPorts.AddRange((From ep As System.Net.IPEndPoint In ipsTCP Select ep.Port).ToArray)
            Call allPorts.AddRange((From ep As System.Net.IPEndPoint In ipsUDP Select ep.Port).ToArray)
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

#Region "OAuth Arguments"

        Const hash As String = "hash"
        Const uid As String = "uid"

        <Extension> Public Function BuildOAuth(ca As Net.SSL.Certificate) As String
            Dim array As KeyValuePair(Of String, String)() = {
                New KeyValuePair(Of String, String)(hash, ca.PrivateKey),
                New KeyValuePair(Of String, String)(uid, ca.uid)
            }
            Dim oauth As String = WebServiceUtils.BuildUrlData(array)
            Return oauth
        End Function

        <Extension> Public Function GetCA(args As String) As Net.SSL.Certificate
#If DEBUG Then
            Call $"{MethodBase.GetCurrentMethod.GetFullName} ==> {args}".__DEBUG_ECHO
#End If
            Dim data = WebServiceUtils.RequestParser(args, False)
#If DEBUG Then
            Call data.AllKeys.ToArray(Function(k) data(k)).GetJson.__DEBUG_ECHO
#End If
            Dim privateKey As String = data(hash)
            Dim uid As Long = Scripting.CTypeDynamic(Of Long)(data(TCPExtensions.uid))
            Return Net.SSL.Certificate.Install(privateKey, uid)
        End Function
#End Region
    End Module
End Namespace
