Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports number = System.Double
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Python
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming

Namespace Layouts.Cola.GridRouter

    Public Class GridRouter(Of Node)

        Public leaves As NodeWrapper()
        Public groups As NodeWrapper()
        Public nodes As NodeWrapper()
        Public cols As GridLine()
        Public rows As GridLine()
        Public root As NodeWrapper
        Public verts As Vert()
        Public edges
        Public backToFront
        Public obstacles
        Public passableEdges

        ''' <summary>
        ''' get the depth of the given node in the group hierarchy
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Function getDepth(v As NodeWrapper) As Double
            Dim depth As Integer = 0

            While (Not v.parent Is root)
                depth += 1
                v = v.parent
            End While

            Return depth
        End Function

        ''' <summary>
        ''' in the given axis, find sets of leaves overlapping in that axis
        ''' center of each GridLine Is average of all nodes in column
        ''' </summary>
        ''' <param name="axisOverlap"></param>
        ''' <returns></returns>
        Private Function getGridLines(axisOverlap As Func(Of Rectangle2D, Rectangle2D, Double), axisCenter As Func(Of Rectangle2D, Double)) As GridLine()
            Dim columns As New List(Of GridLine)
            Dim ls = leaves.ToList

            While (ls.Count > 0)
                ' find a column of all leaves overlapping in axis with the first leaf
                Dim overlapping As NodeWrapper() = ls.Where(Function(v) axisOverlap(v.rect, ls(0).rect) <> 0).ToArray
                Dim col As New GridLine With {
                    .nodes = overlapping,
                    .pos = overlapping.Select(Function(v) axisCenter(v.rect)).Average
                }
                columns.Add(col)

                For Each v In col.nodes
                    Call ls.Remove(v)
                Next
            End While

            Call columns.Sort(New GridLine.Comparer)

            Return columns.ToArray
        End Function

        ''' <summary>
        ''' find path from v to root including both v And root
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Private Function findLineage(v As NodeWrapper) As NodeWrapper()
            Dim lineage As New List(Of NodeWrapper) From {v}
            Do
                v = v.parent
                lineage.Add(v)
            Loop While (Not v Is root)

            Return lineage.ReverseIterator.ToArray
        End Function

        ''' <summary>
        ''' find path connecting a And b through their lowest common ancestor
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Private Function findAncestorPathBetween(a As NodeWrapper, b As NodeWrapper) As (commonAncestor As NodeWrapper, lineages As NodeWrapper())
            Dim aa = findLineage(a), ba = findLineage(b)
            Dim i = 0

            While (aa(i) Is ba(i))
                i += 1
            End While

            ' i-1 to include common ancestor only once (as first element)
            Return (commonAncestor:=aa(i - 1), lineages:=aa.slice(i).Concat(ba.slice(i)).ToArray)
        End Function

        ''' <summary>
        ''' when finding a path between two nodes a And b, siblings of a And b on the
        ''' paths from a And b to their least common ancestor are obstacles
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Function siblingObstacles(a As NodeWrapper, b As NodeWrapper) As NodeWrapper()
            Dim path = findAncestorPathBetween(a, b)
            Dim lineageLookup As New Index(Of Integer)
            path.lineages.ForEach(Sub(v, i)
                                      lineageLookup.Add(v.id)
                                  End Sub)
            Dim obstacles = path.commonAncestor.children.Where(Function(v) lineageLookup.NotExists(v)).ToArray

            path.lineages.Where(Function(v) Not v.parent Is path.commonAncestor).ForEach(Sub(v, i)
                                                                                             obstacles = obstacles.Concat(v.parent.children.Where(Function(C) C = v.id))
                                                                                         End Sub)

            Return obstacles.Select(Function(v) nodes(v)).ToArray
        End Function

        ''' <summary>
        ''' for the given routes, extract all the segments orthogonal to the axis x
        ''' And return all them grouped by x position
        ''' </summary>
        ''' <param name="routes"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Shared Function getSegmentSets(routes As route()(), x%, y%) As List(Of segmentset)
            ' vsegments Is a list of vertical segments sorted by x position
            Dim vsegments As New List(Of route)

            For ei As Integer = 0 To routes.Length - 1
                Dim route = routes(ei)

                For si As Integer = 0 To route.Length - 1
                    Dim s = route(si)
                    s.edgeid = ei
                    s.i = si
                    Dim sdx = s(1)(x) - s(0)(x)
                    If (Math.Abs(sdx) < 0.1) Then
                        vsegments.Add(s)
                    End If
                Next
            Next

            vsegments.Sort(New route.Comparer With {.i = x})

            ' vsegmentsets Is a set of sets of segments grouped by x position
            Dim vsegmentsets As New List(Of segmentset)
            Dim segmentset As segmentset = Nothing

            For i As Integer = 0 To vsegments.Count - 1
                Dim s = vsegments(i)
                If (segmentset Is Nothing OrElse Math.Abs(s(0)(x) - segmentset.pos) > 0.1) Then
                    segmentset = New segmentset With {.pos = s(0)(x), .segments = New List(Of route)}
                    vsegmentsets.Add(segmentset)
                End If
                segmentset.segments.Add(s)
            Next

            Return vsegmentsets
        End Function

        ''' <summary>
        ''' for all segments in this bundle create a vpsc problem such that
        ''' each segment's x position is a variable and separation constraints
        ''' are given by the partial order over the edges to which the segments belong
        ''' for each pair s1,s2 of segments in the open set:
        ''' 
        ''' ```
        ''' e1 = edge of s1, e2 = edge of s2
        ''' if leftOf(e1,e2) create constraint s1.x + gap &lt;= s2.x
        ''' else if leftOf(e2,e1) create cons. s2.x + gap &lt;= s1.x
        ''' ```
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="routes"></param>
        ''' <param name="segments"></param>
        ''' <param name="leftOf"></param>
        ''' <param name="gap"></param>
        Public Shared Sub nudgeSegs(x As String, y As String, routes As route()(), segments As route(), leftOf As Func(Of Integer, Integer, Boolean), gap As number)
            Dim n = segments.Length
            If (n <= 1) Then Return
            Dim vs = segments.Select(Function(s) New Variable(s(0)(x))).ToArray
            Dim cs As New List(Of Constraint)
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    If (i = j) Then Continue For
                    Dim s1 = segments(i),
                        s2 = segments(j),
                        e1 = s1.edgeid,
                        e2 = s2.edgeid,
                        lind = -1,
                        rind = -1
                    ' in page coordinates (Not cartesian) the notion of 'leftof' is flipped in the horizontal axis from the vertical axis
                    ' that Is, when nudging vertical segments, if they increase in the y(conj) direction the segment belonging to the
                    ' 'left' edge actually needs to be nudged to the right
                    ' when nudging horizontal segments, if the segments increase in the x direction
                    ' then the 'left' segment needs to go higher, i.e. to have y pos less than that of the right
                    If (x = "x") Then
                        If (leftOf(e1, e2)) Then
                            ' console.log('s1: ' + s1[0][x] + ',' + s1[0][y] + '-' + s1[1][x] + ',' + s1[1][y]);
                            If (s1(0)(y) < s1(1)(y)) Then
                                lind = j
                                rind = i
                            Else
                                lind = i
                                rind = j
                            End If
                        End If
                    Else
                        If (leftOf(e1, e2)) Then
                            If (s1(0)(y) < s1(1)(y)) Then
                                lind = i
                                rind = j
                            Else
                                lind = j
                                rind = i
                            End If
                        End If
                    End If
                    If (lind >= 0) Then
                        ' console.log(x+' constraint: ' + lind + '<' + rind);
                        cs.Add(New Constraint(vs(lind), vs(rind), gap))
                    End If
                Next
            Next
            Dim Solver = New Solver(vs, cs)
            Solver.solve()
            vs.ForEach(Sub(v, i)
                           Dim s = segments(i)
                           Dim pos = v.position()
                           s(0)(x) = s(1)(x) = pos
                           Dim route = routes(s.edgeid)
                           If (s.i > 0) Then route(s.i - 1)(1)(x) = pos
                           If (s.i < route.Length - 1) Then route(s.i + 1)(0)(x) = pos
                       End Sub)
        End Sub

        Public Shared Sub nudgeSegments(routes As route()(), x As String, y As String, leftOf As Func(Of Integer, Integer, Boolean), gap As number)
            Dim vsegmentsets = getSegmentSets(routes, x, y)
            ' scan the grouped (by x) segment sets to find co-linear bundles
            For i As Integer = 0 To vsegmentsets.Count - 1
                Dim ss = vsegmentsets(i)
                Dim events = New List(Of [Event])
                For j As Integer = 0 To ss.segments.Count - 1
                    Dim s = ss.segments(j)
                    events.Add(New [Event] With {.type = 0, .s = s, .pos = System.Math.Min(s(0)(y), s(1)(y))})
                    events.Add(New [Event] With {.type = 1, .s = s, .pos = System.Math.Max(s(0)(y), s(1)(y))})
                Next
                events.Sort(New [Event].Comparer)
                Dim open As New List(Of route)
                Dim openCount = 0
                events.ForEach(Sub(e)
                                   If (e.type = 0) Then
                                       open.Add(e.s)
                                       openCount += 1
                                   Else
                                       openCount -= 1
                                   End If
                                   If (openCount = 0) Then
                                       nudgeSegs(x, y, routes, open, leftOf, gap)
                                       open = New List(Of route)
                                   End If
                               End Sub)
            Next
        End Sub

        ''' <summary>
        ''' path may have been reversed by the subsequence processing in orderEdges
        ''' so now we need to restore the original order
        ''' </summary>
        ''' <param name="routes"></param>
        ''' <param name="routePaths"></param>
        Public Shared Sub unreverseEdges(routes, routePaths)
            routes.forEach(Sub(segments, i)
                               Dim path = routePaths(i)
                               If (path.reversed) Then
                                   segments.reverse() ' reverse order Of segments
                                   segments.forEach(Function(segment)
                                                        segment.reverse()  ' reverse Each segment
                                                    End Function)
                               End If
                           End Sub)
        End Sub

        ''' <summary>
        ''' obtain routes for the specified edges, nicely nudged apart
        ''' warning: edge paths may be reversed such that common paths are ordered consistently within bundles!
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="edges">list of edges</param>
        ''' <param name="nudgeGap">how much to space parallel edge segements</param>
        ''' <param name="source">function to retrieve the index of the source node for a given edge</param>
        ''' <param name="target">function to retrieve the index of the target node for a given edge</param>
        ''' <returns>an array giving, for each edge, an array of segments, each segment a pair of points in an array</returns>
        Public Function routeEdges(Of T)(edges As T(), nudgeGap As number, source As Func(Of T, number), target As Func(Of T, number)) As Point2D()()()
            'Dim routePaths = edges.Select(Function(e) New route(source(e), target(e))).toarray
            'Dim order = orderEdges(routePaths)
            'Dim routes As Point2D()()() = routePaths.select(Function(e) makeSegments(e)).toarray
            'nudgeSegments(routes, "x", "y", order, nudgeGap)
            'nudgeSegments(routes, "y", "x", order, nudgeGap)
            'unreverseEdges(routes, routePaths)
            'Return routes
        End Function

        ''' <summary>
        ''' does the path a-b-c describe a left turn?
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="C"></param>
        ''' <returns></returns>
        Private Shared Function isLeft(a As Point2D, b As Point2D, C As Point2D)
            Return ((b.X - a.X) * (C.Y - a.Y) - (b.Y - a.Y) * (C.X - a.X)) <= 0
        End Function

        ''' <summary>
        ''' returns an ordering (a lookup function) that determines the correct order to nudge the
        ''' edge paths apart to minimize crossings
        ''' </summary>
        ''' <param name="edges"></param>
        ''' <returns></returns>
        Public Shared Function orderEdges(edges As Point2D()()) As Func(Of Integer, Integer, Boolean)
            Dim edgeOrder As New List(Of (Integer, Integer))

            For i As Integer = 0 To edges.Length - 2
                For j As Integer = i + 1 To edges.Length - 1
                    Dim e = edges(i),
                        f = edges(j),
                        lcs = New LongestCommonSubsequence(Of Point2D)(e, f, AddressOf Point2D.Equals)
                    Dim u, vi, vj
                    If (lcs.length = 0) Then
                        Continue For ' no common subpath
                    End If
                    If (lcs.reversed) Then
                        ' if we found a common subpath but one of the edges runs the wrong way,
                        ' then reverse f.
                        f.Reverse()
                        ' f.reversed = True
                        lcs = New LongestCommonSubsequence(Of Point2D)(e, f, AddressOf Point2D.Equals)
                    End If
                    If ((lcs.si <= 0 OrElse lcs.ti <= 0) AndAlso (lcs.si + lcs.length >= e.Length OrElse lcs.ti + lcs.length >= f.Length)) Then
                        ' the paths do Not diverge, so make an arbitrary ordering decision
                        edgeOrder.Add((i, j))
                        Continue For
                    End If
                    If (lcs.si + lcs.length >= e.Length OrElse lcs.ti + lcs.length >= f.Length) Then
                        ' if the common subsequence of the
                        ' two edges being considered goes all the way to the
                        ' end of one (Or both) of the lines then we have to
                        ' base our ordering decision on the other end of the
                        ' common subsequence
                        u = e(lcs.si + 1)
                        vj = e(lcs.si - 1)
                        vi = f(lcs.ti - 1)
                    Else
                        u = e(lcs.si + lcs.length - 2)
                        vi = e(lcs.si + lcs.length)
                        vj = f(lcs.ti + lcs.length)
                    End If

                    If (isLeft(u, vi, vj)) Then
                        edgeOrder.Add((j, i))
                    Else
                        edgeOrder.Add((i, j))
                    End If
                Next
            Next
            ' edgeOrder.forEach(function (e) { console.log('l:' + e.l + ',r:' + e.r) });
            Return getOrder(edgeOrder)
        End Function

        Private Shared Function isStraight(a As Point2D, b As Point2D, C As Point2D) As Boolean
            Return Math.Abs((b.X - a.X) * (C.Y - a.Y) - (b.Y - a.Y) * (C.X - a.X)) < 0.001
        End Function

        ''' <summary>
        ''' for an orthogonal path described by a sequence of points, create a list of segments
        ''' if consecutive segments would make a straight line they are merged into a single segment
        ''' segments are over cloned points, Not the original vertices
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Public Shared Function makeSegments(path As Point2D()) As Point2D()()
            Dim segments As New List(Of Point2D())
            Dim a = New Point2D(path(0))
            For i As Integer = 1 To path.Length - 1
                Dim b = New Point2D(path(i))
                Dim C = If(i < path.Length - 1, path(i + 1), Nothing)
                If (C Is Nothing OrElse Not isStraight(a, b, C)) Then
                    segments.Add({a, b})
                    a = b
                End If
            Next
            Return segments.ToArray
        End Function

        ''' <summary>
        ''' for the given list of ordered pairs, returns a function that (efficiently) looks-up a specific pair to
        ''' see if it exists in the list
        ''' </summary>
        ''' <param name="pairs"></param>
        ''' <returns></returns>
        Private Shared Function getOrder(pairs As List(Of (l As Integer, r As Integer))) As Func(Of Integer, Integer, Boolean)
            Dim outgoing As Boolean()() = {}
            For i As Integer = 0 To pairs.Count - 1
                Dim p = pairs(i)
                If (outgoing(p.l) Is Nothing) Then outgoing(p.l) = {}
                outgoing(p.l)(p.r) = True
            Next
            Return Function(l, r) Not outgoing(l) Is Nothing AndAlso outgoing(l)(r)
        End Function
    End Class

    Public Class segmentset
        Public pos As number
        Public segments As List(Of route)

    End Class

    Public Class route
        Public edgeid As Integer
        Public i As Integer

        Public matrix As Dictionary(Of String, Double)()

        Default Public Property item(i As Integer) As Dictionary(Of String, Double)
            Get
                Return matrix(i)
            End Get
            Set
                matrix(i) = Value
            End Set
        End Property

        Public ReadOnly Property length As Integer
            Get
                Return matrix.Length
            End Get
        End Property

        Public Structure Comparer : Implements IComparer(Of route)

            Public Property i As Integer

            Public Function Compare(x As route, y As route) As Integer Implements IComparer(Of route).Compare
                Return x(0)(i) - y(0)(i)
            End Function
        End Structure
    End Class
End Namespace