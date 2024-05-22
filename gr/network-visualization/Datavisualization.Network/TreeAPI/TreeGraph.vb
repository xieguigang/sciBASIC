#Region "Microsoft.VisualBasic::a075c461b75a52060e80e58f67cc4571, gr\network-visualization\Datavisualization.Network\TreeAPI\TreeGraph.vb"

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

    '   Total Lines: 50
    '    Code Lines: 39 (78.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (22.00%)
    '     File Size: 2.36 KB


    ' Class TreeGraph
    ' 
    '     Function: CreateGraph
    ' 
    '     Sub: appendGraph
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Public NotInheritable Class TreeGraph(Of K, V)

    Public Shared Function CreateGraph(tree As BinaryTree(Of K, V), uniqueId As Func(Of V, String), keyId As Func(Of K, String), Optional connectLeft As Boolean = False) As NetworkGraph
        Dim g As New NetworkGraph
        Dim root As Node = g.CreateNode(keyId(tree.Key), New NodeData With {.Properties = New Dictionary(Of String, String) From {{NamesOf.REFLECTION_ID_MAPPING_NODETYPE, keyId(tree.Key)}}})

        Call appendGraph(g, root, tree, uniqueId, keyId, connectLeft)
        Return g
    End Function

    Private Shared Sub appendGraph(g As NetworkGraph, center As Node, tree As BinaryTree(Of K, V), uniqueId As Func(Of V, String), keyId As Func(Of K, String), connectLeft As Boolean)
        Dim clusterId = keyId(tree.Key)
        Dim v As Node
        Dim guid As String

        For Each member In tree.Members
            guid = uniqueId(member)

            If g.GetElementByID(guid) Is Nothing Then
                v = g.CreateNode(uniqueId(member), New NodeData With {.Properties = New Dictionary(Of String, String) From {{NamesOf.REFLECTION_ID_MAPPING_NODETYPE, clusterId}}})
            Else
                v = g.GetElementByID(guid)
            End If

            g.CreateEdge(center, v)
        Next

        If Not tree.Left Is Nothing Then
            clusterId = keyId(tree.Left.Key)
            v = g.CreateNode(clusterId, New NodeData With {.Properties = New Dictionary(Of String, String) From {{NamesOf.REFLECTION_ID_MAPPING_NODETYPE, clusterId}}})

            If connectLeft Then
                g.CreateEdge(v, center)
            End If

            appendGraph(g, v, tree.Left, uniqueId, keyId, connectLeft)
        End If

        If Not tree.Right Is Nothing Then
            clusterId = keyId(tree.Right.Key)
            v = g.CreateNode(clusterId, New NodeData With {.Properties = New Dictionary(Of String, String) From {{NamesOf.REFLECTION_ID_MAPPING_NODETYPE, clusterId}}})
            g.CreateEdge(v, center)
            appendGraph(g, v, tree.Right, uniqueId, keyId, connectLeft)
        End If
    End Sub
End Class
