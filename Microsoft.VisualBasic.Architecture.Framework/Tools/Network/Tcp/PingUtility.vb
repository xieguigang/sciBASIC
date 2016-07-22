#Region "Microsoft.VisualBasic::60de79234aa9a3293f19bf5f13d1acd4, ..\Microsoft.VisualBasic.Architecture.Framework\Tools\Network\Tcp\PingUtility.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Collections.Generic
Imports System.Text
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Runtime.InteropServices
Imports System.Threading

Namespace Net

    ''' <summary>
    ''' http://www.codeproject.com/Articles/18635/Ping-exe-replica-in-C
    ''' 
    ''' Ping.exe replica in C# 2.0
    '''
    ''' Stefan Prodan, 3 May 2007 CPOL
    ''' Usage example of the System.Net.NetworkInformation.Ping.
    ''' </summary>
    ''' <remarks></remarks>
    Public Module PingUtility

        <Flags> Public Enum ConnectionState As Integer
            INTERNET_CONNECTION_MODEM = &H1
            INTERNET_CONNECTION_LAN = &H2
            INTERNET_CONNECTION_PROXY = &H4
            INTERNET_RAS_INSTALLED = &H10
            INTERNET_CONNECTION_OFFLINE = &H20
            INTERNET_CONNECTION_CONFIGURED = &H40
        End Enum

        <DllImport("wininet", CharSet:=CharSet.Auto)> Public Function InternetGetConnectedState(ByRef lpdwFlags As ConnectionState, dwReserved As Integer) As Boolean
        End Function

        Public Function IsOffline() As Boolean
            Dim state As ConnectionState = 0
            InternetGetConnectedState(state, 0)
            If (CInt(ConnectionState.INTERNET_CONNECTION_OFFLINE) And CInt(state)) <> 0 Then
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' 返回与目标远程机器之间的平均通信时间长度
        ''' </summary>
        ''' <param name="IP"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Ping(IP As IPAddress, Optional TimeOut As UInteger = 3000) As Double
            'set options ttl=128 and no fragmentation
            Dim options As New PingOptions(128, True)

            'create a Ping object
            Dim pingTools As New System.Net.NetworkInformation.Ping()

            '32 empty bytes buffer
            Dim data As Byte() = New Byte(31) {}

            Dim received As Integer = 0
            Dim responseTimes As New List(Of Long)()

            'ping 4 times
            For i As Integer = 0 To 3
                Dim reply As PingReply

                Try
                    reply = pingTools.Send(IP, TimeOut, data, options)
                Catch ex As Exception
                    received += 1
                    responseTimes.Add(10 * 1000)
                    Continue For
                End Try

                If reply Is Nothing Then
                    Call "Ping failed for an unknown reason".__DEBUG_ECHO
                    Continue For
                End If

                Select Case reply.Status

                    Case IPStatus.Success

                        If reply.Options Is Nothing Then
                            Console.WriteLine("Reply from {0}: bytes={1} time={2}ms", reply.Address, reply.Buffer.Length, reply.RoundtripTime)
                        Else
                            Console.WriteLine("Reply from {0}: bytes={1} time={2}ms TTL={3}", reply.Address, reply.Buffer.Length, reply.RoundtripTime, reply.Options.Ttl)
                        End If

                        received += 1
                        responseTimes.Add(reply.RoundtripTime)

                    Case IPStatus.TimedOut
                        Console.WriteLine("Request timed out.")

                    Case Else
                        Console.WriteLine("Ping failed {0}", reply.Status.ToString())

                End Select
            Next

            'statistics calculations
            Dim averageTime As Long = -1
            Dim minimumTime As Long = 0
            Dim maximumTime As Long = 0

            For i As Integer = 0 To responseTimes.Count - 1
                If i = 0 Then
                    minimumTime = responseTimes(i)
                    maximumTime = responseTimes(i)
                Else
                    If responseTimes(i) > maximumTime Then
                        maximumTime = responseTimes(i)
                    End If
                    If responseTimes(i) < minimumTime Then
                        minimumTime = responseTimes(i)
                    End If
                End If
                averageTime += responseTimes(i)
            Next

            Dim statistics As New StringBuilder()
            statistics.AppendFormat("Ping statistics for {0}:", IP.ToString())
            statistics.AppendLine()
            statistics.AppendFormat("   Packets: Sent = 4, Received = {0}, Lost = {1} <{2}% loss>,", received, 4 - received, Convert.ToInt32(((4 - received) * 100) \ 4))
            statistics.AppendLine()
            statistics.Append("Approximate round trip times in milli-seconds:")
            statistics.AppendLine()

            'show only if loss is not 100%
            If averageTime <> -1 Then
                statistics.AppendFormat("    Minimum = {0}ms, Maximum = {1}ms, Average = {2}ms", minimumTime, maximumTime, CLng(averageTime \ received))
            End If

            Console.WriteLine()
            Console.WriteLine(statistics.ToString())
            Console.WriteLine()

            If received <= 0 Then  'Ping不通
                Return Double.MaxValue
            End If

            Return averageTime \ received
        End Function
    End Module
End Namespace
