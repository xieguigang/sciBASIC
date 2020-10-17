'#Region "Microsoft.VisualBasic::3ca09afeaf85f3e21c5af252ed11241b, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\AppServer\PushAPI\APIBase.vb"

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

'    '     Class APIBase
'    ' 
'    '         Properties: PushServer
'    ' 
'    '         Constructor: (+1 Overloads) Sub New
'    ' 
'    ' 
'    ' /********************************************************************************/

'#End Region

'Imports Microsoft.VisualBasic.Net.Protocols.Reflection
'Imports Microsoft.VisualBasic.Parallel

'Namespace NETProtocol.PushAPI

'    Public MustInherit Class APIBase

'        Public ReadOnly Property PushServer As PushServer

'        Protected __protocols As ProtocolHandler

'        Sub New(push As PushServer)
'            PushServer = push
'        End Sub

'        ''' <summary>
'        ''' <see cref="DataRequestHandler"/>
'        ''' </summary>
'        ''' <returns></returns>
'        Public MustOverride Function Handler(request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream

'    End Class
'End Namespace
