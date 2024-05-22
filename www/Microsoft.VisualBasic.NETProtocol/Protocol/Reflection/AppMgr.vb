#Region "Microsoft.VisualBasic::d315b51b60f30e97e1c3adba55c65e27, www\Microsoft.VisualBasic.NETProtocol\Protocol\Reflection\AppMgr.vb"

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

    '   Total Lines: 65
    '    Code Lines: 40 (61.54%)
    ' Comment Lines: 12 (18.46%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 13 (20.00%)
    '     File Size: 2.24 KB


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

Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Parallel
Imports TcpEndPoint = System.Net.IPEndPoint

Namespace Protocols.Reflection

    ''' <summary>
    ''' 能够处理多种协议数据
    ''' </summary>
    Public Class AppMgr : Inherits IProtocolHandler

        Public ReadOnly Property ProtocolApps As New Dictionary(Of Long, ProtocolHandler)

        Public Overrides ReadOnly Property ProtocolEntry As Long
            Get
                Return -1
            End Get
        End Property

        Sub New()

        End Sub

        Public Function Register(app As Object, [overrides] As Boolean) As Boolean
            Dim protocol = ProtocolHandler.SafelyCreateObject(app)

            If protocol Is Nothing Then
                Return False
            End If
            If ProtocolApps.ContainsKey(protocol.ProtocolEntry) Then
                If [overrides] Then
                    ' 覆盖掉原有的协议数据
                    Call ProtocolApps.Remove(protocol.ProtocolEntry)
                Else
                    ' 没有被注册
                    Return False
                End If
            End If

            Call ProtocolApps.Add(protocol.ProtocolEntry, protocol)

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

        Public Overrides Function HandleRequest(request As RequestStream, remoteDevcie As TcpEndPoint) As BufferPipe
            If Not ProtocolApps.ContainsKey(request.ProtocolCategory) Then
                Return New DataPipe(NetResponse.RFC_NOT_FOUND)
            End If

            Dim protocol As ProtocolHandler = ProtocolApps(request.ProtocolCategory)
            Return protocol.HandleRequest(request, remoteDevcie)
        End Function
    End Class
End Namespace
