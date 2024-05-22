#Region "Microsoft.VisualBasic::3595d61e1ef35e1de1b66b466ead6cd6, www\Microsoft.VisualBasic.NETProtocol\Persistent\Protocols\Protocol.vb"

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

    '   Total Lines: 83
    '    Code Lines: 66 (79.52%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (20.48%)
    '     File Size: 3.18 KB


    '     Module ServicesProtocol
    ' 
    ' 
    '         Enum Protocols
    ' 
    '             Broadcast, ConfigConnection, GetMyIPAddress, IsUserOnline, SendMessage
    '             ServerHash
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: EntryPoint
    ' 
    '     Function: BroadcastMessage, GetLogOnUSER, GetSendMessage, GetServicesConnection, IsUserOnlineRequest
    '               LogOnRequest, SendChannelHashCode, SendMessageRequest
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Parallel

Namespace Tcp.Persistent.Application.Protocols

    Public Module ServicesProtocol

        Public Enum Protocols As Long
            Logon = 59867
            ServerHash
            SendMessage
            ConfigConnection
            IsUserOnline
            GetMyIPAddress
            Broadcast
        End Enum

        Public ReadOnly Property EntryPoint As Long = New ProtocolAttribute(GetType(Protocols)).EntryPoint

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SendChannelHashCode(hash As Integer) As RequestStream
            Return New RequestStream(EntryPoint, Protocols.ServerHash, CStr(hash))
        End Function

        Public Const WorkSocketPortal As Integer = 66982

        Public Function LogOnRequest(USER_ID As Long, Socket As String) As RequestStream
            Dim post As String = New LogonPOST With {
                .Socket = Socket,
                .USER_ID = USER_ID
            }.GetXml
            Return New RequestStream(EntryPoint, Protocols.Logon, post)
        End Function

        Public Function GetLogOnUSER(request As String, ByRef USER_ID As Long, ByRef Socket As String) As Boolean
            Dim post As LogonPOST = request.LoadFromXml(Of LogonPOST)(False)

            If post Is Nothing Then
                Return False
            Else
                USER_ID = post.USER_ID
                Socket = post.Socket

                Return USER_ID <> 0 AndAlso Not String.IsNullOrEmpty(Socket)
            End If
        End Function

        Public Function SendMessageRequest([FROM] As Long, USER_ID As Long, Message As RequestStream) As RequestStream
            Dim post As New SendMessagePost With {
                .FROM = FROM,
                .Message = Message,
                .USER_ID = USER_ID
            }
            Return New RequestStream(EntryPoint, Protocols.SendMessage, post.Serialize)
        End Function

        Public Function GetSendMessage(request As RequestStream, ByRef [FROM] As Long, ByRef USER_ID As Long) As Boolean
            Dim post As SendMessagePost = New SendMessagePost(request.ChunkBuffer)

            [FROM] = post.FROM
            USER_ID = post.USER_ID

            Return True
        End Function

        Public Function GetServicesConnection() As RequestStream
            Return New RequestStream(EntryPoint, Protocols.ConfigConnection, "GET")
        End Function

        Public Function IsUserOnlineRequest(USER_ID As Long) As RequestStream
            Return New RequestStream(EntryPoint, Protocols.IsUserOnline, CStr(USER_ID))
        End Function

        Public Function BroadcastMessage(USER_ID As Long, Message As RequestStream) As RequestStream
            Dim post As New BroadcastPOST With {
                .Message = Message,
                .USER_ID = USER_ID
            }
            Return New RequestStream(EntryPoint, Protocols.Broadcast, post.Serialize)
        End Function
    End Module
End Namespace
