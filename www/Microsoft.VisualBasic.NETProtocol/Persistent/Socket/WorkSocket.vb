#Region "Microsoft.VisualBasic::5489bad5d7e1a506ed7d57fe9928511b, www\Microsoft.VisualBasic.NETProtocol\Persistent\Socket\WorkSocket.vb"

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

    '     Class WorkSocket
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: (+2 Overloads) PushMessage, ReadCallback
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net.Sockets
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Parallel

Namespace Tcp.Persistent.Socket

    ''' <summary>
    ''' 长连接之中只是进行消息的发送处理，并不保证数据能够被接收到
    ''' </summary>
    Public Class WorkSocket : Inherits StateObject

        Public ExceptionHandle As ExceptionHandler
        Public ForceCloseHandle As ForceCloseHandle
        Public TotalBytes As Double

        Public ReadOnly ConnectTime As Date = Now

        Sub New(Socket As StateObject)
            Me.received = Socket.received
            Me.readBuffer = Socket.readBuffer
            Me.workSocket = Socket.workSocket
        End Sub

        ''' <summary>
        ''' DO_NOTHING
        ''' </summary>
        ''' <param name="ar"></param>
        Public Sub ReadCallback(ar As IAsyncResult)
            ' DO_NOTHING
        End Sub 'ReadCallback

        ''' <summary>
        ''' Server send message to user client.
        ''' </summary>
        ''' <param name="request"></param>
        Public Sub PushMessage(request As RequestStream)
            Dim byteData As Byte() = request.Serialize
            Try
                Call Me.workSocket.Send(byteData, byteData.Length, SocketFlags.None)
            Catch ex As Exception
                Call ForceCloseHandle(Me)
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub PushMessage(message As String)
            Call PushMessage(New RequestStream(message))
        End Sub
    End Class
End Namespace
