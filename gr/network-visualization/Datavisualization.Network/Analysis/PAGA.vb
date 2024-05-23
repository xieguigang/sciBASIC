
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
        Public Function Abstraction(manifolds As NetworkGraph) As NetworkGraph
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

            For Each edge As Edge In abstract.graphEdges
                edge.weight /= max_w * 10
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