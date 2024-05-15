#Region "Microsoft.VisualBasic::cc10132a86b72e00e05e5fbc2dd6f31b, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\Protocols\UserAPI.vb"

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

    '   Total Lines: 42
    '    Code Lines: 25
    ' Comment Lines: 11
    '   Blank Lines: 6
    '     File Size: 1.50 KB


    '     Module UserAPI
    ' 
    ' 
    '         Enum Protocols
    ' 
    '             GetData, InitUser
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: ProtocolEntry
    ' 
    '     Function: InitUser, Uid
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
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

        Public ReadOnly Property ProtocolEntry As Long = New ProtocolAttribute(GetType(Protocols)).EntryPoint

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
            Dim rep = New TcpRequest(remote).SendMessage(req)
            Dim args = rep.LoadObject(AddressOf JSON.LoadJSON(Of InitPOSTBack))
            args.Portal.ipAddress = remote.ipAddress ' 服务器端偷懒了
            Return args
        End Function
    End Module
End Namespace
