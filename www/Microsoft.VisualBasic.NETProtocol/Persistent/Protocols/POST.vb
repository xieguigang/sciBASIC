#Region "Microsoft.VisualBasic::47a176099fe8ea7633561d49142e3ac1, www\Microsoft.VisualBasic.NETProtocol\Persistent\Protocols\POST.vb"

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

    '   Total Lines: 86
    '    Code Lines: 63
    ' Comment Lines: 4
    '   Blank Lines: 19
    '     File Size: 3.57 KB


    '     Class LogonPOST
    ' 
    '         Properties: Socket, USER_ID
    ' 
    '     Class SendMessagePost
    ' 
    '         Properties: [FROM], Message, USER_ID
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: Serialize
    ' 
    '     Class BroadcastPOST
    ' 
    '         Properties: Message, USER_ID
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Serialization

Namespace Tcp.Persistent.Application.Protocols

    Public Class LogonPOST
        <XmlAttribute> Public Property USER_ID As Long
        <XmlAttribute> Public Property Socket As String
    End Class

    <Serializable> Public Class SendMessagePost : Inherits RawStream

        ''' <summary>
        ''' 发送这条消息的客户端的在服务器上面的句柄，也可以看作为用户编号，尽管这个编号是经过一些映射操作最终得到的
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property [FROM] As Long
        <XmlAttribute> Public Property USER_ID As Long
        <XmlText> Public Property Message As RequestStream

        Sub New(rawStream As Byte())
            Dim pTemp As Byte() = New Byte(INT64 - 1) {}
            Dim p As i32 = 0

            Call Array.ConstrainedCopy(rawStream, p + INT64, pTemp, Scan0, pTemp.Length)
            FROM = BitConverter.ToInt64(pTemp, Scan0)
            Call Array.ConstrainedCopy(rawStream, p + INT64, pTemp, Scan0, pTemp.Length)
            USER_ID = BitConverter.ToInt64(pTemp, Scan0)

            pTemp = New Byte(rawStream.Length - INT64 - INT64 - 1) {}
            Call Array.ConstrainedCopy(rawStream, p, pTemp, Scan0, pTemp.Length)
            Message = New RequestStream(pTemp)
        End Sub

        Sub New()
        End Sub

        Public Overrides Sub Serialize(buffer As Stream)
            Dim RequestStream As Byte() = Message.Serialize
            Dim ChunkBuffer As Byte() = New Byte(INT64 + INT64 + RequestStream.Length - 1) {}
            Dim p As i32 = 0

            Call Array.ConstrainedCopy(BitConverter.GetBytes(FROM), Scan0, ChunkBuffer, p + INT64, INT64)
            Call Array.ConstrainedCopy(BitConverter.GetBytes(USER_ID), Scan0, ChunkBuffer, p + INT64, INT64)
            Call Array.ConstrainedCopy(RequestStream, Scan0, ChunkBuffer, p, RequestStream.Length)

            Call buffer.Write(ChunkBuffer, Scan0, ChunkBuffer.Length)
            Call buffer.Flush()

            Erase ChunkBuffer
        End Sub
    End Class

    Public Class BroadcastPOST : Inherits RawStream

        <XmlAttribute> Public Property USER_ID As Long
        <XmlText> Public Property Message As RequestStream

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

        Public Overrides Sub Serialize(buffer As Stream)
            Dim RequestStream As Byte() = Message.Serialize
            Dim ChunkBuffer As Byte() = New Byte(INT64 + RequestStream.Length - 1) {}
            Call Array.ConstrainedCopy(BitConverter.GetBytes(USER_ID), Scan0, ChunkBuffer, Scan0, INT64)
            Call Array.ConstrainedCopy(RequestStream, Scan0, ChunkBuffer, INT64, RequestStream.Length)

            Call buffer.Write(ChunkBuffer, Scan0, ChunkBuffer.Length)
            Call buffer.Flush()

            Erase ChunkBuffer
        End Sub
    End Class
End Namespace
