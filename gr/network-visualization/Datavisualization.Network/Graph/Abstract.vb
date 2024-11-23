#Region "Microsoft.VisualBasic::854968be5330ce7461f5707751a96ff8, gr\network-visualization\Datavisualization.Network\Graph\Abstract.vb"

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

'   Total Lines: 96
'    Code Lines: 63 (65.62%)
' Comment Lines: 21 (21.88%)
'    - Xml Docs: 90.48%
' 
'   Blank Lines: 12 (12.50%)
'     File Size: 3.27 KB


'     Interface INode
' 
'         Properties: Id, NodeType
' 
'     Interface IInteraction
' 
'         Properties: source, target
' 
'     Interface INetworkEdge
' 
'         Properties: Interaction, value
' 
'     Module ExtensionsAPI
' 
'         Function: Contains, Equals, GetConnectedNode, GetDirectedGuid, GetNullDirectedGuid
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory.SparseGraph

Namespace Graph.Abstract

    ''' <summary>
    ''' Node model in the network
    ''' </summary>
    Public Interface INode
        Property Id As String
        Property NodeType As String
    End Interface

    Public Interface INetworkEdge : Inherits IInteraction
        Property value As Double
        Property Interaction As String
    End Interface

    <HideModuleName>
    Public Module ExtensionsAPI

        <Extension>
        Public Function Equals(Model As IInteraction, Node1 As String, Node2 As String) As Boolean
            If String.Equals(Model.source, Node1, StringComparison.OrdinalIgnoreCase) Then
                Return String.Equals(Model.target, Node2, StringComparison.OrdinalIgnoreCase)
            ElseIf String.Equals(Model.target, Node1, StringComparison.OrdinalIgnoreCase) Then
                Return String.Equals(Model.source, Node2, StringComparison.OrdinalIgnoreCase)
            Else
                Return False
            End If
        End Function

        <Extension>
        Public Function GetConnectedNode(edge As IInteraction, a As String) As String
            If String.Equals(edge.source, a) Then
                Return edge.target
            ElseIf String.Equals(edge.target, a) Then
                Return edge.source
            Else
                Return ""
            End If
        End Function

        <Extension>
        Public Function Contains(edge As IInteraction, node As String) As Boolean
            Return String.Equals(node, edge.source, StringComparison.OrdinalIgnoreCase) OrElse
                String.Equals(node, edge.target, StringComparison.OrdinalIgnoreCase)
        End Function

        ''' <summary>
        ''' 返回没有方向性的统一标识符
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GetNullDirectedGuid(edge As INetworkEdge, Optional ignoreTypes As Boolean = False) As String
            Dim array$() = {
                edge.source, edge.target
            }.OrderBy(Function(s) s) _
             .ToArray

            If ignoreTypes Then
                Return array(0) & " + " & array(1)
            Else
                Return String.Format("[{0}] {1};{2}", edge.Interaction, array(0), array(1))
            End If
        End Function

        ''' <summary>
        ''' 带有方向的互作关系字符串
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GetDirectedGuid(edge As INetworkEdge, Optional ignoreTypes As Boolean = False) As String
            If Not ignoreTypes Then
                Return $"{edge.source} {edge.Interaction} {edge.target}"
            Else
                Return $"{edge.source} + {edge.target}"
            End If
        End Function
    End Module
End Namespace
