Imports number = System.Double
Imports any = System.Object

Interface NodeAccessor(Of Node)
	Function getChildren(v As Node) As number()
	Function getBounds(v As Node) As Rectangle
End Interface
Class NodeWrapper
	Private leaf As Boolean
	Private parent As NodeWrapper
	Private ports As Vert()

	Public id As number
	Public rect As Rectangle
	Public children As number()

	Private Sub New(id As number, rect As Rectangle, children As number())
		Me.leaf = children Is Nothing OrElse children.length = 0
	End Sub
End Class
Class Vert

	Public id As number
	Public x As number, y As number
	Public node As NodeWrapper = Nothing
	Public line As Object = Nothing

	Public Sub New(id As number, x As number, y As number, Optional node As NodeWrapper = Nothing, Optional line As Object = Nothing)
	End Sub
End Class


' a horizontal or vertical line of nodes
Class GridLine
	Public nodes As NodeWrapper()
	Public pos As number
End Class
Class GridRouter(Of Node)
	Private leaves As NodeWrapper() = Nothing
	Private groups As NodeWrapper()
	Private nodes As NodeWrapper()
	Private cols As GridLine()
	Private rows As GridLine()
	Private root As any
	Private verts As Vert()
	Private edges As any
	Private backToFront As any
	Private obstacles As any
	Private passableEdges As any
	Private Function avg(a As any()) As number
		Return a.reduce(Function(x, y) x + y) / a.length
	End Function

	' in the given axis, find sets of leaves overlapping in that axis
	' center of each GridLine is average of all nodes in column
	Private Function getGridLines(axis As String) As GridLine()
		Dim columns = New Object() {}
		Dim ls = Me.leaves.slice(0, Me.leaves.length)
		While ls.length > 0
			' find a column of all leaves overlapping in axis with the first leaf
			Dim overlapping = ls.filter(Function(v) v.rect("overlap" & axis.toUpperCase())(ls(0).rect))
			Dim col = New With { _
				Key .nodes = overlapping, _
				Key .pos = Me.avg(overlapping.map(Function(v) v.rect("c"C & axis)())) _
			}
			columns.push(col)
			col.nodes.forEach(Function(v) ls.splice(ls.indexOf(v), 1))
		End While
		columns.sort(Function(a, b) a.pos - b.pos)
		Return columns
	End Function

	' get the depth of the given node in the group hierarchy
	Private Function getDepth(v As any) As number
		Dim depth = 0
		While v.parent <> Me.root
			depth += 1
			v = v.parent
		End While
		Return depth
	End Function

	' medial axes between node centres and also boundary lines for the grid
	Private Function midPoints(a As number()) As number()
		Dim gap = a(1) - a(0)
		Dim mids = New Double() {a(0) - gap / 2}
		For i As var = 1 To a.length - 1
			mids.push((a(i) + a(i - 1)) / 2)
		Next
		mids.push(a(a.length - 1) + gap / 2)
		Return mids
	End Function

	Public originalnodes As Node()
	Public groupPadding As number

	Public Sub New(originalnodes As Node(), accessor As NodeAccessor(Of Node), Optional groupPadding As number = 12)
		Me.nodes = originalnodes.map(Function(v, i) New NodeWrapper(i, accessor.getBounds(v), accessor.getChildren(v)))
		Me.leaves = Me.nodes.filter(Function(v) v.leaf)
		Me.groups = Me.nodes.filter(Function(g) Not g.leaf)
		Me.cols = Me.getGridLines("x"C)
		Me.rows = Me.getGridLines("y"C)

		' create parents for each node or group that is a member of another's children
		Me.groups.forEach(Function(v) v.children.forEach(Function(c) InlineAssignHelper(Me.nodes(CType(c, number)).parent, v)))

		' root claims the remaining orphans
		Me.root = New With { _
			Key .children = New Object() {} _
		}
		Me.nodes.forEach(Function(v) 
		If v.parent Is Nothing Then
			v.parent = Me.root
			Me.root.children.push(v.id)
		End If

		' each node will have grid vertices associated with it,
		' some inside the node and some on the boundary
		' leaf nodes will have exactly one internal node at the center
		' and four boundary nodes
		' groups will have potentially many of each
		v.ports = New Object() {}

End Function)

		' nodes ordered by their position in the group hierarchy
		Me.backToFront = Me.nodes.slice(0)
		Me.backToFront.sort(Function(x, y) Me.getDepth(x) - Me.getDepth(y))

		' compute boundary rectangles for each group
		' has to be done from front to back, i.e. inside groups to outside groups
		' such that each can be made large enough to enclose its interior
		Dim frontToBackGroups = Me.backToFront.slice(0).reverse().filter(Function(g) Not g.leaf)
		frontToBackGroups.forEach(Function(v) 
		Dim r = Rectangle.empty()
		v.children.forEach(Function(c) InlineAssignHelper(r, r.union(Me.nodes(c).rect)))
		v.rect = r.inflate(Me.groupPadding)

End Function)

		Dim colMids = Me.midPoints(Me.cols.map(Function(r) r.pos))
		Dim rowMids = Me.midPoints(Me.rows.map(Function(r) r.pos))

		' setup extents of lines
		Dim rowx__1 = colMids(0)
		Dim rowX__2 = colMids(colMids.length - 1)
		Dim coly__3 = rowMids(0)
		Dim colY__4 = rowMids(rowMids.length - 1)

		' horizontal lines
		Dim hlines = Me.rows.map(Function(r) New With { _
			Key .x1 = rowx__1, _
			Key .x2 = rowX__2, _
			Key .y1 = r.pos, _
			Key .y2 = r.pos _
		}).concat(rowMids.map(Function(m) New With { _
			Key .x1 = rowx__1, _
			Key .x2 = rowX__2, _
			Key .y1 = m, _
			Key .y2 = m _
		}))

		' vertical lines
		Dim vlines = Me.cols.map(Function(c) New With { _
			Key .x1 = c.pos, _
			Key .x2 = c.pos, _
			Key .y1 = coly__3, _
			Key .y2 = colY__4 _
		}).concat(colMids.map(Function(m) New With { _
			Key .x1 = m, _
			Key .x2 = m, _
			Key .y1 = coly__3, _
			Key .y2 = colY__4 _
		}))

		' the full set of lines
		Dim lines = hlines.concat(vlines)

		' we record the vertices associated with each line
		lines.forEach(Function(l) InlineAssignHelper(l.verts, New Object() {}))

		' the routing graph
		Me.verts = New Vert() {}
		Me.edges = New Object() {}

		' create vertices at the crossings of horizontal and vertical grid-lines
		hlines.forEach(Function(h) vlines.forEach(Function(v) 
		Dim p = New Vert(Me.verts.length, v.x1, h.y1)
		h.verts.push(p)
		v.verts.push(p)
		Me.verts.push(p)

		' assign vertices to the nodes immediately under them
		Dim i As Integer = Me.backToFront.length
		While (System.Math.Max(System.Threading.Interlocked.Decrement(i),i + 1)) > 0
			Dim node = Me.backToFront(i)
			Dim r = node.rect
			Dim dx = Math.abs(p.x - r.cx())
			Dim dy = Math.abs(p.y - r.cy())
			If dx < r.width() / 2 AndAlso dy < r.height() / 2 Then
				DirectCast(p, any).node = node
				Exit While
			End If
		End While

End Function))

		lines.forEach(Function(l, li) 
		' create vertices at the intersections of nodes and lines
		Me.nodes.forEach(Function(v, i) 
		v.rect.lineIntersections(l.x1, l.y1, l.x2, l.y2).forEach(Function(intersect, j) 
		'console.log(li+','+i+','+j+':'+intersect.x + ',' + intersect.y);
		Dim p = New Vert(Me.verts.length, intersect.x, intersect.y, v, l)
		Me.verts.push(p)
		l.verts.push(p)
		v.ports.push(p)

End Function)

End Function)

		' split lines into edges joining vertices
		Dim isHoriz = Math.abs(l.y1 - l.y2) < 0.1
		Dim delta = Function(a, b) If(isHoriz, b.x - a.x, b.y - a.y)
		l.verts.sort(delta)
		For i As var = 1 To l.verts.length - 1
			Dim u = l.verts(i - 1), v = l.verts(i)
			If u.node AndAlso u.node = v.node AndAlso u.node.leaf Then
				Continue For
			End If
			Me.edges.push(New With { _
				Key .source = u.id, _
				Key .target = v.id, _
				Key .length = Math.abs(delta(u, v)) _
			})
		Next




End Function)
	End Sub

	' find path from v to root including both v and root
	Private Function findLineage(v As any) As any
		Dim lineage = New Object() {v}
		Do
			v = v.parent
			lineage.push(v)
		Loop While v IsNot Me.root
		Return lineage.reverse()
	End Function

	' find path connecting a and b through their lowest common ancestor
	Private Function findAncestorPathBetween(a As any, b As any) As any
		Dim aa = Me.findLineage(a)
		Dim ba = Me.findLineage(b)
		Dim i = 0
		While aa(i) = ba(i)
			i += 1
		End While
		' i-1 to include common ancestor only once (as first element)
		Return New With { _
			Key .commonAncestor = aa(i - 1), _
			Key .lineages = aa.slice(i).concat(ba.slice(i)) _
		}
	End Function

	' when finding a path between two nodes a and b, siblings of a and b on the
	' paths from a and b to their least common ancestor are obstacles
	Public Function siblingObstacles(a As any, b As any) As any
		Dim path = Me.findAncestorPathBetween(a, b)
		Dim lineageLookup = New Object() {}
		path.lineages.forEach(Function(v) InlineAssignHelper(lineageLookup(v.id), New Object() {}))
		Dim obstacles = path.commonAncestor.children.filter(Function(v) Not (lineageLookup.Have(v)))

		path.lineages.filter(Function(v) v.parent <> path.commonAncestor).forEach(Function(v) InlineAssignHelper(obstacles, obstacles.concat(v.parent.children.filter(Function(c) c <> v.id))))

		Return obstacles.map(Function(v) Me.nodes(v))
	End Function

	' for the given routes, extract all the segments orthogonal to the axis x
	' and return all them grouped by x position
	Private Shared Function getSegmentSets(routes As any, x As Integer, y As Integer) As any
		' vsegments is a list of vertical segments sorted by x position
		Dim vsegments = New Object() {}
		For ei As var = 0 To routes.length - 1
			Dim route = routes(ei)
			For si As var = 0 To route.length - 1
				Dim s = route(si)
				s.edgeid = ei
				s.i = si
				Dim sdx = s(1)(x) - s(0)(x)
				If Math.abs(sdx) < 0.1 Then
					vsegments.push(s)
				End If
			Next
		Next
		vsegments.sort(Function(a, b) a(0)(x) - b(0)(x))

		' vsegmentsets is a set of sets of segments grouped by x position
		Dim vsegmentsets = New Object() {}
		Dim segmentset = Nothing
		For i As var = 0 To vsegments.length - 1
			Dim s = vsegments(i)
			If Not segmentset OrElse Math.abs(s(0)(x) - segmentset.pos) > 0.1 Then
				segmentset = New With { _
					Key .pos = s(0)(x), _
					Key .segments = New Object() {} _
				}
				vsegmentsets.push(segmentset)
			End If
			segmentset.segments.push(s)
		Next
		Return vsegmentsets
	End Function

	' for all segments in this bundle create a vpsc problem such that
	' each segment's x position is a variable and separation constraints
	' are given by the partial order over the edges to which the segments belong
	' for each pair s1,s2 of segments in the open set:
	'   e1 = edge of s1, e2 = edge of s2
	'   if leftOf(e1,e2) create constraint s1.x + gap <= s2.x
	'   else if leftOf(e2,e1) create cons. s2.x + gap <= s1.x
	Private Shared Sub nudgeSegs(x As String, y As String, routes As any, segments As any, leftOf As any, gap As number)
		Dim n = segments.length
		If n <= 1 Then
			Return
		End If
		Dim vs = segments.map(Function(s) New Variable(s(0)(x)))
		Dim cs = New Object() {}
		For i As var = 0 To n - 1
			For j As var = 0 To n - 1
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
				If x = "x"C Then
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
					cs.push(New Constraint(vs(lind), vs(rind), gap))
				End If
			Next
		Next
		Dim solver = New Solver(vs, cs)
		solver.solve()
		vs.forEach(Function(v, i) 
		Dim s = segments(i)
		Dim pos = v.position()
		s(0)(x) = InlineAssignHelper(s(1)(x), pos)
		Dim route = routes(s.edgeid)
		If s.i > 0 Then
			route(s.i - 1)(1)(x) = pos
		End If
		If s.i < route.length - 1 Then
			route(s.i + 1)(0)(x) = pos
		End If

End Function)
	End Sub

	Private Shared Sub nudgeSegments(routes As any, x As String, y As String, leftOf As Func(Of number, number, Boolean), gap As number)
		Dim vsegmentsets = GridRouter.getSegmentSets(routes, x, y)
		' scan the grouped (by x) segment sets to find co-linear bundles
		For i As var = 0 To vsegmentsets.length - 1
			Dim ss = vsegmentsets(i)
			Dim events = New Object() {}
			For j As var = 0 To ss.segments.length - 1
				Dim s = ss.segments(j)
				events.push(New With { _
					Key .type = 0, _
					Key .s = s, _
					Key .pos = Math.min(s(0)(y), s(1)(y)) _
				})
				events.push(New With { _
					Key .type = 1, _
					Key .s = s, _
					Key .pos = Math.max(s(0)(y), s(1)(y)) _
				})
			Next
			events.sort(Function(a, b) a.pos - b.pos + a.type - b.type)
			Dim open = New Object() {}
			Dim openCount = 0
			events.forEach(Function(e) 
			If e.type = 0 Then
				open.push(e.s)
				openCount += 1
			Else
				openCount -= 1
			End If
			If openCount = 0 Then
				GridRouter.nudgeSegs(x, y, routes, open, leftOf, gap)
				open = New any() {}
			End If

End Function)
		Next
	End Sub

	' obtain routes for the specified edges, nicely nudged apart
	' warning: edge paths may be reversed such that common paths are ordered consistently within bundles!
	' @param edges list of edges
	' @param nudgeGap how much to space parallel edge segements
	' @param source function to retrieve the index of the source node for a given edge
	' @param target function to retrieve the index of the target node for a given edge
	' @returns an array giving, for each edge, an array of segments, each segment a pair of points in an array
	Private Function routeEdges(Of Edge)(edges As Edge(), nudgeGap As number, source As Func(Of Edge, number), target As Func(Of Edge, number)) As Point()()()
		Dim routePaths = edges.map(Function(e) Me.route(source(e), target(e)))
		Dim order = GridRouter.orderEdges(routePaths)
		Dim routes = routePaths.map(Function(e) GridRouter.makeSegments(e))
		GridRouter.nudgeSegments(routes, "x"C, "y"C, order, nudgeGap)
		GridRouter.nudgeSegments(routes, "y"C, "x"C, order, nudgeGap)
		GridRouter.unreverseEdges(routes, routePaths)
		Return routes
	End Function

	' path may have been reversed by the subsequence processing in orderEdges
	' so now we need to restore the original order
	Private Shared Sub unreverseEdges(routes As any, routePaths As any)
		routes.forEach(Function(segments, i) 
		Dim path = routePaths(i)
		If DirectCast(path, any).reversed Then
			segments.reverse()
			' reverse order of segments
				' reverse each segment
			segments.forEach(Function(segment) segment.reverse())
		End If

End Function)
	End Sub

	Private Shared Function angleBetween2Lines(line1 As Point(), line2 As Point()) As number
		Dim angle1 = Math.atan2(line1(0).y - line1(1).y, line1(0).x - line1(1).x)
		Dim angle2 = Math.atan2(line2(0).y - line2(1).y, line2(0).x - line2(1).x)
		Dim diff = angle1 - angle2
		If diff > Math.PI OrElse diff < -Math.PI Then
			diff = angle2 - angle1
		End If
		Return diff
	End Function

	' does the path a-b-c describe a left turn?
	Private Shared Function isLeft(a As Point, b As Point, c As Point) As Boolean
		Return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) <= 0
	End Function

	Public Class Pair
		Public l As number
		Public r As number
	End Class

	' for the given list of ordered pairs, returns a function that (efficiently) looks-up a specific pair to
	' see if it exists in the list
	Private Shared Function getOrder(pairs As Pair()) As Func(Of number, number, Boolean)
		Dim outgoing = New Object() {}
		For i As var = 0 To pairs.length - 1
			Dim p = pairs(i)
			If outgoing(p.l) Is Nothing Then
				outgoing(p.l) = New any() {}
			End If
			outgoing(p.l)(p.r) = True
		Next
		Return Function(l, r) outgoing(l) IsNot Nothing AndAlso outgoing(l)(r)
	End Function

	' returns an ordering (a lookup function) that determines the correct order to nudge the
	' edge paths apart to minimize crossings
	Private Shared Function orderEdges(edges As any) As any
		Dim edgeOrder = New Object() {}
		For i As var = 0 To edges.length - 2
			For j As var = i + 1 To edges.length - 1
				Dim e = edges(i)
				Dim f = edges(j)
				Dim lcs = New LongestCommonSubsequence(e, f)
				Dim u As any, vi As any, vj As any
				If lcs.length = 0 Then
					Continue For
				End If
				' no common subpath
				If lcs.reversed Then
					' if we found a common subpath but one of the edges runs the wrong way,
					' then reverse f.
					f.reverse()
					f.reversed = True
					lcs = New LongestCommonSubsequence(e, f)
				End If
				If (lcs.si <= 0 OrElse lcs.ti <= 0) AndAlso (lcs.si + lcs.length >= e.length OrElse lcs.ti + lcs.length >= f.length) Then
					' the paths do not diverge, so make an arbitrary ordering decision
					edgeOrder.push(New With { _
						Key .l = i, _
						Key .r = j _
					})
					Continue For
				End If
				If lcs.si + lcs.length >= e.length OrElse lcs.ti + lcs.length >= f.length Then
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
				If GridRouter.isLeft(u, vi, vj) Then
					edgeOrder.push(New With { _
						Key .l = j, _
						Key .r = i _
					})
				Else
					edgeOrder.push(New With { _
						Key .l = i, _
						Key .r = j _
					})
				End If
			Next
		Next
		'edgeOrder.forEach(function (e) { console.log('l:' + e.l + ',r:' + e.r) });
		Return GridRouter.getOrder(edgeOrder)
	End Function

	' for an orthogonal path described by a sequence of points, create a list of segments
	' if consecutive segments would make a straight line they are merged into a single segment
	' segments are over cloned points, not the original vertices
	Private Shared Function makeSegments(path As Point()) As Point()()
		Dim copyPoint = Function(p As Point) 
		Return New Point() With { _
			Key .x = p.x, _
			Key .y = p.y _
		}

End Function
		Dim isStraight = Function(a, b, c) Math.abs((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) < 0.001
		Dim segments = New Object() {}
		Dim a = copyPoint(path(0))
		For i As var = 1 To path.length - 1
			Dim b = copyPoint(path(i)), c = If(i < path.length - 1, path(i + 1), Nothing)
			If Not c OrElse Not isStraight(a, b, c) Then
				segments.push(New any() {a, b})
				a = b
			End If
		Next
		Return segments
	End Function

	' find a route between node s and node t
	' returns an array of indices to verts
	Public Function route(s As number, t As number) As Point()
		Dim source = Me.nodes(CType(s, number))
		Dim target = Me.nodes(CType(t, number))
		Me.obstacles = Me.siblingObstacles(source, target)

		Dim obstacleLookup = New Object() {}
		Me.obstacles.forEach(Function(o) InlineAssignHelper(obstacleLookup(o.id), o))
		Me.passableEdges = Me.edges.filter(Function(e) 
		Dim u = Me.verts(e.source)
		Dim v = Me.verts(e.target)
		Return Not (u.node AndAlso obstacleLookup.Have(u.node.id) OrElse v.node AndAlso obstacleLookup.Have(v.node.id))

End Function)

		' add dummy segments linking ports inside source and target
		For i As Integer = 1 To source.ports.length - 1
			Dim u = source.ports(0).id
			Dim v = source.ports(i).id
			Me.passableEdges.push(New With { _
				Key .source = u, _
				Key .target = v, _
				Key .length = 0 _
			})
		Next
		For i As var = 1 To target.ports.length - 1
			Dim u = target.ports(0).id
			Dim v = target.ports(i).id
			Me.passableEdges.push(New With { _
				Key .source = u, _
				Key .target = v, _
				Key .length = 0 _
			})
		Next

		Dim getSource = Function(e) e.source
		Dim getTarget = Function(e) e.target
		Dim getLength = Function(e) e.length

		Dim shortestPathCalculator = New Calculator(Me.verts.length, Me.passableEdges, getSource, getTarget, getLength)
		Dim bendPenalty = Function(u, v, w) 
		Dim a = Me.verts(u)
		Dim b = Me.verts(v)
		Dim c = Me.verts(w)
		Dim dx = Math.abs(c.x - a.x)
		Dim dy = Math.abs(c.y - a.y)
		' don't count bends from internal node edges
		If a.node Is source AndAlso a.node Is b.node OrElse b.node Is target AndAlso b.node Is c.node Then
			Return 0
		End If
		Return If(dx > 1 AndAlso dy > 1, 1000, 0)

End Function

		' get shortest path
		Dim shortestPath = shortestPathCalculator.PathFromNodeToNodeWithPrevCost(source.ports(0).id, target.ports(0).id, bendPenalty)

		' shortest path is reversed and does not include the target port
		Dim pathPoints = shortestPath.reverse().map(Function(vi) Me.verts(vi))
		pathPoints.push(Me.nodes(target.id).ports(0))

		' filter out any extra end points that are inside the source or target (i.e. the dummy segments above)
		Return pathPoints.filter(Function(v, i) Not (i < pathPoints.length - 1 AndAlso pathPoints(i + 1).node = source AndAlso v.node = source OrElse i > 0 AndAlso v.node = target AndAlso pathPoints(i - 1).node = target))
	End Function

	Public Shared Function getRoutePath(route As Point()(), cornerradius As number, arrowwidth As number, arrowheight As number) As any
		Dim result = New With { _
			Key .routepath = "M " & route(0)(0).x & " "C & route(0)(0).y & " "C, _
			Key .arrowpath = "" _
		}
		If route.length > 1 Then
			For i As var = 0 To route.length - 1
				Dim li = route(i)
				Dim x = li(1).x
				Dim y = li(1).y
				Dim dx = x - li(0).x
				Dim dy = y - li(0).y
				If i < route.length - 1 Then
					If Math.abs(dx) > 0 Then
						x -= dx / Math.abs(dx) * cornerradius
					Else
						y -= dy / Math.abs(dy) * cornerradius
					End If
					result.routepath += "L " & x & " "C & y & " "C
					Dim l = route(i + 1)
					Dim x0 = l(0).x
					Dim y0 = l(0).y
					Dim x1 = l(1).x
					Dim y1 = l(1).y
					dx = x1 - x0
					dy = y1 - y0
					Dim angle = If(GridRouter.angleBetween2Lines(li, l) < 0, 1, 0)
					'console.log(cola.GridRouter.angleBetween2Lines(li, l))
					Dim x2
					Dim y2
					If Math.abs(dx) > 0 Then
						x2 = x0 + dx / Math.abs(dx) * cornerradius
						y2 = y0
					Else
						x2 = x0
						y2 = y0 + dy / Math.abs(dy) * cornerradius
					End If
					Dim cx = Math.abs(x2 - x)
					Dim cy = Math.abs(y2 - y)
					result.routepath += "A " & Convert.ToString(cx) & " " & Convert.ToString(cy) & " 0 0 " & angle & " " & Convert.ToString(x2) & " " & Convert.ToString(y2) & " "
				Else
					Dim arrowtip = New any() {x, y}
					Dim arrowcorner1
					Dim arrowcorner2
					If Math.abs(dx) > 0 Then
						x -= dx / Math.abs(dx) * arrowheight
						arrowcorner1 = New number() {x, y + arrowwidth}
						arrowcorner2 = New number() {x, y - arrowwidth}
					Else
						y -= dy / Math.abs(dy) * arrowheight
						arrowcorner1 = New number() {x + arrowwidth, y}
						arrowcorner2 = New number() {x - arrowwidth, y}
					End If
					result.routepath += "L " & x & " "C & y & " "C
					If arrowheight > 0 Then
						result.arrowpath = (((("M " & Convert.ToString(arrowtip(0)) & " "C & Convert.ToString(arrowtip(1)) & " L ") + arrowcorner1(0) & " "C) + arrowcorner1(1) & " L ") + arrowcorner2(0) & " "C) + arrowcorner2(1)
					End If
				End If
			Next
		Else
			Dim li = route(0)
			Dim x = li(1).x
			Dim y = li(1).y
			Dim dx = x - li(0).x
			Dim dy = y - li(0).y
			Dim arrowtip = New number() {x, y}
			Dim arrowcorner1
			Dim arrowcorner2
			If Math.abs(dx) > 0 Then
				x -= dx / Math.abs(dx) * arrowheight
				arrowcorner1 = New number() {x, y + arrowwidth}
				arrowcorner2 = New number() {x, y - arrowwidth}
			Else
				y -= dy / Math.abs(dy) * arrowheight
				arrowcorner1 = New number() {x + arrowwidth, y}
				arrowcorner2 = New number() {x - arrowwidth, y}
			End If
			result.routepath += "L " & x & " "C & y & " "C
			If arrowheight > 0 Then
				result.arrowpath = (((("M " & arrowtip(0) & " "C & arrowtip(1) & " L ") + arrowcorner1(0) & " "C) + arrowcorner1(1) & " L ") + arrowcorner2(0) & " "C) + arrowcorner2(1)
			End If
		End If
		Return result
	End Function
	Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
		target = value
		Return value
	End Function
End Class
