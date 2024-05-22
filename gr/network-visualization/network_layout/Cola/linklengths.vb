#Region "Microsoft.VisualBasic::a657035672b51bce1f32fbcfe7a02a1b, gr\network-visualization\network_layout\Cola\linklengths.vb"

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

    '   Total Lines: 225
    '    Code Lines: 134 (59.56%)
    ' Comment Lines: 64 (28.44%)
    '    - Xml Docs: 84.38%
    ' 
    '   Blank Lines: 27 (12.00%)
    '     File Size: 10.15 KB


    '     Module linkLengthExtensions
    ' 
    '         Function: generateDirectedEdgeConstraints, getNeighbours, intersectionCount, stronglyConnectedComponents, unionCount
    ' 
    '         Sub: computeLinkLengths, jaccardLinkLengths, symmetricDiffLinkLengths
    '         Class NodeIndexer
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter
Imports any = System.Object
Imports stdNum = System.Math

Namespace Cola

    Module linkLengthExtensions

        ''' <summary>
        ''' compute the size of the union of two sets a and b
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function unionCount(a As Dictionary(Of Integer, any), b As Dictionary(Of Integer, any)) As Integer
            Return (a.Keys.AsList + b.Keys).Distinct.Count
        End Function

        ''' <summary>
        ''' compute the size of the intersection of two sets a and b
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Private Function intersectionCount(a As Dictionary(Of Integer, any), b As Dictionary(Of Integer, any)) As Integer
            Dim n = 0

            For Each i As Integer In a.Keys
                If b.ContainsKey(i) Then
                    n += 1
                End If
            Next

            Return n
        End Function

        Private Function getNeighbours(Of Link)(links As Link(), la As LinkAccessor(Of Link)) As Dictionary(Of Integer, Dictionary(Of Integer, Object))
            Dim neighbours As New Dictionary(Of Integer, Dictionary(Of Integer, Object))
            Dim addNeighbours = Sub(u As Integer, v As Integer)
                                    If neighbours(u) Is Nothing Then
                                        neighbours(u) = New Dictionary(Of Integer, any)
                                    End If
                                    neighbours(u)(v) = New Object
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
        Private Sub computeLinkLengths(Of Link)(links As Link(), w As Double, f As Func(Of Dictionary(Of Integer, any), Dictionary(Of Integer, any), Double), la As LinkAccessor(Of Link))
            Dim neighbours = getNeighbours(Of Link)(links, la)

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
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub symmetricDiffLinkLengths(Of Link)(links As Link(), la As LinkAccessor(Of Link), Optional w As Double = 1)
            computeLinkLengths(links, w, Function(a, b) stdNum.Sqrt(unionCount(a, b) - intersectionCount(a, b)), la)
        End Sub

        ''' <summary>
        ''' modify the specified links lengths based on the jaccard difference between their neighbours
        ''' </summary>
        ''' <typeparam name="Link"></typeparam>
        ''' <param name="links"></param>
        ''' <param name="la"></param>
        ''' <param name="w"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub jaccardLinkLengths(Of Link)(links As Link(), la As LinkAccessor(Of Link), Optional w As Double = 1)
            computeLinkLengths(links, w, Function(a, b) If(stdNum.Min(a.Keys.Count, b.Keys.Count) < 1.1, 0, intersectionCount(a, b) / unionCount(a, b)), la)
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
        Public Function generateDirectedEdgeConstraints(Of Link)(n As Double, links As Link(), axis As String, la As LinkAccessor(Of Link)) As List(Of IConstraint)
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
            Dim strongConnect As Action(Of NodeIndexer) =
                Sub(v As NodeIndexer)
                    ' Set the depth index for v to the smallest unused index
                    v.lowlink = stdNum.Max(Interlocked.Increment(index), index - 1)
                    v.index = v.lowlink
                    stack.Push(v)
                    v.onStack = True

                    ' Consider successors of v
                    For Each w As NodeIndexer In v.out
                        If w.index Is Nothing Then
                            ' Successor w has not yet been visited; recurse on it
                            strongConnect(w)
                            v.lowlink = stdNum.Min(v.lowlink, w.lowlink)
                        ElseIf w.onStack Then
                            ' Successor w is in stack S and hence in the current SCC
                            v.lowlink = stdNum.Min(v.lowlink, CType(w.index, Integer))
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
    End Module
End Namespace
