#Region "Microsoft.VisualBasic::101d419776556a1e68617c9a5d76d1d0, Microsoft.VisualBasic.Core\src\Net\Tcp\TCPExtensions.vb"

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

    '   Total Lines: 110
    '    Code Lines: 57 (51.82%)
    ' Comment Lines: 35 (31.82%)
    '    - Xml Docs: 74.29%
    ' 
    '   Blank Lines: 18 (16.36%)
    '     File Size: 4.84 KB


    '     Module TCPExtensions
    ' 
    '         Function: ConnectSocket, GetFirstAvailablePort, PortIsAvailable, PortIsUsed
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Net.Sockets
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Net.Tcp

    <HideModuleName>
    Public Module TCPExtensions

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
            ' an exception that occurs when the host host IP Address is not compatible with the 
            ' address family (typical in the IPv6 case).
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

        ''' <summary>
        ''' 系统tcp/udp端口数最大是65535
        ''' </summary>
        Public Const MAX_PORT As Integer = 65535

        ''' <summary>
        ''' Get the first available TCP port on this local machine.
        ''' (获取第一个可用的端口号，请注意，在高并发状态下可能会出现端口被占用的情况，
        ''' 所以这时候建议将<paramref name="BEGIN_PORT"/>设置为-1，则本函数将会尝试使用随机数来分配可用端口，从而避免一些系统崩溃的情况产生)
        ''' </summary>
        ''' <param name="BEGIN_PORT">Check the local port available from this port value.(从这个端口开始检测)</param>
        ''' <returns></returns>
        Public Function GetFirstAvailablePort(Optional BEGIN_PORT As Integer = 100) As Integer
            If BEGIN_PORT <= 0 Then
                ' 为了避免高并发的时候出现端口占用的情况，在这里使用随机数来解决一些问题
                BEGIN_PORT = randf.NextInteger(MAX_PORT - 1)
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
        ''' <remarks>
        ''' not supported on linux
        ''' </remarks>
        Public Function PortIsUsed() As Integer()
            '获取本地计算机的网络连接和通信统计数据的信息
            Dim ipGlobalProperties As IPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties()
            '返回本地计算机上的所有Tcp监听程序
            Dim ipsTCP As System.Net.IPEndPoint() = ipGlobalProperties.GetActiveTcpListeners()
            '返回本地计算机上的所有UDP监听程序
            Dim ipsUDP As System.Net.IPEndPoint() = ipGlobalProperties.GetActiveUdpListeners()
            '返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
            Dim tcpConnInfoArray As TcpConnectionInformation() = ipGlobalProperties.GetActiveTcpConnections()

            Dim allPorts As New List(Of Integer)

            Call allPorts.AddRange(From ep As System.Net.IPEndPoint In ipsTCP Select ep.Port)
            Call allPorts.AddRange(From ep As System.Net.IPEndPoint In ipsUDP Select ep.Port)
            Call allPorts.AddRange(From conn As TcpConnectionInformation
                                   In tcpConnInfoArray
                                   Select conn.LocalEndPoint.Port)

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
