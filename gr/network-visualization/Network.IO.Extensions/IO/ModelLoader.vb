#Region "Microsoft.VisualBasic::c520cbaefdabb4bc5f151371a6c212d6, gr\network-visualization\Network.IO.Extensions\IO\ModelLoader.vb"

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

    '     Module ModelLoader
    ' 
    '         Function: (+2 Overloads) CreateGraph, getEdgeGuid, Tabular
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports names = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic.NamesOf

Namespace FileStream

    Public Module ModelLoader

        ''' <summary>
        ''' 将<see cref="NetworkGraph"/>保存到csv文件之中
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="properties">
        ''' The data property names of nodes and edges.
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function Tabular(g As NetworkGraph, Optional properties$() = Nothing, Optional is2D As Boolean = True) As NetworkTables
            Dim nodes As New List(Of Node)
            Dim edges As New List(Of NetworkEdge)
            Dim data As Dictionary(Of String, String)

            For Each n As Graph.Node In g.vertex
                If n.data Is Nothing Then
                    n.data = New NodeData
                End If

                data = New Dictionary(Of String, String) From {
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

                If Not n.data.color Is Nothing AndAlso n.data.color.GetType Is GetType(SolidBrush) Then
                    data(names.REFLECTION_ID_MAPPING_NODECOLOR) = DirectCast(n.data.color, SolidBrush).Color.ToHtmlColor
                End If

                If Not properties Is Nothing Then
                    For Each key As String In properties.Where(Function(p) n.data.HasProperty(p))
                        data(key) = n.data(key)
                    Next
                End If

                For Each key As String In {
                    names.REFLECTION_ID_MAPPING_DEGREE,
                    names.REFLECTION_ID_MAPPING_DEGREE_IN,
                    names.REFLECTION_ID_MAPPING_DEGREE_OUT
                }.Where(Function(p) n.data.HasProperty(p))

                    data(key) = n.data(key)
                Next

                ' 20191022
                ' name 会和cytoscape之中的name属性产生冲突
                ' 所以在这里修改为label
                If Not data.ContainsKey("label") Then
                    data.Add("label", n.data.label)
                End If

                nodes += New Node With {
                    .ID = n.label,
                    .NodeType = n.data(names.REFLECTION_ID_MAPPING_NODETYPE),
                    .Properties = data
                }
            Next

            For Each l As Edge In g.graphEdges
                edges += New NetworkEdge With {
                    .fromNode = l.U.label,
                    .toNode = l.V.label,
                    .interaction = l.data(names.REFLECTION_ID_MAPPING_INTERACTION_TYPE),
                    .value = l.weight,
                    .Properties = New Dictionary(Of String, String) From {
                        {NameOf(EdgeData.label), l.data.label},
                        {names.REFLECTION_ID_MAPPING_EDGE_GUID, l.ID}
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
        <Extension> Public Function CreateGraph(net As NetworkTables,
                                                Optional nodeColor As Func(Of Node, Brush) = Nothing,
                                                Optional defaultNodeSize$ = "20,20") As NetworkGraph

            Return CreateGraph(Of Node, NetworkEdge)(net, nodeColor, defaultNodeSize:=defaultNodeSize)
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
                                                                            Optional defaultNodeSize$ = "20,20") As NetworkGraph
            Dim defaultNodeSizeVals As Double() = defaultNodeSize _
                .Split(","c) _
                .Select(AddressOf Val) _
                .ToArray
            Dim getRadius = Function(node As Node) As Double()
                                Dim s$ = node(names.REFLECTION_ID_MAPPING_DEGREE)

                                If s.StringEmpty Then
                                    Return defaultNodeSizeVals
                                Else
                                    Return {Val(s)}
                                End If
                            End Function
            Dim defaultColor As Brush = New SolidBrush(defaultBrush.TranslateColor)

            If Not net.nodes.All(Function(node) node.Properties.ContainsKey(names.REFLECTION_ID_MAPPING_DEGREE)) Then
                Call $"Not all of the nodes contains degree value, nodes' radius will use default value: {defaultNodeSize}".Warning
            End If

            If nodeColor Is Nothing Then
                nodeColor = Function(n)
                                ' check property value at first
                                If n.HasProperty(names.REFLECTION_ID_MAPPING_NODECOLOR) Then
                                    Return n(names.REFLECTION_ID_MAPPING_NODECOLOR).GetBrush
                                End If

                                ' if not exists then use default color
                                Return defaultColor
                            End Function
            End If

            Dim nodes = LinqAPI.Exec(Of Graph.Node) <=
 _
                From n As Node
                In net.nodes
                Let id = n.ID
                Let pos As AbstractVector = New FDGVector2(Val(n("x")), Val(n("y")))
                Let c As Brush = nodeColor(n)
                Let r As Double() = getRadius(node:=n)
                Let data As NodeData = New NodeData With {
                    .color = c,
                    .size = r,
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
                                         Let id = edge.getEdgeGuid
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

        <Extension>
        Private Function getEdgeGuid(edge As NetworkEdge) As String
            If edge.HasProperty(names.REFLECTION_ID_MAPPING_EDGE_GUID) Then
                Return edge(names.REFLECTION_ID_MAPPING_EDGE_GUID)
            Else
                Return edge.GetNullDirectedGuid
            End If
        End Function
    End Module
End Namespace
