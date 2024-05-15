#Region "Microsoft.VisualBasic::cd8cd06196c5dec3e459c02f5ebf1860, gr\network-visualization\network_layout\Cola\GridRouter\gridrouter.vb"

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

    '   Total Lines: 777
    '    Code Lines: 580
    ' Comment Lines: 125
    '   Blank Lines: 72
    '     File Size: 37.28 KB


    '     Class GridRouter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: findAncestorPathBetween, findLineage, getDepth, getGridLines, getOrder
    '                   getRoutePath, getSegmentSets, isLeft, isStraight, makeSegments
    '                   orderEdges, route, routeEdges, siblingObstacles
    ' 
    '         Sub: nudgeSegments, nudgeSegs, unreverseEdges
    '         Class AncestorPath
    ' 
    ' 
    ' 
    '         Class Pair
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language.Python
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.My.JavaScript
Imports any = System.Object
Imports number = System.Double
Imports stdNum = System.Math

Namespace Cola

    Public Class GridRouter(Of Node)

        Private leaves As NodeWrapper() = Nothing
        Private groups As NodeWrapper()
        Private nodes As NodeWrapper()
        Private cols As GridLine()
        Private rows As GridLine()
        Private root As NodeWrapper
        Private verts As List(Of Vert)
        Private edges As List(Of Link3D)
        Private backToFront As NodeWrapper()
        Private obstacles As NodeWrapper()
        Private passableEdges As List(Of Link3D)

        ''' <summary>
        ''' in the given axis, find sets of leaves overlapping in that axis
        ''' center of each GridLine is average of all nodes in column
        ''' </summary>
        ''' <param name="axis"></param>
        ''' <returns></returns>
        Private Function getGridLines(axis As String) As List(Of GridLine)
            Dim columns As New List(Of GridLine)
            Dim ls = Me.leaves.slice(0, Me.leaves.Length).ToArray
            While ls.Length > 0
                ' find a column of all leaves overlapping in axis with the first leaf
                Dim overlapping As NodeWrapper() = ls.Where(Function(v) v.rect.OverlapLambda(axis)(ls(0).rect) > 0).ToArray
                Dim col = New GridLine With {
                .nodes = overlapping,
                .pos = overlapping.Select(Function(v) v.rect("c"c & axis)).Average
            }
                columns.Add(col)
                col.nodes.DoEach(Sub(v) ls.splice(ls.IndexOf(v), 1))
            End While
            columns.Sort(Function(a, b) a.pos - b.pos)
            Return columns
        End Function

        ''' <summary>
        ''' get the depth of the given node in the group hierarchy
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Private Function getDepth(v As any) As Double
            Dim depth = 0
            While v.parent <> Me.root
                depth += 1
                v = v.parent
            End While
            Return depth
        End Function

        Public originalnodes As Node()
        Public groupPadding As Double

        Public Sub New(originalnodes As Node(), accessor As NodeAccessor(Of Node), Optional groupPadding As Double = 12)
            Me.nodes = originalnodes _
                .Select(Function(v, i)
                            Return New NodeWrapper(i, accessor.getBounds(v), accessor.getChildren(v))
                        End Function) _
                .ToArray
            Me.leaves = Me.nodes.Where(Function(v) v.leaf).ToArray
            Me.groups = Me.nodes.Where(Function(g) Not g.leaf).ToArray
            Me.cols = Me.getGridLines("x").ToArray
            Me.rows = Me.getGridLines("y").ToArray

            ' create parents for each node or group that is a member of another's children
            Me.groups.DoEach(Sub(v) v.children.DoEach(Sub(c) Me.nodes(c).parent = v))

            ' root claims the remaining orphans
            Me.root = New NodeWrapper With {
                 .children = New List(Of Integer)
            }
            Me.nodes.DoEach(Sub(v)
                                If v.parent Is Nothing Then
                                    v.parent = Me.root
                                    Me.root.children.Add(v.id)
                                End If

                                ' each node will have grid vertices associated with it,
                                ' some inside the node and some on the boundary
                                ' leaf nodes will have exactly one internal node at the center
                                ' and four boundary nodes
                                ' groups will have potentially many of each
                                v.ports = New List(Of Vert)
                            End Sub)

            ' nodes ordered by their position in the group hierarchy
            Me.backToFront = Me.nodes.slice(0).ToArray
            Me.backToFront.Sort(Function(x, y) Me.getDepth(x) - Me.getDepth(y))

            ' compute boundary rectangles for each group
            ' has to be done from front to back, i.e. inside groups to outside groups
            ' such that each can be made large enough to enclose its interior
            Dim frontToBackGroups = Me.backToFront _
                .slice(0) _
                .Reverse() _
                .Where(Function(g) Not g.leaf) _
                .ToArray

            frontToBackGroups.DoEach(
                Sub(v)
                    Dim r = New Rectangle2D()
                    v.children.DoEach(Sub(c) r = r.Union(Me.nodes(c).rect))
                    v.rect = r.inflate(Me.groupPadding)
                End Sub)

            Dim colMids = BezierCurve.MidPoints(Me.cols.Select(Function(r) r.pos).ToArray).ToArray
            Dim rowMids = BezierCurve.MidPoints(Me.rows.Select(Function(r) r.pos).ToArray).ToArray

            ' setup extents of lines
            Dim rowx__1 = colMids(0)
            Dim rowX__2 = colMids(colMids.Length - 1)
            Dim coly__3 = rowMids(0)
            Dim colY__4 = rowMids(rowMids.Length - 1)

            ' horizontal lines
            Dim hlines = Me.rows.Select(Function(r) New LinkLine With {
             .X1 = rowx__1,
             .X2 = rowX__2,
             .Y1 = r.pos,
             .Y2 = r.pos
        }).Concat(rowMids.Select(Function(m) New LinkLine With {
             .X1 = rowx__1,
             .X2 = rowX__2,
             .Y1 = m,
             .Y2 = m
        })).ToArray

            ' vertical lines
            Dim vlines = Me.cols.Select(Function(c) New LinkLine With {
            .X1 = c.pos,
            .X2 = c.pos,
            .Y1 = coly__3,
            .Y2 = colY__4
        }).Concat(colMids.Select(Function(m) New LinkLine With {
            .X1 = m,
            .X2 = m,
            .Y1 = coly__3,
            .Y2 = colY__4
        })).ToArray

            ' the full set of lines
            Dim lines = hlines.Concat(vlines).ToArray

            ' we record the vertices associated with each line
            lines.DoEach(Sub(l) l.verts = New List(Of Vert))

            ' the routing graph
            Me.verts = New List(Of Vert)
            Me.edges = New List(Of Link3D)

            ' create vertices at the crossings of horizontal and vertical grid-lines
            hlines.DoEach(Sub(h)
                              vlines.DoEach(Sub(v)
                                                Dim p = New Vert(Me.verts.Count, v.X1, h.Y1)
                                                h.verts.Add(p)
                                                v.verts.Add(p)
                                                Me.verts.Add(p)

                                                ' assign vertices to the nodes immediately under them
                                                Dim i As Integer = Me.backToFront.Length
                                                While (System.Math.Max(Interlocked.Decrement(i), i + 1)) > 0
                                                    Dim node = Me.backToFront(i)
                                                    Dim r = node.rect
                                                    Dim dx = stdNum.Abs(p.X - r.CenterX())
                                                    Dim dy = stdNum.Abs(p.Y - r.CenterY())
                                                    If dx < r.Width() / 2 AndAlso dy < r.Height() / 2 Then
                                                        DirectCast(p, any).node = node
                                                        Exit While
                                                    End If
                                                End While
                                            End Sub)
                          End Sub)

            lines.ForEach(Sub(l, li)
                              ' create vertices at the intersections of nodes and lines
                              Me.nodes.ForEach(Sub(v, i)
                                                   v.rect _
                                                    .lineIntersections(l.X1, l.Y1, l.X2, l.Y2) _
                                                    .ForEach(Sub(intersect, j)
                                                                 'console.log(li+','+i+','+j+':'+intersect.x + ',' + intersect.y);
                                                                 Dim p = New Vert(Me.verts.Count, intersect.X, intersect.Y, v, l)
                                                                 Me.verts.Add(p)
                                                                 l.verts.Add(p)
                                                                 v.ports.Add(p)
                                                             End Sub)
                                               End Sub)

                              ' split lines into edges joining vertices
                              Dim isHoriz = stdNum.Abs(l.Y1 - l.Y2) < 0.1
                              Dim delta = Function(a As Vert, b As Vert)
                                              Return If(isHoriz, b.X - a.X, b.Y - a.Y)
                                          End Function
                              l.verts.Sort(delta)
                              For i As Integer = 1 To l.verts.Count - 1
                                  Dim u = l.verts(i - 1)
                                  Dim v = l.verts(i)

                                  If u.node IsNot Nothing AndAlso u.node Is v.node AndAlso u.node.leaf Then
                                      Continue For
                                  End If
                                  Me.edges.Add(New Link3D With {
                .source = u.id,
                .target = v.id,
                .length = stdNum.Abs(delta(u, v))
            })
                              Next
                          End Sub)
        End Sub

        ''' <summary>
        ''' find path from v to root including both v and root
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Private Function findLineage(v As NodeWrapper) As NodeWrapper()
            Dim lineage As New List(Of NodeWrapper) From {v}
            Do
                v = v.parent
                lineage.Add(v)
            Loop While v IsNot Me.root
            Return lineage.AsEnumerable.Reverse().ToArray
        End Function

        ''' <summary>
        ''' find path connecting a and b through their lowest common ancestor
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Private Function findAncestorPathBetween(a As NodeWrapper, b As NodeWrapper) As AncestorPath
            Dim aa = Me.findLineage(a)
            Dim ba = Me.findLineage(b)
            Dim i = 0
            While aa(i) Is ba(i)
                i += 1
            End While
            ' i-1 to include common ancestor only once (as first element)
            Return New AncestorPath With {
             .commonAncestor = aa(i - 1),
             .lineages = aa.slice(i).Concat(ba.slice(i)).ToArray
        }
        End Function

        Public Class AncestorPath
            Public commonAncestor As NodeWrapper
            Public lineages As NodeWrapper()
        End Class

        ''' <summary>
        ''' when finding a path between two nodes a and b, siblings of a and b on the
        ''' paths from a and b to their least common ancestor are obstacles
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Function siblingObstacles(a As NodeWrapper, b As NodeWrapper) As NodeWrapper()
            Dim path As AncestorPath = Me.findAncestorPathBetween(a, b)
            Dim lineageLookup = New Dictionary(Of String, Object)

            path.lineages.DoEach(Sub(v)
                                     lineageLookup(v.id.ToString) = Nothing
                                 End Sub)

            Dim obstacles = path _
                .commonAncestor _
                .children _
                .Where(Function(v) Not (lineageLookup.ContainsKey(v.ToString))) _
                .ToArray

            path.lineages _
                .Where(Function(v)
                           Return Not v.parent Is path.commonAncestor
                       End Function) _
                .DoEach(Sub(v)
                            obstacles = obstacles _
                                .Concat(v.parent.children.Where(Function(c) c <> v.id)) _
                                .ToArray
                        End Sub)

            Return obstacles.Select(Function(v) Me.nodes(v)).ToArray
        End Function

        ''' <summary>
        ''' for the given routes, extract all the segments orthogonal to the axis x
        ''' and return all them grouped by x position
        ''' </summary>
        ''' <param name="routes"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Private Shared Function getSegmentSets(routes As List(Of Segment()), x As Integer, y As Integer) As List(Of vsegmentsets)
            ' vsegments is a list of vertical segments sorted by x position
            Dim vsegments As New List(Of Segment)

            For ei As Integer = 0 To routes.Count - 1
                Dim route = routes(ei)
                For si As Integer = 0 To route.Length - 1
                    Dim s = route(si)
                    s.edgeid = ei
                    s.i = si
                    Dim sdx = s(1)(x) - s(0)(x)
                    If stdNum.Abs(sdx) < 0.1 Then
                        vsegments.Add(s)
                    End If
                Next
            Next
            vsegments.Sort(Function(a, b) a(0)(x) - b(0)(x))

            ' vsegmentsets is a set of sets of segments grouped by x position
            Dim vsegmentsets = New List(Of vsegmentsets)
            Dim segmentset As vsegmentsets = Nothing
            For i As Integer = 0 To vsegments.Count - 1
                Dim s As Segment = vsegments(i)
                If segmentset Is Nothing OrElse stdNum.Abs(s(0)(x) - segmentset.pos) > 0.1 Then
                    segmentset = New vsegmentsets With {
                    .pos = s(0)(x),
                    .segments = New List(Of Segment)
                }
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
        ''' + e1 = edge of s1, e2 = edge of s2
        ''' + if leftOf(e1,e2) create constraint s1.x + gap &lt;= s2.x
        ''' + else if leftOf(e2,e1) create cons. s2.x + gap &lt;= s1.x
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="routes"></param>
        ''' <param name="segments"></param>
        ''' <param name="leftOf"></param>
        ''' <param name="gap"></param>
        Private Shared Sub nudgeSegs(x As String, y As String, routes As List(Of Segment()), segments As List(Of Segment), leftOf As any, gap As Double)
            Dim n = segments.Count
            If n <= 1 Then
                Return
            End If
            Dim vs As Variable() = segments.Select(Function(s) New Variable(s(0)(x))).ToArray
            Dim cs As New List(Of Constraint)
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    If i = j Then
                        Continue For
                    End If
                    Dim s1 = segments(i)
                    Dim s2 = segments(j)
                    Dim e1 = s1.edgeid
                    Dim e2 = s2.edgeid
                    Dim lind = -1
                    Dim rind = -1
                    ' in page coordinates (not cartesian) the notion of 'leftof' is flipped in the horizontal axis from the vertical axis
                    ' that is, when nudging vertical segments, if they increase in the y(conj) direction the segment belonging to the
                    ' 'left' edge actually needs to be nudged to the right
                    ' when nudging horizontal segments, if the segments increase in the x direction
                    ' then the 'left' segment needs to go higher, i.e. to have y pos less than that of the right
                    If x = "x"c Then
                        If leftOf(e1, e2) Then
                            'console.log('s1: ' + s1[0][x] + ',' + s1[0][y] + '-' + s1[1][x] + ',' + s1[1][y]);
                            If s1(0)(y) < s1(1)(y) Then
                                lind = j
                                rind = i
                            Else
                                lind = i
                                rind = j
                            End If
                        End If
                    Else
                        If leftOf(e1, e2) Then
                            If s1(0)(y) < s1(1)(y) Then
                                lind = i
                                rind = j
                            Else
                                lind = j
                                rind = i
                            End If
                        End If
                    End If
                    If lind >= 0 Then
                        'console.log(x+' constraint: ' + lind + '<' + rind);
                        cs.Add(New Constraint(vs(lind), vs(rind), gap))
                    End If
                Next
            Next

            Dim solver = New Solver(vs, cs)

            Call solver.solve()
            Call vs.ForEach(Sub(v, i)
                                Dim s = segments(i)
                                Dim pos = v.position()
                                Dim route = routes(s.edgeid)

                                s(1)(x) = pos
                                s(0)(x) = s(1)(x)

                                If s.i > 0 Then
                                    route(s.i - 1)(1)(x) = pos
                                End If

                                If s.i < route.Length - 1 Then
                                    route(s.i + 1)(0)(x) = pos
                                End If
                            End Sub)
        End Sub

        Private Shared Sub nudgeSegments(routes As List(Of Segment()), x As String, y As String, leftOf As Func(Of Integer, Integer, Boolean), gap As Double)
            Dim vsegmentsets As List(Of vsegmentsets) = GridRouter(Of Node).getSegmentSets(routes, x, y)
            ' scan the grouped (by x) segment sets to find co-linear bundles
            For i As Integer = 0 To vsegmentsets.Count - 1
                Dim ss = vsegmentsets(i)
                Dim events = New List(Of [Event])
                For j As Integer = 0 To ss.segments.Count - 1
                    Dim s As Segment = ss.segments(j)
                    events.Add(New [Event] With {
                    .type = 0,
                    .s = s,
                    .pos = System.Math.Min(s(0)(y), s(1)(y))
                })
                    events.Add(New [Event] With {
                     .type = 1,
                     .s = s,
                     .pos = stdNum.Max(s(0)(y), s(1)(y))
                })
                Next
                events.Sort(Function(a, b) a.pos - b.pos + a.type - b.type)
                Dim open As New List(Of Segment)
                Dim openCount = 0
                events.DoEach(Sub(e)
                                  If e.type = 0 Then
                                      open.Add(e.s)
                                      openCount += 1
                                  Else
                                      openCount -= 1
                                  End If
                                  If openCount = 0 Then
                                      GridRouter(Of Node).nudgeSegs(x, y, routes, open, leftOf, gap)
                                      open = New List(Of Segment)
                                  End If
                              End Sub)
            Next
        End Sub

        ' obtain routes for the specified edges, nicely nudged apart
        ' warning: edge paths may be reversed such that common paths are ordered consistently within bundles!
        ' @param edges list of edges
        ' @param nudgeGap how much to space parallel edge segements
        ' @param source function to retrieve the index of the source node for a given edge
        ' @param target function to retrieve the index of the target node for a given edge
        ' @returns an array giving, for each edge, an array of segments, each segment a pair of points in an array
        Public Function routeEdges(Of Edge)(edges As Edge(), nudgeGap As Double, source As Func(Of Edge, Integer), target As Func(Of Edge, Integer)) As List(Of Segment())
            Dim routePaths As Vert()() = edges.Select(Function(e) Me.route(source(e), target(e))).ToArray
            Dim order = GridRouter(Of Node).orderEdges(routePaths)
            Dim routes As List(Of Segment()) = routePaths.Select(Function(e) GridRouter(Of Node).makeSegments(e).ToArray).ToList
            GridRouter(Of Node).nudgeSegments(routes, "x"c, "y"c, order, nudgeGap)
            GridRouter(Of Node).nudgeSegments(routes, "y"c, "x"c, order, nudgeGap)
            GridRouter(Of Node).unreverseEdges(routes, routePaths)
            Return routes
        End Function

        ' path may have been reversed by the subsequence processing in orderEdges
        ' so now we need to restore the original order
        Private Shared Sub unreverseEdges(routes As List(Of Segment()), routePaths As Point2D()())
            routes.ForEach(Sub(segments, i)
                               Dim path = routePaths(i)
                               If DirectCast(path, any).reversed Then
                                   segments.Reverse()
                                   ' reverse order of segments
                                   ' reverse each segment
                                   segments.DoEach(Sub(segment) segment.Reverse())
                               End If
                           End Sub)
        End Sub

        ' does the path a-b-c describe a left turn?
        Private Shared Function isLeft(a As Point2D, b As Point2D, c As Point2D) As Boolean
            Return ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) <= 0
        End Function

        Public Class Pair
            Public l As Integer
            Public r As Integer
        End Class

        ' for the given list of ordered pairs, returns a function that (efficiently) looks-up a specific pair to
        ' see if it exists in the list
        Private Shared Function getOrder(pairs As List(Of Pair)) As Func(Of Integer, Integer, Boolean)
            Dim outgoing = New Object() {}
            For i As Integer = 0 To pairs.Count - 1
                Dim p As Pair = pairs(i)
                If outgoing(p.l) Is Nothing Then
                    outgoing(p.l) = New any() {}
                End If
                outgoing(p.l)(p.r) = True
            Next
            Return Function(l, r) outgoing(l) IsNot Nothing AndAlso outgoing(l)(r)
        End Function

        ' returns an ordering (a lookup function) that determines the correct order to nudge the
        ' edge paths apart to minimize crossings
        Private Shared Function orderEdges(edges As Vert()()) As Func(Of Integer, Integer, Boolean)
            Dim edgeOrder As New List(Of Pair)

            For i As Integer = 0 To edges.Length - 2
                For j As Integer = i + 1 To edges.Length - 1
                    Dim e = edges(i)
                    Dim f = edges(j)
                    Dim lcs = New LongestCommonSubsequence(Of Point2D)(e, f, AddressOf Point2D.Equals)
                    Dim u As Point2D, vi As Point2D, vj As Point2D

                    If lcs.length = 0 Then
                        Continue For
                    End If

                    ' no common subpath
                    If lcs.reversed Then
                        ' if we found a common subpath but one of the edges runs the wrong way,
                        ' then reverse f.
                        f = f.Reverse().ToArray
                        ' f.reversed = True
                        lcs = New LongestCommonSubsequence(Of Point2D)(e, f, AddressOf Point2D.Equals)
                    End If

                    If (lcs.si <= 0 OrElse lcs.ti <= 0) AndAlso (lcs.si + lcs.length >= e.Length OrElse lcs.ti + lcs.length >= f.Length) Then
                        ' the paths do not diverge, so make an arbitrary ordering decision
                        edgeOrder.Add(New Pair With {.l = i, .r = j})
                        Continue For
                    End If

                    If lcs.si + lcs.length >= e.Length OrElse lcs.ti + lcs.length >= f.Length Then
                        ' if the common subsequence of the
                        ' two edges being considered goes all the way to the
                        ' end of one (or both) of the lines then we have to
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

                    If isLeft(u, vi, vj) Then
                        edgeOrder.Add(New Pair With {.l = j, .r = i})
                    Else
                        edgeOrder.Add(New Pair With {.l = i, .r = j})
                    End If
                Next
            Next
            'edgeOrder.forEach(function (e) { console.log('l:' + e.l + ',r:' + e.r) });
            Return GridRouter(Of Node).getOrder(edgeOrder)
        End Function

        Private Shared Function isStraight(a As Point2D, b As Point2D, c As Point2D) As Boolean
            Return stdNum.Abs((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) < 0.001
        End Function

        ' for an orthogonal path described by a sequence of points, create a list of segments
        ' if consecutive segments would make a straight line they are merged into a single segment
        ' segments are over cloned points, not the original vertices
        Private Shared Function makeSegments(path As Point2D()) As List(Of Segment)
            Dim copyPoint = Function(p As Point2D) New Point2D(p)
            Dim segments = New List(Of Segment)
            Dim a = copyPoint(path(0))

            For i As Integer = 1 To path.Length - 1
                Dim b = copyPoint(path(i))
                Dim c = If(i < path.Length - 1, path(i + 1), Nothing)

                If c Is Nothing OrElse Not isStraight(a, b, c) Then
                    segments.Add(New Segment With {.Points = {a, b}})
                    a = b
                End If
            Next

            Return segments
        End Function

        ' find a route between node s and node t
        ' returns an array of indices to verts
        Public Function route(s As Integer, t As Integer) As Vert()
            Dim source = Me.nodes(s)
            Dim target = Me.nodes(t)
            Me.obstacles = Me.siblingObstacles(source, target)

            Dim obstacleLookup As New Dictionary(Of String, Object)
            Me.obstacles.DoEach(Sub(o) obstacleLookup(o.id.ToString) = o)
            Me.passableEdges = Me.edges _
                .Where(Function(e)
                           Dim u = Me.verts(e.source)
                           Dim v = Me.verts(e.target)
                           Return Not (u.node IsNot Nothing AndAlso obstacleLookup.ContainsKey(u.node.id.ToString) OrElse v.node IsNot Nothing AndAlso obstacleLookup.ContainsKey(v.node.id.ToString))
                       End Function) _
                .ToList

            ' add dummy segments linking ports inside source and target
            For i As Integer = 1 To source.ports.Count - 1
                Dim u = source.ports(0).id
                Dim v = source.ports(i).id
                Me.passableEdges.Add(New Link3D With {
                .source = u,
                .target = v,
                .length = 0
            })
            Next
            For i As Integer = 1 To target.ports.Count - 1
                Dim u = target.ports(0).id
                Dim v = target.ports(i).id
                Me.passableEdges.Add(New Link3D With {
                 .source = u,
                 .target = v,
                 .length = 0
            })
            Next

            Dim getSource = Function(e As Link3D) e.source
            Dim getTarget = Function(e As Link3D) e.target
            Dim getLength = Function(e As Link3D) e.length

            Dim shortestPathCalculator = New Dijkstra.Calculator(Of Link3D)(Me.verts.Count, Me.passableEdges, getSource, getTarget, getLength)
            Dim bendPenalty = Function(u As Integer, v As Integer, w As Integer) As Double
                                  Dim a = Me.verts(u)
                                  Dim b = Me.verts(v)
                                  Dim c = Me.verts(w)
                                  Dim dx = stdNum.Abs(c.X - a.X)
                                  Dim dy = stdNum.Abs(c.Y - a.Y)

                                  ' don't count bends from internal node edges
                                  If a.node Is source AndAlso a.node Is b.node OrElse b.node Is target AndAlso b.node Is c.node Then
                                      Return 0
                                  End If

                                  Return If(dx > 1 AndAlso dy > 1, 1000, 0)
                              End Function

            ' get shortest path
            Dim shortestPath = shortestPathCalculator.PathFromNodeToNodeWithPrevCost(
                source.ports(0).id,
                target.ports(0).id,
                bendPenalty
            )

            ' shortest path is reversed and does not include the target port
            Dim pathPoints As List(Of Vert) = shortestPath _
                .AsEnumerable _
                .Reverse() _
                .Select(Function(vi) Me.verts(vi)) _
                .ToList

            Call pathPoints.Add(Me.nodes(target.id).ports(0))

            ' filter out any extra end points that are inside the source or target (i.e. the dummy segments above)
            Return pathPoints _
                .Where(Function(v, i)
                           Return Not (i < pathPoints.Count - 1 AndAlso
                               pathPoints(i + 1).node Is source AndAlso
                               v.node Is source OrElse
                               i > 0 AndAlso
                               v.node Is target AndAlso
                               pathPoints(i - 1).node Is target)
                       End Function) _
                .ToArray
        End Function

        Public Shared Function getRoutePath(route As Point2D()(), cornerradius As Double, arrowwidth As Double, arrowheight As Double) As SVGRoutePath
            Dim result As New SVGRoutePath With {
             .routepath = "M " & route(0)(0).X & " "c & route(0)(0).Y & " "c,
             .arrowpath = ""
        }
            If route.Length > 1 Then
                For i As Integer = 0 To route.Length - 1
                    Dim li = route(i)
                    Dim x = li(1).X
                    Dim y = li(1).Y
                    Dim dx = x - li(0).X
                    Dim dy = y - li(0).Y
                    If i < route.Length - 1 Then
                        If stdNum.Abs(dx) > 0 Then
                            x -= dx / stdNum.Abs(dx) * cornerradius
                        Else
                            y -= dy / stdNum.Abs(dy) * cornerradius
                        End If
                        result.routepath += "L " & x & " "c & y & " "c
                        Dim l = route(i + 1)
                        Dim x0 = l(0).X
                        Dim y0 = l(0).Y
                        Dim x1 = l(1).X
                        Dim y1 = l(1).Y
                        dx = x1 - x0
                        dy = y1 - y0
                        Dim angle = If(GeometryMath.angleBetween2Lines(li, l) < 0, 1, 0)
                        'console.log(cola.GridRouter.angleBetween2Lines(li, l))
                        Dim x2
                        Dim y2
                        If stdNum.Abs(dx) > 0 Then
                            x2 = x0 + dx / stdNum.Abs(dx) * cornerradius
                            y2 = y0
                        Else
                            x2 = x0
                            y2 = y0 + dy / stdNum.Abs(dy) * cornerradius
                        End If
                        Dim cx = stdNum.Abs(x2 - x)
                        Dim cy = stdNum.Abs(y2 - y)
                        result.routepath += "A " & Convert.ToString(cx) & " " & Convert.ToString(cy) & " 0 0 " & angle & " " & Convert.ToString(x2) & " " & Convert.ToString(y2) & " "
                    Else
                        Dim arrowtip = New any() {x, y}
                        Dim arrowcorner1
                        Dim arrowcorner2
                        If stdNum.Abs(dx) > 0 Then
                            x -= dx / stdNum.Abs(dx) * arrowheight
                            arrowcorner1 = New number() {x, y + arrowwidth}
                            arrowcorner2 = New number() {x, y - arrowwidth}
                        Else
                            y -= dy / stdNum.Abs(dy) * arrowheight
                            arrowcorner1 = New number() {x + arrowwidth, y}
                            arrowcorner2 = New number() {x - arrowwidth, y}
                        End If
                        result.routepath += "L " & x & " "c & y & " "c
                        If arrowheight > 0 Then
                            result.arrowpath = (((("M " & Convert.ToString(arrowtip(0)) & " "c & Convert.ToString(arrowtip(1)) & " L ") + arrowcorner1(0) & " "c) + arrowcorner1(1) & " L ") + arrowcorner2(0) & " "c) + arrowcorner2(1)
                        End If
                    End If
                Next
            Else
                Dim li = route(0)
                Dim x = li(1).X
                Dim y = li(1).Y
                Dim dx = x - li(0).X
                Dim dy = y - li(0).Y
                Dim arrowtip = New number() {x, y}
                Dim arrowcorner1
                Dim arrowcorner2
                If stdNum.Abs(dx) > 0 Then
                    x -= dx / stdNum.Abs(dx) * arrowheight
                    arrowcorner1 = New number() {x, y + arrowwidth}
                    arrowcorner2 = New number() {x, y - arrowwidth}
                Else
                    y -= dy / stdNum.Abs(dy) * arrowheight
                    arrowcorner1 = New number() {x + arrowwidth, y}
                    arrowcorner2 = New number() {x - arrowwidth, y}
                End If
                result.routepath += "L " & x & " "c & y & " "c
                If arrowheight > 0 Then
                    result.arrowpath = (((("M " & arrowtip(0) & " "c & arrowtip(1) & " L ") + arrowcorner1(0) & " "c) + arrowcorner1(1) & " L ") + arrowcorner2(0) & " "c) + arrowcorner2(1)
                End If
            End If
            Return result
        End Function
    End Class
End Namespace
