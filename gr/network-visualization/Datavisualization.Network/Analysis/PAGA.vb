
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

            Return abstract
        End Function

    End Module
End Namespace