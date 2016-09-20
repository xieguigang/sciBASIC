#Region "Microsoft.VisualBasic::c13b750cc22e0455a7759e1450816c13, ..\visualbasic_App\Microsoft.VisualBasic.NETProtocol\AppServer\PushAPI\APIBase.vb"

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

Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace NETProtocol.PushAPI

    Public MustInherit Class APIBase

        Public ReadOnly Property PushServer As PushServer

        Protected __protocols As ProtocolHandler

        Sub New(push As PushServer)
            PushServer = push
        End Sub

        ''' <summary>
        ''' <see cref="DataRequestHandler"/>
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function Handler(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream

    End Class
End Namespace
