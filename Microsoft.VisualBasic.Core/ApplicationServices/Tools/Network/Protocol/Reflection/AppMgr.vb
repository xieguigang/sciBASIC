#Region "Microsoft.VisualBasic::2b0445de7dfb962360dc63693f8cbfeb, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Network\Protocol\Reflection\AppMgr.vb"

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

    '     Class AppMgr
    ' 
    '         Properties: ProtocolApps, ProtocolEntry
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: HandleRequest, Register, RegisterApp
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net
Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Http

Namespace Net.Protocols.Reflection

    ''' <summary>
    ''' 能够处理多种协议数据
    ''' </summary>
    Public Class AppMgr : Inherits IProtocolHandler

        Public ReadOnly Property ProtocolApps As Dictionary(Of Long, ProtocolHandler) =
            New Dictionary(Of Long, ProtocolHandler)

        Public Overrides ReadOnly Property ProtocolEntry As Long
            Get
                Return -1
            End Get
        End Property

        Sub New()

        End Sub

        Public Function Register(app As Object, [overrides] As Boolean) As Boolean
            Dim Protocol = ProtocolHandler.SafelyCreateObject(app)

            If Protocol Is Nothing Then
                Return False
            End If
            If ProtocolApps.ContainsKey(Protocol.ProtocolEntry) Then
                If [overrides] Then
                    Call ProtocolApps.Remove(Protocol.ProtocolEntry)  ' 覆盖掉原有的协议数据
                Else
                    Return False ' 没有被注册
                End If
            End If

            Call ProtocolApps.Add(Protocol.ProtocolEntry, Protocol)

            Return True
        End Function

        ''' <summary>
        ''' 有点多此一举？？
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="App"></param>
        ''' <param name="[overrides]"></param>
        ''' <returns></returns>
        Public Function RegisterApp(Of T As Class)(App As T, [overrides] As Boolean) As Boolean
            Return Register(DirectCast(App, Object), [overrides])
        End Function

        Public Overrides Function HandleRequest(CA As Long, request As RequestStream, remoteDevcie As System.Net.IPEndPoint) As RequestStream
            If Not ProtocolApps.ContainsKey(request.ProtocolCategory) Then
                Return NetResponse.RFC_NOT_FOUND
            End If

            Dim Protocol As ProtocolHandler = ProtocolApps(request.ProtocolCategory)
            Return Protocol.HandleRequest(CA, request, remoteDevcie)
        End Function
    End Class
End Namespace
