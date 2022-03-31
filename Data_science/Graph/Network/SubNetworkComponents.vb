#Region "Microsoft.VisualBasic::fe83d00ae41972618fcf897231bf4adb, sciBASIC#\Data_science\Graph\Network\SubNetworkComponents.vb"

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

    '   Total Lines: 100
    '    Code Lines: 78
    ' Comment Lines: 2
    '   Blank Lines: 20
    '     File Size: 3.60 KB


    '     Class SubNetworkComponents
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetEnumerator, GetSingleNodeGraphs, IEnumerable_GetEnumerator, IteratesSubNetworks, measureSubComponent
    '                   popFirstEdge
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Network

    Public Class SubNetworkComponents(Of Node As {New, Network.Node}, U As {New, Network.Edge(Of Node)}, Graph As {New, NetworkGraph(Of Node, U)})
        Implements IEnumerable(Of Graph)

        Dim edges As List(Of U)
        Dim network As NetworkGraph(Of Node, U)
        Dim components As Graph()
        Dim populatedNodes As New List(Of Node)

        Sub New(network As NetworkGraph(Of Node, U), Optional singleNodeAsGraph As Boolean = False)
            Me.network = network
            Me.edges = network.graphEdges.AsList
            Me.components = IteratesSubNetworks.ToArray

            If singleNodeAsGraph Then
                Me.components = Me.components _
                    .JoinIterates(GetSingleNodeGraphs) _
                    .ToArray
            End If
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of Graph) Implements IEnumerable(Of Graph).GetEnumerator
            For Each g As Graph In components
                Yield g
            Next
        End Function

        Private Iterator Function GetSingleNodeGraphs() As IEnumerable(Of Graph)
            Dim removedIndex As Index(Of Node) = populatedNodes.Distinct.Indexing
            Dim [single] As New Graph

            For Each v As Node In network.vertex.Where(Function(n) removedIndex(n) = -1)
                [single] = New Graph
                [single].AddVertex(v)

                Yield [single]
            Next
        End Function

        Private Function popFirstEdge(n As Node) As U
            Return edges _
                .Where(Function(e) e.U Is n OrElse e.V Is n) _
                .FirstOrDefault
        End Function

        Private Function measureSubComponent() As Graph
            Dim subnetwork As New Graph
            Dim edge As U = edges.First
            Dim list As New List(Of Node)

            Call list.Add(edge.U)
            Call list.Add(edge.V)

            Do While list > 0
                ' U和V是由edge带进来的，可能会产生重复
                subnetwork.AddVertex(edge.U)
                subnetwork.AddVertex(edge.V)
                subnetwork.AddEdge(edge.U, edge.V)
                populatedNodes.Add(edge.U)
                populatedNodes.Add(edge.V)
                edges.Remove(edge)

                If -1 = list.IndexOf(edge.U) Then
                    Call list.Add(edge.U)
                End If
                If -1 = list.IndexOf(edge.V) Then
                    Call list.Add(edge.V)
                End If

                edge = Nothing

                Do While edge Is Nothing AndAlso list > 0
                    edge = popFirstEdge(list.First)

                    If edge Is Nothing Then
                        ' 当前的这个节点已经没有相连的边了，移除这个节点
                        Call list.RemoveAt(Scan0)
                    End If
                Loop
            Loop

            Return subnetwork
        End Function

        Private Iterator Function IteratesSubNetworks() As IEnumerable(Of Graph)
            Do While edges > 0
                Yield measureSubComponent()
            Loop
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
