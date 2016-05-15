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