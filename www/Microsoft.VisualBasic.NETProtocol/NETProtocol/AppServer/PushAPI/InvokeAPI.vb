#Region "Microsoft.VisualBasic::9d4abd31197e588fd551c884ffaaa35f, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\AppServer\PushAPI\InvokeAPI.vb"

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

    '   Total Lines: 71
    '    Code Lines: 0 (0.00%)
    ' Comment Lines: 58 (81.69%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (18.31%)
    '     File Size: 2.64 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::6b1487624496de01c1ab2d126da9eafb, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\AppServer\PushAPI\InvokeAPI.vb"

'    ' Author:
'    ' 
'    '       asuka (amethyst.asuka@gcmodeller.org)
'    '       xie (genetics@smrucc.org)
'    '       xieguigang (xie.guigang@live.com)
'    ' 
'    ' Copyright (c) 2018 GPL3 Licensed
'    ' 
'    ' 
'    ' GNU GENERAL PUBLIC LICENSE (GPL3)
'    ' 
'    ' 
'    ' This program is free software: you can redistribute it and/or modify
'    ' it under the terms of the GNU General Public License as published by
'    ' the Free Software Foundation, either version 3 of the License, or
'    ' (at your option) any later version.
'    ' 
'    ' This program is distributed in the hope that it will be useful,
'    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
'    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    ' GNU General Public License for more details.
'    ' 
'    ' You should have received a copy of the GNU General Public License
'    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



'    ' /********************************************************************************/

'    ' Summaries:

'    '     Class InvokeAPI
'    ' 
'    '         Constructor: (+1 Overloads) Sub New
'    '         Function: __invokePush, Handler
'    ' 
'    ' 
'    ' /********************************************************************************/

'#End Region

'Imports Microsoft.VisualBasic.Net.HTTP
'Imports Microsoft.VisualBasic.Net.Protocols.Reflection
'Imports Microsoft.VisualBasic.Parallel

'Namespace NETProtocol.PushAPI

'    ''' <summary>
'    ''' 服务器端的其他模块调用消息更新推送的接口
'    ''' </summary>
'    <ProtocolAttribute(GetType(Protocols.InvokeAPI.Protocols))>
'    Public Class InvokeAPI : Inherits APIBase

'        Sub New(push As PushServer)
'            Call MyBase.New(push)
'            __protocols = New ProtocolHandler(Me)
'        End Sub

'        Public Overrides Function Handler(request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
'            Return __protocols.HandleRequest(request, remote)
'        End Function

'        <ProtocolAttribute(Protocols.InvokeAPI.Protocols.PushToUser)>
'        Private Function __invokePush(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
'            Call PushServer.PushUpdate(request)
'            Return NetResponse.RFC_OK
'        End Function
'    End Class
'End Namespace
