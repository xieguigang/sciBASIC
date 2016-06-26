Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataVisualization.Network.FileStream.Cytoscape
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Imaging
Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

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
                                               Select New Graph.Node(n.Identifier, New NodeData)
            Dim nodehash As New Dictionary(Of Graph.Node)(nodes)
            Dim edges As Edge() =
                LinqAPI.Exec(Of Edge) <= From edge As NetworkEdge
                                         In net.Edges
                                         Select New Edge(
                                             edge.GetNullDirectedGuid,
                                             nodehash(edge.FromNode),
                                             nodehash(edge.ToNode),
                                             New EdgeData)
            Dim graph As New NetworkGraph With {
                .nodes = New List(Of Graph.Node)(nodes),
                .edges = New List(Of Edge)(edges)
            }
            Return graph
        End Function

        Public Function CytoscapeExportAsGraph(edgesDf As String, nodesDf As String) As NetworkGraph
            Dim edges As Edges() = edgesDf.LoadCsv(Of Edges)
            Dim nodes As Nodes() = nodesDf.LoadCsv(Of Nodes)
            Dim colors As Color() = AllDotNetPrefixColors
            Dim randColor = Function() As Color
                                Return Color.FromArgb(220, colors(RandomSingle() * (colors.Length - 1)))
                            End Function
            Dim gNodes As List(Of Graph.Node) =
                LinqAPI.MakeList(Of Graph.Node) <= From n As Nodes
                                                   In nodes
                                                   Let nd As NodeData = New NodeData With {
                                                       .radius = If(n.Degree <= 4, 4, n.Degree) * 5,
                                                       .Color = New SolidBrush(randColor())
                                                   }
                                                   Select New Graph.Node(n.name, nd)
            Dim nodehash As New Dictionary(Of Graph.Node)(gNodes)
            Dim gEdges As List(Of Graph.Edge) =
                LinqAPI.MakeList(Of Edge) <= From edge As Edges
                                             In edges
                                             Let geNodes As Graph.Node() =
                                                 edge.GetNodes(nodehash).ToArray
                                             Select New Edge(
                                                 edge.SUID,
                                                 geNodes(0),
                                                 geNodes(1),
                                                 New EdgeData)
            Return New NetworkGraph With {
                .edges = gEdges,
                .nodes = gNodes
            }
        End Function
    End Module

    Namespace Cytoscape

        Public Class Edges
            Public Property SUID As String
            Public Property EdgeBetweenness As String
            Public Property interaction As String
            Public Property name As String

            Public Iterator Function GetNodes(nodeHash As Dictionary(Of Graph.Node)) As IEnumerable(Of Graph.Node)
                Dim tokens As String() =
                    Strings.Split(name, $"({interaction})") _
                    .ToArray(Function(s) s.Trim)

                Yield nodeHash(tokens.First)
                Yield nodeHash(tokens.Last)
            End Function

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function
        End Class

        Public Class Nodes
            Public Property SUID As String
            Public Property AverageShortestPathLength As String
            Public Property BetweennessCentrality As String
            Public Property ClosenessCentrality As String
            Public Property ClusteringCoefficient As String
            Public Property Degree As Integer
            Public Property Eccentricity As Integer
            Public Property IsSingleNode As String
            Public Property name As String
            Public Property NeighborhoodConnectivity As String
            Public Property NumberOfDirectedEdges As String
            Public Property NumberOfUndirectedEdges As String
            Public Property PartnerOfMultiEdgedNodePairs As String
            Public Property Radiality As String
            Public Property SelfLoops As String
            <Column("shared name")> Public Property SharedName As String
            Public Property Stress As String
            Public Property TopologicalCoefficient As String

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function
        End Class
    End Namespace
End Namespace