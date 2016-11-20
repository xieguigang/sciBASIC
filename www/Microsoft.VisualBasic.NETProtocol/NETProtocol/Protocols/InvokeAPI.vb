#Region "Microsoft.VisualBasic::070975be0b0baf7475e490c64fcdec74, ..\sciBASIC#\www\Microsoft.VisualBasic.NETProtocol\NETProtocol\Protocols\InvokeAPI.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace NETProtocol.Protocols

    Module InvokeAPI

        ''' <summary>
        ''' 其他的服务器模块向消息推送服务发送更新数据的协议
        ''' </summary>
        Public Enum Protocols
            ''' <summary>
            ''' Push data to user
            ''' </summary>
            PushToUser
        End Enum

        Public ReadOnly Property ProtocolEntry As Long =
            New Protocol(GetType(Protocols)).EntryPoint

        <Extension> Public Function PushData(data As Byte()) As RequestStream
            Return New RequestStream(ProtocolEntry, Protocols.PushToUser, data)
        End Function

        <Extension> Public Sub PushData(API As IPEndPoint, data As Byte())
            Dim req As RequestStream = data.PushData   ' 创建协议
            Dim invoke As AsynInvoke = New AsynInvoke(API)  ' 创建socket
            Dim rep As RequestStream = invoke.SendMessage(req) ' 发送消息
        End Sub
    End Module
End Namespace
