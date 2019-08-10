Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math

Namespace FileStream.Cytoscape

    Public Module CytoscapeTableLoader

        ''' <summary>
        ''' Load cytoscape exports as network graph model.
        ''' </summary>
        ''' <param name="edgesTable">``edges.csv``</param>
        ''' <param name="nodesTable">``nodes.csv``</param>
        ''' <returns></returns>
        Public Function CytoscapeExportAsGraph(edgesTable As String, nodesTable As String) As NetworkGraph
            Dim edges As Edges() = edgesTable.LoadCsv(Of Edges)
            Dim nodes As Nodes() = nodesTable.LoadCsv(Of Nodes)

            Return CytoscapeExportAsGraph(edges, nodes)
        End Function

        Public Function CytoscapeExportAsTable(edgesTable As String, nodesTable As String) As NetworkTables
            Dim edges As NetworkEdge() = edgesTable.LoadCsv(Of Edges) _
                .Select(Function(edge)
                            Dim interaction = edge.GetConnectNodes

                            Return New NetworkEdge With {
                                .fromNode = interaction(Scan0),
                                .toNode = interaction(1),
                                .interaction = edge.interaction,
                                .value = edge.EdgeBetweenness,
                                .Properties = edge.data
                            }
                        End Function) _
                .ToArray
            Dim nodes As Node() = nodesTable.LoadCsv(Of Nodes) _
                .Select(Function(node)
                            Return New Node With {
                                .ID = node.name,
                                .NodeType = "node",
                                .Properties = node.ToTable
                            }
                        End Function) _
                .ToArray

            Return New NetworkTables(edges, nodes)
        End Function

        <Extension>
        Public Function CytoscapeNetworkFromEdgeTable(edgesData As IEnumerable(Of Edges)) As NetworkGraph
            Dim edges = edgesData.ToArray
            Dim nodes = edges _
                .Select(Function(e) e.GetConnectNodes) _
                .IteratesALL _
                .Distinct _
                .Select(Function(id)
                            Return New Nodes With {
                                .name = id
                            }
                        End Function) _
                .ToArray

            Dim graph As NetworkGraph = CytoscapeExportAsGraph(edges, nodes)
            Return graph
        End Function

        Public Function CytoscapeExportAsGraph(edges As Edges(), nodes As Nodes()) As NetworkGraph
            Dim colors As Color() = AllDotNetPrefixColors
            Dim randColor As Func(Of Color) =
                Function()
                    Dim index = RandomSingle() * (colors.Length - 1)
                    Dim color As Color = colors(index)

                    Return Color.FromArgb(
                        baseColor:=color,
                        alpha:=225
                    )
                End Function

            Dim gNodes As List(Of Graph.Node) =
 _
                LinqAPI.MakeList(Of Graph.Node) <= From n As Nodes
                                                   In nodes
                                                   Let r = If(n.Degree <= 4, 4, n.Degree) * 5
                                                   Let nd As NodeData = New NodeData With {
                                                       .radius = r,
                                                       .color = New SolidBrush(randColor())
                                                   }
                                                   Select New Graph.Node(n.name, nd)

            Dim nodesTable As New Dictionary(Of Graph.Node)(gNodes)
            Dim gEdges As List(Of Graph.Edge) =
 _
                LinqAPI.MakeList(Of Edge) <= From edge As Edges
                                             In edges
                                             Let geNodes As Graph.Node() = edge.GetNodes(nodesTable).ToArray
                                             Let u = geNodes(0)
                                             Let v = geNodes(1)
                                             Select New Edge(edge.SUID, u, v, New EdgeData)

            Return New NetworkGraph(gNodes, gEdges)
        End Function
    End Module
End Namespace