Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter
Imports any = System.Object
Imports sys = System.Math

Namespace Layouts.Cola

    Module linkLengthExtensions

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
            Dim neighbours = New Object()() {}
            Dim addNeighbours = Sub(u As Integer, v As Integer)
                                    If neighbours(u) Is Nothing Then
                                        neighbours(u) = New Object() {}
                                    End If
                                    neighbours(u)(v) = New Object() {}
                                End Sub
            links.DoEach(Sub(e)
                             Dim u = la.getSourceIndex(e), v = la.getTargetIndex(e)
                             addNeighbours(u, v)
                             addNeighbours(v, u)
                         End Sub)
            Return neighbours
        End Function

        ''' <summary>
        ''' modify the lengths of the specified links by the result of function f weighted by w
        ''' </summary>
        ''' <typeparam name="Link"></typeparam>
        ''' <param name="links"></param>
        ''' <param name="w"></param>
        ''' <param name="f"></param>
        ''' <param name="la"></param>
        Private Sub computeLinkLengths(Of Link)(links As Link(), w As Double, f As Func(Of any, any, Double), la As LinkLengthAccessor(Of Link))
            Dim neighbours = getNeighbours(links, la)

            links.DoEach(Sub(l)
                             Dim a = neighbours(la.getSourceIndex(l))
                             Dim b = neighbours(la.getTargetIndex(l))

                             Call la.setLength()(l, 1 + w * f(a, b))
                         End Sub)
        End Sub

        ''' <summary>
        ''' modify the specified link lengths based on the symmetric difference of their neighbours
        ''' </summary>
        ''' <typeparam name="Link"></typeparam>
        ''' <param name="links"></param>
        ''' <param name="la"></param>
        ''' <param name="w"></param>
        Private Sub symmetricDiffLinkLengths(Of Link)(links As Link(), la As LinkLengthAccessor(Of Link), Optional w As Double = 1)
            computeLinkLengths(links, w, Function(a, b) Math.Sqrt(unionCount(a, b) - intersectionCount(a, b)), la)
        End Sub

        ''' <summary>
        ''' modify the specified links lengths based on the jaccard difference between their neighbours
        ''' </summary>
        ''' <typeparam name="Link"></typeparam>
        ''' <param name="links"></param>
        ''' <param name="la"></param>
        ''' <param name="w"></param>
        Public Sub jaccardLinkLengths(Of Link)(links As Link(), la As LinkLengthAccessor(Of Link), Optional w As Double = 1)
            computeLinkLengths(links, w, Function(a, b) If(sys.Min([Object].keys(a).length, [Object].keys(b).length) < 1.1, 0, intersectionCount(a, b) / unionCount(a, b)), la)
        End Sub

        ''' <summary>
        ''' generate separation constraints for all edges unless both their source and sink are in the same strongly connected component
        ''' </summary>
        ''' <typeparam name="Link"></typeparam>
        ''' <param name="n"></param>
        ''' <param name="links"></param>
        ''' <param name="axis"></param>
        ''' <param name="la"></param>
        ''' <returns></returns>
        Private Function generateDirectedEdgeConstraints(Of Link)(n As Double, links As Link(), axis As String, la As LinkSepAccessor(Of Link)) As List(Of IConstraint)
            Dim components = stronglyConnectedComponents(n, links, la)
            Dim nodes As New List(Of Integer)
            Dim constraints As New List(Of IConstraint)

            Call components.ForEach(Sub(c, i)
                                        c.DoEach(Sub(v) nodes(v) = i)
                                    End Sub)
            Call links.DoEach(Sub(l)
                                  Dim ui = la.getSourceIndex(l)
                                  Dim vi = la.getTargetIndex(l)
                                  Dim u = nodes(ui)
                                  Dim v = nodes(vi)

                                  If u <> v Then
                                      Dim c As New IConstraint With {
                                          .axis = axis,
                                          .left = ui,
                                          .right = vi,
                                          .gap = la.getMinSeparation(l)
                                      }

                                      Call constraints.Add(c)
                                  End If
                              End Sub)

            Return constraints
        End Function

        ''' <summary>
        ''' Tarjan's strongly connected components algorithm for directed graphs
        ''' returns an array of arrays of node indicies in each of the strongly connected components.
        ''' a vertex not in a SCC of two or more nodes is it's own SCC.
        ''' adaptation of https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
        ''' </summary>
        ''' <typeparam name="Link"></typeparam>
        ''' <param name="numVertices"></param>
        ''' <param name="edges"></param>
        ''' <param name="la"></param>
        ''' <returns></returns>
        Private Function stronglyConnectedComponents(Of Link)(numVertices As Integer, edges As Link(), la As LinkAccessor(Of Link)) As List(Of Integer())
            Dim nodes As New List(Of NodeIndexer)
            Dim index = 0
            Dim stack As New Stack(Of NodeIndexer)
            Dim components As New List(Of Integer())
            Dim strongConnect As Action(Of NodeIndexer) = Sub(v As NodeIndexer)
                                                              ' Set the depth index for v to the smallest unused index
                                                              v.index = InlineAssignHelper(v.lowlink, System.Math.Max(System.Threading.Interlocked.Increment(index), index - 1))
                                                              stack.Push(v)
                                                              v.onStack = True

                                                              ' Consider successors of v
                                                              For Each w As NodeIndexer In v.out
                                                                  If w.index Is Nothing Then
                                                                      ' Successor w has not yet been visited; recurse on it
                                                                      strongConnect(w)
                                                                      v.lowlink = sys.Min(v.lowlink, w.lowlink)
                                                                  ElseIf w.onStack Then
                                                                      ' Successor w is in stack S and hence in the current SCC
                                                                      v.lowlink = sys.Min(v.lowlink, CType(w.index, Integer))
                                                                  End If
                                                              Next

                                                              ' If v is a root node, pop the stack and generate an SCC
                                                              If v.lowlink = v.index Then
                                                                  ' start a new strongly connected component
                                                                  Dim component As New List(Of NodeIndexer)

                                                                  While stack.Count > 0
                                                                      Dim w = stack.Pop()
                                                                      w.onStack = False
                                                                      'add w to current strongly connected component
                                                                      component.Add(w)

                                                                      If w Is v Then
                                                                          Exit While
                                                                      End If
                                                                  End While

                                                                  ' output the current strongly connected component
                                                                  components.Add(component.Select(Function(vi) vi.id).ToArray)
                                                              End If

                                                          End Sub
            For i As Integer = 0 To numVertices - 1
                nodes.Add(New NodeIndexer With {
                          .id = i,
                          .out = New List(Of NodeIndexer)
                          })
            Next

            For Each e As Link In edges
                Dim v = nodes(la.getSourceIndex(e))
                Dim w = nodes(la.getTargetIndex(e))

                v.out.Add(w)
            Next
            For Each v As NodeIndexer In nodes
                If v.index Is Nothing Then
                    strongConnect(v)
                End If
            Next
            Return components
        End Function

        Private Class NodeIndexer
            Public index As Integer?
            Public id As Integer
            Public out As List(Of NodeIndexer)
            Public lowlink As Integer
            Public onStack As Boolean
        End Class

        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Module
End Namespace