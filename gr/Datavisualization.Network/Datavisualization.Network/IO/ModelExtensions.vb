#Region "Microsoft.VisualBasic::833d9b09c69a14436ad204277ff44f08, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\IO\GraphAPI.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Cytoscape
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Quantile
Imports names = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic.NameOf

Namespace FileStream

    ''' <summary>
    ''' Data Model Extensions
    ''' </summary>
    Public Module GraphAPI

        <Extension> Public Sub AddEdges(net As NetworkTables, from$, targets$())
            If Not net.HaveNode(from) Then
                net += New Node With {
                    .ID = from
                }
            End If

            For Each [to] As String In targets
                If Not net.HaveNode([to]) Then
                    net += New Node With {
                        .ID = [to]
                    }
                End If

                net += New NetworkEdge With {
                    .FromNode = from,
                    .ToNode = [to]
                }
            Next
        End Sub

        ''' <summary>
        ''' 将<see cref="NetworkGraph"/>保存到csv文件之中
        ''' </summary>
        ''' <param name="g"></param>
        ''' <returns></returns>
        <Extension> Public Function Tabular(g As NetworkGraph) As NetworkTables
            Dim nodes As New List(Of Node)
            Dim edges As New List(Of NetworkEdge)

            For Each n In g.nodes
                Dim data As New Dictionary(Of String, String)

                data("x") = n.Data.initialPostion.x
                data("y") = n.Data.initialPostion.y
                ' data("z") = n.Data.initialPostion.z

                nodes += New Node With {
                    .ID = n.ID,
                    .NodeType = n.Data(names.REFLECTION_ID_MAPPING_NODETYPE),
                    .Properties = data
                }
            Next

            For Each l As Edge In g.edges
                edges += New NetworkEdge With {
                    .FromNode = l.Source.ID,
                    .ToNode = l.Target.ID,
                    .Interaction = l.Data(names.REFLECTION_ID_MAPPING_INTERACTION_TYPE)
                }
            Next

            Return New NetworkTables With {
                .Edges = edges,
                .Nodes = nodes
            }
        End Function

        ''' <summary>
        ''' Create a <see cref="NetworkGraph"/> model from csv table data
        ''' </summary>
        ''' <param name="net"></param>
        ''' <returns></returns>
        <Extension> Public Function CreateGraph(net As NetworkTables, Optional nodeColor As Func(Of Node, Brush) = Nothing) As NetworkGraph
            Return CreateGraph(Of Node, NetworkEdge)(net, nodeColor)
        End Function

        <Extension>
        Public Function ScaleRadius(ByRef graph As NetworkGraph, range As DoubleRange) As NetworkGraph
            Dim nodes = graph.nodes.ToArray
            Dim r#() = nodes _
                .Select(Function(x) CDbl(x.Data.radius)) _
                .RangeTransform(range)

            For i As Integer = 0 To nodes.Length - 1
                nodes(i).Data.radius = r#(i)
            Next

            Return graph
        End Function

        ''' <summary>
        ''' Transform the network data model to graph model
        ''' </summary>
        ''' <typeparam name="TNode"></typeparam>
        ''' <typeparam name="TEdge"></typeparam>
        ''' <param name="net"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CreateGraph(Of TNode As Node, TEdge As NetworkEdge)(net As Network(Of TNode, TEdge),
                                                                            Optional nodeColor As Func(Of Node, Brush) = Nothing,
                                                                            Optional defaultBrush$ = "black",
                                                                            Optional defaultRadius! = 20) As NetworkGraph

            Dim getRadius = Function(node As Node) As Single
                                Dim s$ = node(names.REFLECTION_ID_MAPPING_DEGREE)

                                If s.StringEmpty Then
                                    Return defaultRadius
                                Else
                                    Return Val(s)
                                End If
                            End Function

            If nodeColor Is Nothing Then
                Dim br As New SolidBrush(defaultBrush.TranslateColor)
                nodeColor = Function(n) br
            End If


            Dim nodes = LinqAPI.Exec(Of Graph.Node) <=
 _
                From n As Node
                In net.Nodes
                Let id = n.ID
                Let pos As AbstractVector = New FDGVector2(Val(n("x")), Val(n("y")))
                Let c As Brush = nodeColor(n)
                Let r As Single = getRadius(node:=n)
                Let data As NodeData = New NodeData With {
                    .Color = c,
                    .radius = r,
                    .Properties = New Dictionary(Of String, String) From {
                        {names.REFLECTION_ID_MAPPING_NODETYPE, n.NodeType}
                    },
                    .initialPostion = pos
                }
                Select New Graph.Node(id, data)

            Dim nodeTable As New Dictionary(Of Graph.Node)(nodes)
            Dim edges As Edge() =
 _
                LinqAPI.Exec(Of Edge) <= From edge As NetworkEdge
                                         In net.Edges
                                         Let a = nodeTable(edge.FromNode)
                                         Let b = nodeTable(edge.ToNode)
                                         Let id = edge.GetNullDirectedGuid
                                         Let data As EdgeData = New EdgeData With {
                                             .Properties = New Dictionary(Of String, String) From {
                                                 {names.REFLECTION_ID_MAPPING_INTERACTION_TYPE, edge.Interaction}
                                             }
                                         }
                                         Select New Edge(id, a, b, data)

            Dim graph As New NetworkGraph With {
                .nodes = New List(Of Graph.Node)(nodes),
                .edges = New List(Of Edge)(edges)
            }
            Return graph
        End Function

        ''' <summary>
        ''' Load cytoscape exports as network graph model.
        ''' </summary>
        ''' <param name="edgesDf">``edges.csv``</param>
        ''' <param name="nodesDf">``nodes.csv``</param>
        ''' <returns></returns>
        Public Function CytoscapeExportAsGraph(edgesDf As String, nodesDf As String) As NetworkGraph
            Dim edges As Edges() = edgesDf.LoadCsv(Of Edges)
            Dim nodes As Nodes() = nodesDf.LoadCsv(Of Nodes)
            Return CytoscapeExportAsGraph(edges, nodes)
        End Function

        <Extension>
        Public Function CytoscapeNetworkFromEdgeTable(edgesData As IEnumerable(Of Edges)) As NetworkGraph
            Dim edges = edgesData.ToArray
            Dim nodes = edges _
                .Select(Function(x) x.GetConnectNodes) _
                .IteratesALL _
                .Distinct _
                .Select(Function(id) New Nodes With {
                    .name = id
                }).ToArray
            Dim graph As NetworkGraph = CytoscapeExportAsGraph(edges, nodes)
            Return graph
        End Function

        Public Function CytoscapeExportAsGraph(edges As Edges(), nodes As Nodes()) As NetworkGraph
            Dim colors As Color() = AllDotNetPrefixColors
            Dim randColor As Func(Of Color) =
                Function() Color.FromArgb(
                    baseColor:=colors(RandomSingle() * (colors.Length - 1)),
                    alpha:=225)

            Dim gNodes As List(Of Graph.Node) =
 _
                LinqAPI.MakeList(Of Graph.Node) <= From n As Nodes
                                                   In nodes
                                                   Let r = If(n.Degree <= 4, 4, n.Degree) * 5
                                                   Let nd As NodeData = New NodeData With {
                                                       .radius = r,
                                                       .Color = New SolidBrush(randColor())
                                                   }
                                                   Select New Graph.Node(n.name, nd)

            Dim nodesTable As New Dictionary(Of Graph.Node)(gNodes)
            Dim gEdges As List(Of Graph.Edge) =
 _
                LinqAPI.MakeList(Of Edge) <= From edge As Edges
                                             In edges
                                             Let geNodes As Graph.Node() =
                                                 edge.GetNodes(nodesTable).ToArray
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

        ''' <summary>
        ''' 默认移除degree少于10% quantile的节点
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="quantile#"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RemovesByDegreeQuantile(net As NetworkTables, Optional quantile# = 0.1, Optional ByRef removeIDs$() = Nothing) As NetworkTables
            Dim qCut& = net _
                .Nodes _
                .Select(Function(n) n(names.REFLECTION_ID_MAPPING_DEGREE)) _
                .Select(Function(d) CLng(Val(d))) _
                .GKQuantile() _
                .Query(quantile)

            Return net.RemovesByDegree(
                degree:=qCut,
                removeIDs:=removeIDs)
        End Function

        ''' <summary>
        ''' 直接按照节点的``Degree``来筛选
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="degree%">``<see cref="Node"/> -> "Degree"``</param>
        ''' <param name="removeIDs$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RemovesByDegree(net As NetworkTables, Optional degree% = 0, Optional ByRef removeIDs$() = Nothing) As NetworkTables
            Dim nodes As New List(Of Node)
            Dim removes As New List(Of String)

            For Each node As Node In net.Nodes
                Dim ndg As Integer = CInt(Val(node(names.REFLECTION_ID_MAPPING_DEGREE)))

                If ndg > degree Then
                    nodes += node
                Else
                    removes += node.ID
                End If
            Next

            removeIDs = removes

            Dim edges As New List(Of NetworkEdge)
            Dim index As New Index(Of String)(removes)

            For Each edge As NetworkEdge In net.Edges

                ' 如果边之中的任意一个节点被包含在index里面，即有小于cutoff值的节点，则不会被添加
                If index(edge.FromNode) > -1 OrElse index(edge.ToNode) > -1 Then
                Else
                    edges += edge
                End If
            Next

            Return New NetworkTables With {
                .Edges = edges,
                .Nodes = nodes
            }
        End Function
    End Module
End Namespace
