#Region "Microsoft.VisualBasic::929af781aa9c68aa878aadcb86f6e299, gr\network-visualization\Datavisualization.Network\NetworkAPI.vb"

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

    '   Total Lines: 61
    '    Code Lines: 47 (77.05%)
    ' Comment Lines: 6 (9.84%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (13.11%)
    '     File Size: 2.33 KB


    ' Module NetworkAPI
    ' 
    '     Function: EndPoints, Neighborhood, RemoveDuplicated
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract
Imports Microsoft.VisualBasic.Linq

Public Module NetworkAPI

    <Extension>
    Public Iterator Function Neighborhood(adj As AdjacencySet(Of Graph.Edge)) As IEnumerable(Of Node)
        For Each edge As Graph.Edge In adj.EnumerateAllEdges()
            If adj.U = edge.U.label Then
                Yield edge.V
            Else
                Yield edge.U
            End If
        Next
    End Function

    <Extension>
    Public Function EndPoints(network As Graph.NetworkGraph) As (input As Graph.Node(), output As Graph.Node())
        Return New NetworkGraph(Of Graph.Node, Graph.Edge)(network.vertex, network.graphEdges).EndPoints
    End Function

    ''' <summary>
    ''' 移除的重复的边
    ''' </summary>
    ''' <remarks></remarks>
    ''' <param name="directed">是否忽略方向？</param>
    ''' <param name="ignoreTypes">是否忽略边的类型？</param>
    <Extension>
    Public Function RemoveDuplicated(g As Graph.NetworkGraph, Optional directed As Boolean = True, Optional ignoreTypes As Boolean = False) As Graph.NetworkGraph
        Dim graph As New Graph.NetworkGraph()
        Dim uid = Function(edge As Graph.Edge) As String
                      If directed Then
                          Return edge.GetDirectedGuid(ignoreTypes)
                      Else
                          Return edge.GetNullDirectedGuid(ignoreTypes)
                      End If
                  End Function

        For Each node As Graph.Node In g.vertex
            Call graph.AddNode(node.Clone)
        Next

        For Each edge As Graph.Edge In g.graphEdges _
            .GroupBy(uid) _
            .Select(Function(eg) eg.First)

            Call New Graph.Edge With {
                .U = graph.GetElementByID(edge.U.label),
                .V = graph.GetElementByID(edge.V.label),
                .isDirected = edge.isDirected,
                .weight = edge.weight,
                .data = edge.data
            }.DoCall(AddressOf graph.AddEdge)
        Next

        Return graph
    End Function

    <Extension>
    Public Function TakeSubGraph(g As NetworkGraph, v As IEnumerable(Of String), Optional radius As Integer = 1) As NetworkGraph

    End Function

    Private Function TakeSubEdges(g As NetworkGraph, v As String, radius As Integer) As IEnumerable(Of Edge)
        If radius <= 0 Then
            Return {}
        End If

        Dim node As Graph.Node = g.GetElementByID(v)
        Dim edges = node.directedVertex.AsEnumerable.ToArray

        Return edges
    End Function
End Module
