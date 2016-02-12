Imports Microsoft.VisualBasic.Net.Protocol
Imports System.Net.Sockets
Imports System.Threading

Namespace Net.Persistent.Application

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

        Public ReadOnly Property ProtocolEntry As Long =
            New Net.Protocol.Reflection.Protocol(GetType(Protocols)).EntryPoint

        Public Function SendServerHash(hash As Integer) As RequestStream
            Return New RequestStream(ProtocolEntry, Protocols.ServerHash, CStr(hash))
        End Function

        Public Const WorkSocketPortal As Integer = 66982

        Public Class LogonPOST
            <System.Xml.Serialization.XmlAttribute> Public Property USER_ID As Long
            <System.Xml.Serialization.XmlAttribute> Public Property Socket As String
        End Class

        Public Function LogOnRequest(USER_ID As Long, Socket As String) As RequestStream
            Return New RequestStream(ProtocolEntry, Protocols.Logon, New LogonPOST With {.Socket = Socket, .USER_ID = USER_ID}.GetXml)
        End Function

        Public Function GetLogOnUSER(request As String, ByRef USER_ID As Long, ByRef Socket As String) As Boolean
            Dim post As LogonPOST = request.CreateObjectFromXml(Of LogonPOST)(False)

            If post Is Nothing Then
                Return False
            Else
                USER_ID = post.USER_ID
                Socket = post.Socket

                Return USER_ID <> 0 AndAlso Not String.IsNullOrEmpty(Socket)
            End If
        End Function

        <Serializable>
        Public Class SendMessagePost : Inherits RawStream

            ''' <summary>
            ''' 发送这条消息的客户端的在服务器上面的句柄，也可以看作为用户编号，尽管这个编号是经过一些映射操作最终得到的
            ''' </summary>
            ''' <returns></returns>
            <System.Xml.Serialization.XmlAttribute> Public Property [FROM] As Long
            <System.Xml.Serialization.XmlAttribute> Public Property USER_ID As Long
            <System.Xml.Serialization.XmlText> Public Property Message As RequestStream

            Sub New(rawStream As Byte())
                Dim pTemp As Byte() = New Byte(INT64 - 1) {}
                Dim p As Integer

                Call Array.ConstrainedCopy(rawStream, p.Move(INT64), pTemp, Scan0, pTemp.Length)
                FROM = BitConverter.ToInt64(pTemp, Scan0)
                Call Array.ConstrainedCopy(rawStream, p.Move(INT64), pTemp, Scan0, pTemp.Length)
                USER_ID = BitConverter.ToInt64(pTemp, Scan0)

                pTemp = New Byte(rawStream.Length - INT64 - INT64 - 1) {}
                Call Array.ConstrainedCopy(rawStream, p, pTemp, Scan0, pTemp.Length)
                Message = New RequestStream(pTemp)
            End Sub

            Sub New()
            End Sub

            Public Overrides Function Serialize() As Byte()
                Dim RequestStream As Byte() = Message.Serialize
                Dim ChunkBuffer As Byte() = New Byte(Protocol.RequestStream.INT64 + Protocol.RequestStream.INT64 + RequestStream.Length - 1) {}
                Dim p As Integer = 0
                Call Array.ConstrainedCopy(BitConverter.GetBytes(FROM), Scan0, ChunkBuffer, p.Move(INT64), INT64)
                Call Array.ConstrainedCopy(BitConverter.GetBytes(USER_ID), Scan0, ChunkBuffer, p.Move(INT64), INT64)
                Call Array.ConstrainedCopy(RequestStream, Scan0, ChunkBuffer, p, RequestStream.Length)
                Return ChunkBuffer
            End Function
        End Class

        Public Function SendMessageRequest([FROM] As Long, USER_ID As Long, Message As RequestStream) As RequestStream
            Dim post As New SendMessagePost With {
                .FROM = FROM,
                .Message = Message,
                .USER_ID = USER_ID
            }
            Return New RequestStream(ProtocolEntry, Protocols.SendMessage, post.Serialize)
        End Function

        Public Function GetSendMessage(request As RequestStream, ByRef [FROM] As Long, ByRef USER_ID As Long) As Boolean
            Dim post As SendMessagePost = New SendMessagePost(request.ChunkBuffer)

            [FROM] = post.FROM
            USER_ID = post.USER_ID

            Return True
        End Function

        Public Function GetServicesConnection() As RequestStream
            Return New RequestStream(ProtocolEntry, Protocols.ConfigConnection, "GET")
        End Function

        Public Function IsUserOnlineRequest(USER_ID As Long) As RequestStream
            Return New RequestStream(ProtocolEntry, Protocols.IsUserOnline, CStr(USER_ID))
        End Function

        Public Class BroadcastPOST : Inherits RawStream

            <System.Xml.Serialization.XmlAttribute> Public Property USER_ID As Long
            <System.Xml.Serialization.XmlText> Public Property Message As RequestStream

            Public Sub New()
            End Sub

            Sub New(rawStream As Byte())
                Dim pTemp As Byte() = New Byte(INT64 - 1) {}
                Call Array.ConstrainedCopy(rawStream, Scan0, pTemp, Scan0, INT64)
                USER_ID = BitConverter.ToInt64(pTemp, Scan0)
                pTemp = New Byte(rawStream.Length - INT64 - 1) {}
                Call Array.ConstrainedCopy(rawStream, INT64, pTemp, Scan0, pTemp.Length)
                Message = New RequestStream(pTemp)
            End Sub

            Public Overrides Function Serialize() As Byte()
                Dim RequestStream As Byte() = Message.Serialize
                Dim ChunkBuffer As Byte() = New Byte(INT64 + RequestStream.Length - 1) {}
                Call Array.ConstrainedCopy(BitConverter.GetBytes(USER_ID), Scan0, ChunkBuffer, Scan0, INT64)
                Call Array.ConstrainedCopy(RequestStream, Scan0, ChunkBuffer, INT64, RequestStream.Length)
                Return ChunkBuffer
            End Function
        End Class

        Public Function BroadcastMessage(USER_ID As Long, Message As RequestStream) As RequestStream
            Dim post As New BroadcastPOST With {.Message = Message, .USER_ID = USER_ID}
            Return New RequestStream(ProtocolEntry, Protocols.Broadcast, post.Serialize) With {.uid = USER_ID}
        End Function
    End Module
End Namespace