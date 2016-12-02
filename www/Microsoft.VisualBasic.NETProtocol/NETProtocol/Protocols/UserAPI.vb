#Region "Microsoft.VisualBasic::e748018cf16c4dc5803abb5243d7fb50, ..\sciBASIC#\www\Microsoft.VisualBasic.NETProtocol\NETProtocol\Protocols\UserAPI.vb"

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

Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Serialization

Namespace NETProtocol.Protocols

    ''' <summary>
    ''' 用户客户端所调用的协议
    ''' </summary>
    Module UserAPI

        Public Enum Protocols
            InitUser
            ''' <summary>
            ''' 获取得到推送的消息
            ''' </summary>
            GetData
        End Enum

        Public ReadOnly Property ProtocolEntry As Long =
            New Protocol(GetType(Protocols)).EntryPoint

        ''' <summary>
        ''' 在服务器端调用得到用户的唯一标识符
        ''' </summary>
        ''' <param name="sId"></param>
        ''' <returns></returns>
        Public Function Uid(sId As String) As Long
            sId &= Now.ToBinary.ToString
            sId = SecurityString.GetMd5Hash(sId)
            Return SecurityString.ToLong(sId)
        End Function

        Public Function InitUser(remote As IPEndPoint, uid As String) As InitPOSTBack
            Dim req = RequestStream.CreateProtocol(ProtocolEntry, Protocols.InitUser, uid)
            Dim rep = New AsynInvoke(remote).SendMessage(req)
            Dim args = rep.LoadObject(AddressOf JSON.LoadObject(Of InitPOSTBack))
            args.Portal.IPAddress = remote.IPAddress ' 服务器端偷懒了
            Return args
        End Function
    End Module
End Namespace
