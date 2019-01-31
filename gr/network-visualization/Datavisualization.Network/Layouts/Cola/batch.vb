#Region "Microsoft.VisualBasic::c359335b573f44a8f7daa6d72e59a8f5, gr\network-visualization\Datavisualization.Network\Layouts\Cola\batch.vb"

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

    '     Interface network
    ' 
    '         Properties: links, nodes
    ' 
    '     Module batch
    ' 
    '         Function: gridify, powerGraphGridLayout, route
    ' 
    '     Class LayoutGraph
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter

Namespace Layouts.Cola

    Public Interface network
        Property nodes() As Node()
        Property links() As Link(Of Node)()

    End Interface

    ''' <summary>
    ''' ���ģ���Ƕ��⿪�����粼�����ɵļ��㺯���Ľӿ�
    ''' </summary>
    Public Module batch

        '*
        '     * @property nudgeGap spacing between parallel edge segments
        '     * @property margin space around nodes
        '     * @property groupMargin space around groups
        '     

        Private Function gridify(pgLayout As LayoutGraph, nudgeGap As Double, margin As Double, groupMargin As Double) As Object
            pgLayout.cola.start(0, 0, 0, 10, False)
            Dim gridrouter As GridRouter(Of Object) = route(pgLayout.cola.nodes(), pgLayout.cola.groups(), margin, groupMargin)
            Return gridrouter.routeEdges(Of PowerEdge)(pgLayout.powerGraph.powerEdges.ToArray, nudgeGap, Function(e) e.source.routerNode.id, Function(e) e.target.routerNode.id)
        End Function

        Private Function route(nodes As Node(), groups As Group(), margin As Double, groupMargin As Double) As GridRouter(Of Object)
            nodes.DoEach(Sub(d)
                             d.routerNode = New Node With {
                               .name = d.name,
                               .bounds = d.bounds.inflate(-margin)
                          }
                         End Sub)
            groups.DoEach(Sub(d)


                              d.routerNode = New [Group] With {
                                .bounds = d.bounds.inflate(-groupMargin),
                                .children = (If(d.groups IsNot Nothing, d.groups.Select(Function(c) nodes.Length + c.id), New Object() {})).Concat(If(d.leaves IsNot Nothing, d.leaves.Select(Function(c) c.index), New Object() {}))
                           }
                          End Sub)
            Dim gridRouterNodes As Node() = nodes.Concat(groups).Select(Function(d, i)
                                                                            d.routerNode.id = i
                                                                            Return d.routerNode
                                                                        End Function)
            Return New GridRouter(Of Node)(gridRouterNodes, New NodeAccessor(Of Node) With {
            .getChildren = Function(v) v.children,
            .getBounds = Function(v) v.bounds
        }, margin - groupMargin)
        End Function

        ''' <summary>
        ''' �����￪ʼ���в��ֵļ���
        ''' </summary>
        ''' <param name="graph"></param>
        ''' <param name="size"></param>
        ''' <param name="grouppadding"></param>
        ''' <returns></returns>
        Public Function powerGraphGridLayout(graph As network, size As Double(), grouppadding As Double) As LayoutGraph
            ' compute power graph
            Dim powerGraph As PowerGraph = Nothing

            Call graph.nodes.ForEach(Sub(v, i) v.index = i)
            Call New Layout() _
                .avoidOverlaps(False) _
                .nodes(graph.nodes) _
                .links(graph.links) _
                .powerGraphGroups(Sub(d)
                                      ' powerGraph�����������ﱻ��ֵ��ʼ����
                                      powerGraph = d
                                      powerGraph.groups.ForEach(Sub(v) v.padding = grouppadding)
                                  End Sub)

            ' construct a flat graph with dummy nodes for the groups and edges connecting group dummy nodes to their children
            ' power edges attached to groups are replaced with edges connected to the corresponding group dummy node
            Dim n = graph.nodes.Length
            Dim edges As New List(Of PowerEdge)
            Dim vs = graph.nodes.ToList
            vs.ForEach(Sub(v, i) v.index = i)
            powerGraph.groups.forEach(Sub(g)
                                          Dim sourceInd%

                                          g.index = g.id + n
                                          sourceInd = g.index
                                          vs.Add(g)
                                          If g.leaves IsNot Nothing Then
                                              g.leaves.forEach(Sub(v)
                                                                   Call edges.Add(New PowerEdge With {
                                                  .source = sourceInd,
                                                  .target = v.index
                                              })
                                                               End Sub)
                                          End If
                                          If g.groups IsNot Nothing Then
                                              g.groups.forEach(Sub(gg)
                                                                   Call edges.Add(New PowerEdge With {
                                                  .source = sourceInd,
                                                  .target = gg.id + n
                                              })
                                                               End Sub)
                                          End If
                                      End Sub)
            powerGraph.powerEdges.forEach(Sub(e)
                                              Call edges.Add(New PowerEdge With {
                                                  .source = e.source.index,
                                                  .target = e.target.index
                                              })
                                          End Sub)

            ' layout the flat graph with dummy nodes and edges
            Call New Layout().size(size).nodes(vs).links(edges).avoidOverlaps(False).linkDistance(30).symmetricDiffLinkLengths(5).convergenceThreshold(0.0001).start(100, 0, 0, 0, False)

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
                                  End Sub).start(50, 0, 100, 0, False),
            .powerGraph = powerGraph
        }
        End Function

    End Module

    Public Class LayoutGraph
        Public cola As Layout
        Public powerGraph As PowerGraph
    End Class
End Namespace
