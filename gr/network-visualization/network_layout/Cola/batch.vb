#Region "Microsoft.VisualBasic::2525260cb7f18b842ffad2f56c49fbfb, gr\network-visualization\network_layout\Cola\batch.vb"

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

    '   Total Lines: 163
    '    Code Lines: 117 (71.78%)
    ' Comment Lines: 27 (16.56%)
    '    - Xml Docs: 62.96%
    ' 
    '   Blank Lines: 19 (11.66%)
    '     File Size: 8.18 KB


    '     Module batch
    ' 
    '         Function: gridify, powerGraphGridLayout, route
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Cola

    ''' <summary>
    ''' 这个模块是对外开放网络布局生成的计算函数的接口
    ''' </summary>
    Public Module batch

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="pgLayout"></param>
        ''' <param name="nudgeGap">spacing between parallel edge segments</param>
        ''' <param name="margin">space around nodes</param>
        ''' <param name="groupMargin">space around groups</param>
        ''' <returns></returns>
        Private Function gridify(pgLayout As LayoutGraph, nudgeGap As Double, margin As Double, groupMargin As Double) As List(Of Segment())
            Dim gridrouter As GridRouter(Of Node)
            Dim edges As PowerEdge(Of Node)()
            Dim getSource As Func(Of PowerEdge(Of Node), Integer) = Function(e) e.source.routerNode.id
            Dim getTarget As Func(Of PowerEdge(Of Node), Integer) = Function(e) e.target.routerNode.id

            pgLayout.cola.start(0, 0, 0, 10, False)
            gridrouter = route(pgLayout.cola.nodes(), pgLayout.cola.groups(), margin, groupMargin)
            edges = pgLayout.powerGraph.powerEdges.ToArray

            Return gridrouter.routeEdges(edges, nudgeGap, getSource, getTarget)
        End Function

        Private Function route(nodes As Node(), groups As Node(), margin As Double, groupMargin As Double) As GridRouter(Of Node)
            nodes.DoEach(Sub(d)
                             d.routerNode = New Node With {
                               .name = d.name,
                               .bounds = d.bounds.inflate(-margin)
                          }
                         End Sub)
            groups.DoEach(Sub(d)
                              Dim childs As Integer() =
                              d.groups.SafeQuery.As(Of Node).Select(Function(c) nodes.Length + c.id).AsList +
                              d.leaves.SafeQuery.As(Of Node).Select(Function(c) c.index)
                              d.routerNode = New Node With {
                                    .bounds = d.bounds.inflate(-groupMargin),
                                    .children = childs
                               }
                          End Sub)
            Dim gridRouterNodes As Node() = nodes.Concat(groups).Select(Function(d, i)
                                                                            d.routerNode.id = i
                                                                            Return d.routerNode
                                                                        End Function).ToArray
            Dim accessor As New NodeAccessor(Of Node) With {
                .getChildren = Function(v) v.children,
                .getBounds = Function(v) v.bounds
            }

            Return New GridRouter(Of Node)(gridRouterNodes, accessor, margin - groupMargin)
        End Function

        ''' <summary>
        ''' 从这里开始进行布局的计算
        ''' </summary>
        ''' <param name="graph"></param>
        ''' <param name="size">The canvas size [width, height]</param>
        ''' <param name="grouppadding"></param>
        ''' <returns></returns>
        Public Function powerGraphGridLayout(graph As network, size As Integer(), grouppadding As Double) As LayoutGraph
            ' compute power graph
            Dim powerGraph As PowerGraph = Nothing

            Call graph.nodes.ForEach(Sub(v, i) v.index = i)
            Call New Layout() _
                .avoidOverlaps(False) _
                .nodes(graph.nodes) _
                .links(graph.links) _
                .powerGraphGroups(Sub(d)
                                      ' powerGraph对象是在这里被赋值初始化的
                                      powerGraph = d
                                      powerGraph.groups.DoEach(Sub(v)
                                                                   v.padding = grouppadding
                                                               End Sub)
                                  End Sub)

            ' construct a flat graph with dummy nodes for the groups and edges connecting group dummy nodes to their children
            ' power edges attached to groups are replaced with edges connected to the corresponding group dummy node
            Dim n = graph.nodes.Length
            Dim edges As New List(Of PowerEdge(Of Integer))
            Dim vs = graph.nodes.ToList
            vs.ForEach(Sub(v, i) v.index = i)

            powerGraph.groups _
                .ForEach(Sub(g As Node)
                             Dim sourceInd%
                             Dim group As New Node
                             Dim index As Node = g

                             group.index = index.id + n
                             sourceInd = group.index
                             vs.Add(group)

                             If group.leaves IsNot Nothing Then
                                 group.leaves.ForEach(Sub(v)
                                                          Dim ie As New PowerEdge(Of Integer) With {
                                                              .source = sourceInd,
                                                              .target = v.VB.index
                                                          }

                                                          Call edges.Add(ie)
                                                      End Sub)
                             End If
                             If group.groups IsNot Nothing Then
                                 group.groups.ForEach(Sub(gg)
                                                          Call edges.Add(New PowerEdge(Of Integer) With {
                                                              .source = sourceInd,
                                                              .target = gg.VB.id + n
                                                          })
                                                      End Sub)
                             End If
                         End Sub)

            powerGraph.powerEdges.ForEach(Sub(e)
                                              Call edges.Add(New PowerEdge(Of Integer) With {
                                                  .source = e.source.index,
                                                  .target = e.target.index
                                              })
                                          End Sub)

            ' layout the flat graph with dummy nodes and edges
            Call New Layout().size(size) _
                .nodes(vs) _
                .links(edges) _
                .avoidOverlaps(False) _
                .linkDistance(30) _
                .symmetricDiffLinkLengths(5) _
                .convergenceThreshold(0.0001) _
                .start(100, 0, 0, 0, False)

            ' final layout taking node positions from above as starting positions
            ' subject to group containment constraints
            ' and then gridifying the layout
            '.flowLayout('y', 30)

            Return New LayoutGraph With {
                .cola = New Layout().convergenceThreshold(0.001) _
                    .size(size) _
                    .avoidOverlaps(True) _
                    .nodes(graph.nodes) _
                    .links(graph.links) _
                    .groupCompactness(0.0001) _
                    .linkDistance(30) _
                    .symmetricDiffLinkLengths(5) _
                    .powerGraphGroups(Sub(d)
                                          powerGraph = d
                                          powerGraph.groups.DoEach(Sub(v) v.padding = grouppadding)
                                      End Sub) _
                    .start(50, 0, 100, 0, False),
                .powerGraph = powerGraph
            }
        End Function

    End Module
End Namespace
