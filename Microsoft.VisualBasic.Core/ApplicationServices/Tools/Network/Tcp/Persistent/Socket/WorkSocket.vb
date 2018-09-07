#Region "Microsoft.VisualBasic::6255d0b6e20d368b323e6d85eac43266, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Network\Tcp\Persistent\Socket\WorkSocket.vb"

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
    '         Sub: ReadCallback, SendMessage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net.Sockets
Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Protocols

Namespace Net.Persistent.Socket

    ''' <summary>
    ''' 长连接之中只是进行消息的发送处理，并不保证数据能够被接收到
    ''' </summary>
    Public Class WorkSocket : Inherits StateObject
        Public ExceptionHandle As Abstract.ExceptionHandler
        Public ForceCloseHandle As ForceCloseHandle
        Public ReadOnly ConnectTime As Date = Now
        Public TotalBytes As Double

        Sub New(Socket As StateObject)
            Me.ChunkBuffer = Socket.ChunkBuffer
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

        Public Sub SendMessage(request As RequestStream)
            Dim byteData As Byte() = request.Serialize
            Try
                Call Me.workSocket.Send(byteData, byteData.Length, SocketFlags.None)
            Catch ex As Exception
                Call ForceCloseHandle(Me)
            End Try
        End Sub
    End Class
End Namespace
