#Region "Microsoft.VisualBasic::c4bebf35bbfcc74dddece24e02485123, gr\network-visualization\Datavisualization.Network\Graph\Abstract.vb"

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
    '         Function: Contains, Equals, GetConnectedNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Graph.Abstract

    ''' <summary>
    ''' Node model in the network
    ''' </summary>
    Public Interface INode
        Property Id As String
        Property NodeType As String
    End Interface

    Public Interface IInteraction
        Property source As String
        Property target As String
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
    End Module
End Namespace
