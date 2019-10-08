#Region "Microsoft.VisualBasic::0fe30e3f14b0183b66bd17235ab85a81, gr\network-visualization\Datavisualization.Network\IO\ModelExtensions.vb"

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

    '     Module GraphAPI
    ' 
    '         Function: (+2 Overloads) CreateGraph, OrderByDegrees, RemovesByDegree, RemovesByDegreeQuantile, RemovesByKeyValue
    '                   ScaleRadius, Tabular, UsingDegreeAsRadius
    ' 
    '         Sub: AddEdges
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Quantile
Imports names = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic.NamesOf

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
        ''' <param name="properties">
        ''' The data property names of nodes and edges.
        ''' </param>
        ''' <returns></returns>
        <Extension> Public Function Tabular(g As NetworkGraph, Optional properties$() = Nothing, Optional is2D As Boolean = True) As NetworkTables
            Dim nodes As New List(Of Node)
            Dim edges As New List(Of NetworkEdge)

            For Each n As Graph.Node In g.vertex
                Dim data As New Dictionary(Of String, String) From {
                    {"weight", n.data.mass}
                }

                If Not n.data.initialPostion Is Nothing Then
                    ' skip coordination information when no layout data.
                    data("x") = n.data.initialPostion.x
                    data("y") = n.data.initialPostion.y

                    If Not is2D Then
                        data("z") = n.data.initialPostion.z
                    End If
                End If

                If Not properties Is Nothing Then
                    For Each key As String In properties.Where(Function(p) n.data.HasProperty(p))
                        data(key) = n.data(key)
                    Next
                End If

                nodes += New Node With {
                    .ID = n.Label,
                    .NodeType = n.data(names.REFLECTION_ID_MAPPING_NODETYPE),
                    .Properties = data
                }
            Next

            For Each l As Edge In g.graphEdges
                edges += New NetworkEdge With {
                    .FromNode = l.U.Label,
                    .ToNode = l.V.Label,
                    .Interaction = l.data(names.REFLECTION_ID_MAPPING_INTERACTION_TYPE),
                    .value = l.weight,
                    .Properties = New Dictionary(Of String, String) From {
                        {NameOf(EdgeData.label), l.data.label}
                    }
                }

                With edges.Last
                    If Not properties Is Nothing Then
                        For Each key As String In properties.Where(Function(p) l.data.HasProperty(p))
                            .ItemValue(key) = l.data(key)
                        Next
                    End If
                End With
            Next

            Return New NetworkTables With {
                .edges = edges,
                .nodes = nodes
            }
        End Function

        ''' <summary>
        ''' Create a <see cref="NetworkGraph"/> model from csv table data.
        ''' (这个函数会将节点的degree属性值映射为节点的radius)
        ''' </summary>
        ''' <param name="net"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function CreateGraph(net As NetworkTables, Optional nodeColor As Func(Of Node, Brush) = Nothing) As NetworkGraph
            Return CreateGraph(Of Node, NetworkEdge)(net, nodeColor)
        End Function

        ''' <summary>
        ''' 将网络之中的半径值重新映射到另外一个范围区间内
        ''' </summary>
        ''' <param name="graph"></param>
        ''' <param name="range"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ScaleRadius(ByRef graph As NetworkGraph, range As DoubleRange) As NetworkGraph
            Dim nodes = graph.vertex.ToArray
            Dim r#() = nodes _
                .Select(Function(x) CDbl(x.data.radius)) _
                .RangeTransform(range)

            For i As Integer = 0 To nodes.Length - 1
                nodes(i).data.radius = r#(i)
            Next

            Return graph
        End Function

        ''' <summary>
        ''' 将节点组按照组内的节点的degree的总和或者平均值来重排序
        ''' 函数返回的是降序排序的结果
        ''' 如果需要升序排序，则可以对返回的结果进行一次reverse即可
        ''' </summary>
        ''' <param name="nodeGroups"></param>
        ''' <param name="method$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function OrderByDegrees(nodeGroups As IEnumerable(Of IGrouping(Of String, Graph.Node)), Optional method$ = NameOf(Average)) As IEnumerable(Of IGrouping(Of String, Graph.Node))
            Dim orderProvider As Func(Of IGrouping(Of String, Graph.Node), Double) = Nothing

            Select Case method
                Case NameOf(Enumerable.Average)
                    orderProvider = Function(g)
                                        Return Aggregate x In g Into Average(Val(x.data(names.REFLECTION_ID_MAPPING_DEGREE)))
                                    End Function
                Case NameOf(Enumerable.Sum)
                    orderProvider = Function(g)
                                        Return Aggregate x In g Into Sum(Val(x.data(names.REFLECTION_ID_MAPPING_DEGREE)))
                                    End Function
            End Select

            Return nodeGroups.OrderByDescending(orderProvider)
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
            Dim defaultColor As Brush = New SolidBrush(defaultBrush.TranslateColor)

            If Not net.nodes.All(Function(node) node.Properties.ContainsKey(names.REFLECTION_ID_MAPPING_DEGREE)) Then
                Call $"Not all of the nodes contains degree value, nodes' radius will use default value: {defaultRadius}".Warning
            End If

            Dim nodes = LinqAPI.Exec(Of Graph.Node) <=
 _
                From n As Node
                In net.nodes
                Let id = n.ID
                Let pos As AbstractVector = New FDGVector2(Val(n("x")), Val(n("y")))
                Let c As Brush = If(nodeColor Is Nothing, defaultColor, nodeColor(n))
                Let r As Single = getRadius(node:=n)
                Let data As NodeData = New NodeData With {
                    .color = c,
                    .radius = r,
                    .Properties = New Dictionary(Of String, String) From {
                        {names.REFLECTION_ID_MAPPING_NODETYPE, n.NodeType},
                        {names.REFLECTION_ID_MAPPING_DEGREE, n(names.REFLECTION_ID_MAPPING_DEGREE)},
                        {names.REFLECTION_ID_MAPPING_DEGREE_IN, n(names.REFLECTION_ID_MAPPING_DEGREE_IN)},
                        {names.REFLECTION_ID_MAPPING_DEGREE_OUT, n(names.REFLECTION_ID_MAPPING_DEGREE_OUT)}
                    },
                    .initialPostion = pos,
                    .label = n!name
                }.With(Sub(nd)
                           For Each key As String In n.Properties.Keys
                               If Not nd.Properties.ContainsKey(key) Then
                                   Call nd.Properties.Add(key, n.Properties(key))
                               End If
                           Next
                       End Sub)
                Select New Graph.Node(id, data)

            Dim nodeTable As New Dictionary(Of Graph.Node)(nodes)
            Dim edges As Edge() =
 _
                LinqAPI.Exec(Of Edge) <= From edge As NetworkEdge
                                         In net.edges
                                         Let a = nodeTable(edge.fromNode)
                                         Let b = nodeTable(edge.toNode)
                                         Let id = edge.GetNullDirectedGuid
                                         Let data As EdgeData = New EdgeData With {
                                             .Properties = New Dictionary(Of String, String) From {
                                                 {names.REFLECTION_ID_MAPPING_INTERACTION_TYPE, edge.interaction}
                                             }
                                         }.With(Sub(ed)
                                                    For Each key As String In edge.Properties.Keys
                                                        If Not ed.Properties.ContainsKey(key) Then
                                                            Call ed.Properties.Add(key, edge.Properties(key))
                                                        End If
                                                    Next
                                                End Sub)
                                         Select New Edge(id, a, b, data)

            Dim graph As New NetworkGraph(nodes, edges)
            Return graph
        End Function

        ''' <summary>
        ''' 将节点的degree作为节点的绘图半径数据
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="computeDegree"></param>
        ''' <returns></returns>
        <Extension>
        Public Function UsingDegreeAsRadius(g As NetworkGraph, Optional computeDegree As Boolean = False) As NetworkGraph
            If computeDegree Then
                Call g.ComputeNodeDegrees
            End If

            For Each node In g.vertex
                node.data.radius = Val(node.data!degree)
            Next

            Return g
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
                .nodes _
                .Select(Function(n) n(names.REFLECTION_ID_MAPPING_DEGREE)) _
                .Select(Function(d) CLng(Val(d))) _
                .GKQuantile() _
                .Query(quantile)

            Return net.RemovesByDegree(
                degree:=qCut,
                removeIDs:=removeIDs)
        End Function

        ''' <summary>
        ''' 无边连接的节点的Degree值为零
        ''' </summary>
        Public Const NoConnections% = 0

        ''' <summary>
        ''' (请确保在调用这个函数之前网络模型对应已经通过<see cref="AnalysisDegrees"/>函数计算了degree，否则会移除所有的网络节点而返回一个空网络)
        ''' 直接按照节点的``Degree``来筛选，节点被移除的同时，相应的边连接也会被删除
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="degree%">``<see cref="Node"/> -> "Degree"``.（当这个参数为零的时候，表示默认是将无连接的孤立节点删除掉）</param>
        ''' <param name="removeIDs$">可以通过这个参数来获取得到被删除的节点ID列表</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function RemovesByDegree(net As NetworkTables,
                                        Optional degree% = NoConnections,
                                        Optional ByRef removeIDs$() = Nothing) As NetworkTables
            Return net.RemovesByKeyValue(New NamedValue(Of Double)(names.REFLECTION_ID_MAPPING_DEGREE, degree), removeIDs)
        End Function

        <Extension>
        Public Function RemovesByKeyValue(net As NetworkTables, cutoff As NamedValue(Of Double), Optional ByRef removeIDs$() = Nothing) As NetworkTables
            Dim nodes As New List(Of Node)
            Dim removes As New List(Of String)
            Dim allZero As Boolean = True
            Dim key$ = cutoff.Name
            Dim threshold# = cutoff.Value

            For Each node As Node In net.nodes
                Dim ndg# = Val(node(key))

                If ndg > threshold Then
                    nodes += node
                Else
                    removes += node.ID

                    If ndg <> 0 Then
                        allZero = False
                    End If
                End If
            Next

            If allZero Then
                Call $"All of the nodes' {key} equals ZERO, an empty network will be return!".Warning
            End If

            removeIDs = removes

            Dim edges As New List(Of NetworkEdge)
            Dim index As New Index(Of String)(removes)

            For Each edge As NetworkEdge In net.edges

                ' 如果边之中的任意一个节点被包含在index里面，
                ' 即有小于cutoff值的节点， 则不会被添加
                If index(edge.FromNode) > -1 OrElse index(edge.ToNode) > -1 Then
                Else
                    edges += edge
                End If
            Next

            Return New NetworkTables With {
                .edges = edges,
                .nodes = nodes
            }
        End Function
    End Module
End Namespace
