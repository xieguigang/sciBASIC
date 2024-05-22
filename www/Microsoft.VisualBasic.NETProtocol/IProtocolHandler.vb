#Region "Microsoft.VisualBasic::dde078b1e8ade79a9acb6ba52d46a7d8, www\Microsoft.VisualBasic.NETProtocol\IProtocolHandler.vb"

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

    '   Total Lines: 77
    '    Code Lines: 44 (57.14%)
    ' Comment Lines: 14 (18.18%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (24.68%)
    '     File Size: 2.54 KB


    ' Class IProtocolHandler
    ' 
    ' 
    ' 
    ' Delegate Function
    ' 
    ' 
    ' Delegate Sub
    ' 
    ' 
    ' Delegate Sub
    ' 
    ' 
    ' Module Extensions
    ' 
    '     Function: (+2 Overloads) Ping
    ' 
    '     Sub: (+2 Overloads) SendMessage
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel

''' <summary>
''' Object for handles the request <see cref="ProtocolAttribute"/>.
''' </summary>
Public MustInherit Class IProtocolHandler

    MustOverride ReadOnly Property ProtocolEntry As Long
    MustOverride Function HandleRequest(request As RequestStream, remoteDevcie As System.Net.IPEndPoint) As BufferPipe

End Class

#Region "Delegate Abstract Interface"

Public Delegate Function SendMessageInvoke(Message As String) As String

Public Delegate Sub ForceCloseHandle(socket As StateObject)


#End Region

Public Delegate Sub ProcessMessagePush(message As RequestStream)

<HideModuleName>
Public Module Extensions

    ''' <summary>
    ''' -1标识Ping不通
    ''' </summary>
    ''' <param name="operationTimeOut">ms</param>
    ''' <returns></returns>
    Public Function Ping(ep As System.Net.IPEndPoint, Optional operationTimeOut As Integer = 3 * 1000) As Double
        Return New TcpRequest(ep).Ping(operationTimeOut)
    End Function

    ''' <summary>
    ''' -1 ping failure
    ''' </summary>
    ''' <param name="invoke"></param>
    ''' <param name="timeout"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Ping(invoke As TcpRequest, Optional timeout As Integer = 3 * 1000) As Double
        Dim sw As Stopwatch = Stopwatch.StartNew
        Dim request As RequestStream = RequestStream.SystemProtocol(RequestStream.Protocols.Ping, PING_REQUEST)
        Dim response As RequestStream = invoke _
            .SetTimeOut(TimeSpan.FromMilliseconds(timeout)) _
            .SendMessage(request)

        If HTTP_RFC.RFC_REQUEST_TIMEOUT = response.Protocol Then
            Return -1
        End If

        Return sw.ElapsedMilliseconds
    End Function

    Public Const PING_REQUEST As String = "PING/TTL-78973"

#Region ""

    <Extension>
    Public Sub SendMessage(host As System.Net.IPEndPoint, request As String, Callback As Action(Of String))
        Dim client As New TcpRequest(host)
        Call New Threading.Thread(Sub() Callback(client.SendMessage(request))).Start()
    End Sub

    <Extension>
    Public Sub SendMessage(host As Net.IPEndPoint, request As String, Callback As Action(Of String))
        Call host.GetIPEndPoint.SendMessage(request, Callback)
    End Sub

#End Region
End Module
