#Region "Microsoft.VisualBasic::4cf9ec59bbc2bbd054e3a9535e838836, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\LDM\FileStream\GraphAPI.vb"

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
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Cytoscape
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Quantile

Namespace FileStream

    ''' <summary>
    ''' Data Model Extensions
    ''' </summary>
    Public Module GraphAPI

        <Extension>
        Public Sub AddEdges(net As Network, from$, targets$())
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
        <Extension> Public Function Tabular(g As NetworkGraph) As Network
            Dim nodes As New List(Of Node)
            Dim edges As New List(Of NetworkEdge)

            For Each n In g.nodes
                nodes += New Node With {
                    .ID = n.ID
                }
            Next

            For Each l As Edge In g.edges
                edges += New NetworkEdge With {
                    .FromNode = l.Source.ID,
                    .ToNode = l.Target.ID
                }
            Next

            Return New Network With {
                .Edges = edges,
                .Nodes = nodes
            }
        End Function

        <Extension>
        Public Function CreateGraph(net As Network) As NetworkGraph
            Return CreateGraph(Of Node, NetworkEdge)(net)
        End Function

        ''' <summary>
        ''' Transform the network data model to graph model
        ''' </summary>
        ''' <typeparam name="TNode"></typeparam>
        ''' <typeparam name="TEdge"></typeparam>
        ''' <param name="net"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CreateGraph(Of TNode As Node, TEdge As NetworkEdge)(net As Network(Of TNode, TEdge)) As NetworkGraph
            Dim nodes As Graph.Node() =
 _
                LinqAPI.Exec(Of Graph.Node) <= From n As Node
                                               In net.Nodes
                                               Let id = n.ID
                                               Let data As NodeData = New NodeData With {
                                                   .Color = Brushes.Red,
                                                   .radius = 20
                                               }
                                               Select New Graph.Node(id, data)

            Dim nodehash As New Dictionary(Of Graph.Node)(nodes)
            Dim edges As Edge() =
 _
                LinqAPI.Exec(Of Edge) <= From edge As NetworkEdge
                                         In net.Edges
                                         Let a = nodehash(edge.FromNode)
                                         Let b = nodehash(edge.ToNode)
                                         Let id = edge.GetNullDirectedGuid
                                         Select New Edge(id, a, b, New EdgeData)

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

            Dim nodehash As New Dictionary(Of Graph.Node)(gNodes)
            Dim gEdges As List(Of Graph.Edge) =
 _
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

        <Extension>
        Public Function GetDegrees(net As Network) As Dictionary(Of String, Integer)
            Dim degree As New Dictionary(Of String, Integer)
            Dim counts As Action(Of String) =
 _
                Sub(node$) _
 _
                    If degree.ContainsKey(node) Then _
                        degree(node) += 1 _
                    Else _
                        Call degree.Add(node, 1)

            For Each edge As NetworkEdge In net.Edges
                Call counts(edge.FromNode)
                Call counts(edge.ToNode)
            Next

            Return degree
        End Function

        ''' <summary>
        ''' 默认移除degree少于10% quantile的节点
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="quantile#"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RemovesByDegreeQuantile(net As Network, Optional quantile# = 0.1, Optional ByRef removeIDs$() = Nothing) As Network
            Dim qCut& = net _
                .Nodes _
                .Select(Function(n) n("Degree")) _
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
        Public Function RemovesByDegree(net As Network, Optional degree% = 0, Optional ByRef removeIDs$() = Nothing) As Network
            Dim nodes As New List(Of Node)
            Dim removes As New List(Of String)

            For Each node As Node In net.Nodes
                Dim ndg As Integer = CInt(Val(node("Degree")))

                If ndg > degree Then
                    nodes += node
                Else
                    removes += node.ID
                End If
            Next

            removeIDs = removes

            Dim edges As New List(Of NetworkEdge)
            Dim index As New IndexOf(Of String)(removes)

            For Each edge As NetworkEdge In net.Edges

                ' 如果边之中的任意一个节点被包含在index里面，即有小于cutoff值的节点，则不会被添加
                If index(edge.FromNode) > -1 OrElse index(edge.ToNode) > -1 Then
                Else
                    edges += edge
                End If
            Next

            Return New Network With {
                .Edges = edges,
                .Nodes = nodes
            }
        End Function
    End Module
End Namespace
