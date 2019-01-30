Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter
Imports any = System.Object
Imports number = System.Double

Namespace Layouts.Cola

    Public Interface network
        Property nodes() As Node()
        Property links() As Link(Of Node)()

    End Interface

    ''' <summary>
    ''' 这个模块是对外开放网络布局生成的计算函数的接口
    ''' </summary>
    Public Module batch

        '*
        '     * @property nudgeGap spacing between parallel edge segments
        '     * @property margin space around nodes
        '     * @property groupMargin space around groups
        '     

        Private Function gridify(pgLayout As Object, nudgeGap As Double, margin As Double, groupMargin As Double) As Object
            pgLayout.cola.start(0, 0, 0, 10, False)
            Dim gridrouter = route(pgLayout.cola.nodes(), pgLayout.cola.groups(), margin, groupMargin)
            Return gridrouter.routeEdges(Of Object)(pgLayout.powerGraph.powerEdges, nudgeGap, Function(e) e.source.routerNode.id, Function(e) e.target.routerNode.id)
        End Function

        Private Function route(Of Node)(nodes As Node(), groups As any, margin As Double, groupMargin As Double) As GridRouter(Of Object)
            nodes.ForEach(Sub(d)
                              d.routerNode = New With {
                               .name = d.name,
                               .bounds = d.bounds.inflate(-margin)
                          }
                          End Sub)
            groups.forEach(Function(d)
                               d.routerNode = New With {
                                .bounds = d.bounds.inflate(-groupMargin),
                                .children = (If(d.groups IsNot Nothing, d.groups.map(Function(c) nodes.Length + c.id), New Object() {})).concat(If(d.leaves IsNot Nothing, d.leaves.map(Function(c) c.index), New Object() {}))
                           }

                           End Function)
            Dim gridRouterNodes As Node() = nodes.Concat(groups).map(Function(d, i)
                                                                         d.routerNode.id = i
                                                                         Return d.routerNode

                                                                     End Function)
            Return New GridRouter(Of Node)(gridRouterNodes, New NodeAccessor(Of Node) With {
            .getChildren = Function(v) v.children,
            .getBounds = Function(v) v.bounds
        }, margin - groupMargin)
        End Function

        ''' <summary>
        ''' 从这里开始进行布局的计算
        ''' </summary>
        ''' <param name="graph"></param>
        ''' <param name="size"></param>
        ''' <param name="grouppadding"></param>
        ''' <returns></returns>
        Public Function powerGraphGridLayout(graph As network, size As Double(), grouppadding As Double) As Object
            ' compute power graph
            Dim powerGraph = Nothing

            Call graph.nodes.ForEach(Sub(v, i) v.index = i)
            Call New Layout() _
                .avoidOverlaps(False) _
                .nodes(graph.nodes) _
                .links(graph.links) _
                .powerGraphGroups(Sub(d)
                                      powerGraph = d
                                      powerGraph.groups.forEach(Sub(v) v.padding = grouppadding)
                                  End Sub)

            ' construct a flat graph with dummy nodes for the groups and edges connecting group dummy nodes to their children
            ' power edges attached to groups are replaced with edges connected to the corresponding group dummy node
            Dim n = graph.nodes.Length
            Dim edges = New Object() {}
            Dim vs = graph.nodes.ToList
            vs.ForEach(Function(v, i) InlineAssignHelper(v.index, i))
            powerGraph.groups.forEach(Function(g)
                                          Dim sourceInd = InlineAssignHelper(g.index, g.id + n)
                                          vs.Add(g)
                                          If g.leaves IsNot Nothing Then
                                              g.leaves.forEach(Function(v) edges.push(New With {
                                                  Key .source = sourceInd,
                                                  Key .target = v.index
                                              }))
                                          End If
                                          If g.groups IsNot Nothing Then
                                              g.groups.forEach(Function(gg) edges.push(New With {
                                                  Key .source = sourceInd,
                                                  Key .target = gg.id + n
                                              }))
                                          End If

                                      End Function)
            powerGraph.powerEdges.forEach(Function(e)
                                              edges.push(New With {
                                                  Key .source = e.source.index,
                                                  Key .target = e.target.index
                                              })

                                          End Function)

            ' layout the flat graph with dummy nodes and edges
            Call New Layout().size(size).nodes(vs).links(edges).avoidOverlaps(False).linkDistance(30).symmetricDiffLinkLengths(5).convergenceThreshold(0.0001).start(100, 0, 0, 0, False)

            ' final layout taking node positions from above as starting positions
            ' subject to group containment constraints
            ' and then gridifying the layout
            '.flowLayout('y', 30)


            Return New With {
            Key .cola = New Layout().convergenceThreshold(0.001).size(size).avoidOverlaps(True).nodes(graph.nodes).links(graph.links).groupCompactness(0.0001).linkDistance(30).symmetricDiffLinkLengths(5).powerGraphGroups(Function(d)
                                                                                                                                                                                                                                 powerGraph = d
                                                                                                                                                                                                                                 powerGraph.groups.forEach(Function(v) InlineAssignHelper(v.padding, grouppadding))

                                                                                                                                                                                                                             End Function).start(50, 0, 100, 0, False),
            Key .powerGraph = powerGraph
        }
        End Function
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Module
End Namespace