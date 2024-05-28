#Region "Microsoft.VisualBasic::7eb93d6f03e49a007724f017d4117c77, gr\network-visualization\Datavisualization.Network\Analysis\PAGA.vb"

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

    '   Total Lines: 98
    '    Code Lines: 69 (70.41%)
    ' Comment Lines: 14 (14.29%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 15 (15.31%)
    '     File Size: 3.96 KB


    '     Module PAGA
    ' 
    '         Function: Abstraction, Count
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts

Namespace Analysis

    ''' <summary>
    ''' PAGA - partition-based graph abstraction
    ''' </summary>
    Public Module PAGA

        ''' <summary>
        ''' Mapping out the coarse-grained connectivity structures of complex manifolds (Genome Biology, 2019).
        ''' </summary>
        ''' <param name="manifolds">
        ''' the manifolds graph should contains the cluster information inside the node metadata.
        ''' </param>
        ''' <returns>
        ''' an abstract graph of the input manifolds result
        ''' </returns>
        <Extension>
        Public Function Abstraction(manifolds As NetworkGraph, Optional threshold As Double = 0.0) As NetworkGraph
            ' split the nodes by node type
            Dim clusters = manifolds.vertex _
                .GroupBy(Function(v)
                             Return v(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)
                         End Function) _
                .ToArray
            Dim abstract As New NetworkGraph

            For Each cluster As IGrouping(Of String, Node) In clusters
                Dim node_group = cluster.ToArray
                Dim x As Double = Aggregate v In node_group Into Sum(v.data.initialPostion.x)
                Dim y As Double = Aggregate v In node_group Into Sum(v.data.initialPostion.y)

                Call abstract.CreateNode(cluster.Key, New NodeData With {
                    .initialPostion = New FDGVector2(x, y),
                    .label = cluster.Key,
                    .mass = node_group.Length,
                    .origID = cluster.Key,
                    .size = {node_group.Length},
                    .weights = {node_group.Length},
                    .Properties = New Dictionary(Of String, String) From {
                        {NamesOf.REFLECTION_ID_MAPPING_NODETYPE, cluster.Key}
                    }
                })
            Next

            For Each c1 As IGrouping(Of String, Node) In clusters
                For Each c2 As IGrouping(Of String, Node) In clusters
                    If c1.Key = c2.Key Then
                        Continue For
                    End If

                    Dim count_12 As Integer = c1.Count(c2.Key)
                    Dim count_21 As Integer = c2.Count(c1.Key)

                    If count_12 + count_21 > 0 Then
                        Call abstract.CreateEdge(
                            abstract.GetElementByID(c1.Key),
                            abstract.GetElementByID(c2.Key),
                            weight:=count_12 + count_21
                        )
                    End If
                Next
            Next

            ' scale the weight to [0,1]
            Dim max_w As Double = abstract.graphEdges.Select(Function(e) e.weight).Max

            For Each edge As Edge In abstract.graphEdges.ToArray
                edge.weight /= max_w

                If edge.weight <= threshold Then
                    Call abstract.RemoveEdge(edge)
                Else
                    edge.weight *= 10
                End If
            Next

            Return abstract
        End Function

        <Extension>
        Private Function Count(cluster As IGrouping(Of String, Node), cluster_id As String) As Integer
            Return Aggregate vi As Node
                   In cluster.AsParallel
                   Let size As Integer = vi.adjacencies _
                       .EnumerateAllEdges _
                       .Where(Function(e) e.V(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = cluster_id) _
                       .Count
                   Into Sum(size)
        End Function

    End Module
End Namespace
