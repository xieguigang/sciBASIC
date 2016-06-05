Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.Language

Namespace FileStream

    Public Module GraphAPI

        <Extension>
        Public Function CreateGraph(net As Network) As NetworkGraph
            Return CreateGraph(Of Node, NetworkEdge)(net)
        End Function

        <Extension>
        Public Function CreateGraph(Of TNode As Node, TEdge As NetworkEdge)(net As Network(Of TNode, TEdge)) As NetworkGraph
            Dim nodes As Graph.Node() =
                LinqAPI.Exec(Of Graph.Node) <= From n As Node
                                               In net.Nodes
                                               Select New Graph.Node(n.Identifier)
            Dim nodehash As New Dictionary(Of Graph.Node)(nodes)
            Dim edges As Edge() =
                LinqAPI.Exec(Of Edge) <= From edge As NetworkEdge
                                         In net.Edges
                                         Select New Edge(
                                             edge.GetNullDirectedGuid,
                                             nodehash(edge.FromNode),
                                             nodehash(edge.ToNode),
                                             Nothing)
            Dim graph As New NetworkGraph With {
                .nodes = New List(Of Graph.Node)(nodes),
                .edges = New List(Of Edge)(edges)
            }
            Return graph
        End Function
    End Module
End Namespace