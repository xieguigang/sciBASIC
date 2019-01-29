Imports any = System.Object
Imports number = System.Double

Namespace Layouts.Cola

    Interface LinkAccessor(Of Link)
        Function getSourceIndex(l As Link) As Double
        Function getTargetIndex(l As Link) As Double
    End Interface

    Interface LinkLengthAccessor(Of Link)
        Inherits LinkAccessor(Of Link)
        Sub setLength(l As Link, value As Double)
    End Interface

    Class linkLengthExtensions

        ' compute the size of the union of two sets a and b
        Private Function unionCount(a As any, b As any) As Double
            Dim u = New Object() {}
            For Each i As var In a.keys
                u(i) = New Object() {}
            Next
            For Each i As var In b.keys
                u(i) = New Object() {}
            Next
            Return [Object].keys(u).length
        End Function

        ' compute the size of the intersection of two sets a and b
        Private Function intersectionCount(a As Double(), b As Double()) As Double
            Dim n = 0
            For Each i As var In a.keys
                If b(i) IsNot Nothing Then
                    n += 1
                End If
            Next
            Return n
        End Function

        Private Function getNeighbours(Of Link)(links As Link(), la As LinkAccessor(Of Link)) As any
            Dim neighbours = New Object() {}
            Dim addNeighbours = Function(u, v)
                                    If neighbours(u) Is Nothing Then
                                        neighbours(u) = New Object() {}
                                    End If
                                    neighbours(u)(v) = New Object() {}

                                End Function
            links.ForEach(Function(e)
                              Dim u = la.getSourceIndex(e), v = la.getTargetIndex(e)
                              addNeighbours(u, v)
                              addNeighbours(v, u)

                          End Function)
            Return neighbours
        End Function

        ' modify the lengths of the specified links by the result of function f weighted by w
        Private Sub computeLinkLengths(Of Link)(links As Link(), w As Double, f As Func(Of any, any, number), la As LinkLengthAccessor(Of Link))
            Dim neighbours = getNeighbours(links, la)
            links.ForEach(Function(l)
                              Dim a = neighbours(la.getSourceIndex(l))
                              Dim b = neighbours(la.getTargetIndex(l))
                              la.setLength(l, 1 + w * f(a, b))

                          End Function)
        End Sub

        '* modify the specified link lengths based on the symmetric difference of their neighbours
        '     * @class symmetricDiffLinkLengths
        '     

        Private Sub symmetricDiffLinkLengths(Of Link)(links As Link(), la As LinkLengthAccessor(Of Link), Optional w As Double = 1)
            computeLinkLengths(links, w, Function(a, b) Math.Sqrt(unionCount(a, b) - intersectionCount(a, b)), la)
        End Sub

        '* modify the specified links lengths based on the jaccard difference between their neighbours
        '     * @class jaccardLinkLengths
        '     

        Private Sub jaccardLinkLengths(Of Link)(links As Link(), la As LinkLengthAccessor(Of Link), Optional w As Double = 1)
            computeLinkLengths(links, w, Function(a, b) If(Math.Min([Object].keys(a).length, [Object].keys(b).length) < 1.1, 0, intersectionCount(a, b) / unionCount(a, b)), la)
        End Sub

        Public Interface IConstraint
            Property left() As Double
            Property right() As Double
            Property gap() As Double
        End Interface


        Public Interface DirectedEdgeConstraints
            Property axis() As String
            Property gap() As Double
        End Interface

        Public Interface LinkSepAccessor(Of Link)
            Inherits LinkAccessor(Of Link)
            Function getMinSeparation(l As Link) As Double
        End Interface

        '* generate separation constraints for all edges unless both their source and sink are in the same strongly connected component
        '     * @class generateDirectedEdgeConstraints
        '     

        Private Function generateDirectedEdgeConstraints(Of Link)(n As Double, links As Link(), axis As String, la As LinkSepAccessor(Of Link)) As IConstraint()
            Dim components = stronglyConnectedComponents(n, links, la)
            Dim nodes = New Object() {}
            components.forEach(Function(c, i) c.forEach(Function(v) InlineAssignHelper(nodes(v), i)))
            Dim constraints As any() = {}
            links.ForEach(Function(l)
                              Dim ui = la.getSourceIndex(l), vi = la.getTargetIndex(l), u = nodes(ui), v = nodes(vi)
                              If u IsNot v Then
                                  constraints.push(New With {
                                  Key .axis = axis,
                                  Key .left = ui,
                                  Key .right = vi,
                                  Key .gap = la.getMinSeparation(l)
                              })
                              End If

                          End Function)
            Return constraints
        End Function

        '*
        '     * Tarjan's strongly connected components algorithm for directed graphs
        '     * returns an array of arrays of node indicies in each of the strongly connected components.
        '     * a vertex not in a SCC of two or more nodes is it's own SCC.
        '     * adaptation of https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
        '     

        Private Function stronglyConnectedComponents(Of Link)(numVertices As Double, edges As Link(), la As LinkAccessor(Of Link)) As Double()()
            Dim nodes = New Object() {}
            Dim index = 0
            Dim stack = New Object() {}
            Dim components = New Object() {}
            Dim strongConnect = Function(v)
                                    ' Set the depth index for v to the smallest unused index
                                    v.index = InlineAssignHelper(v.lowlink, System.Math.Max(System.Threading.Interlocked.Increment(index), index - 1))
                                    stack.push(v)
                                    v.onStack = True

                                    ' Consider successors of v
                                    For Each w As Object In v.out
                                        If w.index Is Nothing Then
                                            ' Successor w has not yet been visited; recurse on it
                                            strongConnect(w)
                                            v.lowlink = Math.Min(v.lowlink, w.lowlink)
                                        ElseIf w.onStack Then
                                            ' Successor w is in stack S and hence in the current SCC
                                            v.lowlink = Math.Min(v.lowlink, w.index)
                                        End If
                                    Next

                                    ' If v is a root node, pop the stack and generate an SCC
                                    If v.lowlink = v.index Then
                                        ' start a new strongly connected component
                                        Dim component = New Object() {}
                                        While stack.length
                                            w = stack.pop()
                                            w.onStack = False
                                            'add w to current strongly connected component
                                            component.push(w)
                                            If w = v Then
                                                Exit While
                                            End If
                                        End While
                                        ' output the current strongly connected component
                                        components.push(component.map(Function(v) v.id))
                                    End If

                                End Function
            For i As var = 0 To numVertices - 1
                nodes.push(New With {
                Key .id = i,
                Key .out = New Object() {}
            })
            Next
            For Each e As var In edges
                Dim v = nodes(la.getSourceIndex(e))
                Dim w = nodes(la.getTargetIndex(e))
                v.out.push(w)
            Next
            For Each v As var In nodes
                If v.index Is Nothing Then
                    strongConnect(v)
                End If
            Next
            Return components
        End Function
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Class
End Namespace