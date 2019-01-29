Imports number = System.Double
Imports any = System.Object

Class Point
	Public x As number
	Public y As number
End Class

Class LineSegment

	Public x1 As number
	Public y1 As number
	Public x2 As number
	Public y2 As number

	Public Sub New(x1 As number, y1 As number, x2 As number, y2 As number)
		Me.x1 = x1
		Me.y1 = y1
		Me.x2 = x2
		Me.y2 = y2
	End Sub
End Class

Class PolyPoint
	Inherits Point
	Public Property polyIndex() As Integer
		Get
			Return m_polyIndex
		End Get
		Set
			m_polyIndex = Value
		End Set
	End Property
	Private m_polyIndex As Integer
End Class

Class tangentPoly
	Public Property rtan() As Integer
		Get
			Return m_rtan
		End Get
		Set
			m_rtan = Value
		End Set
	End Property
	Private m_rtan As Integer
	Public Property ltan() As Integer
		Get
			Return m_ltan
		End Get
		Set
			m_ltan = Value
		End Set
	End Property
	Private m_ltan As Integer
End Class

Class Extensions

	'* tests if a point is Left|On|Right of an infinite line.
'     * @param points P0, P1, and P2
'     * @return >0 for P2 left of the line through P0 and P1
'     *            =0 for P2 on the line
'     *            &lt;0 for P2 right of the line
'     

	Private Shared Function isLeft(P0 As Point, P1 As Point, P2 As Point) As number
		Return (P1.x - P0.x) * (P2.y - P0.y) - (P2.x - P0.x) * (P1.y - P0.y)
	End Function

	Private Shared Function above(p As Point, vi As Point, vj As Point) As Boolean
		Return isLeft(p, vi, vj) > 0
	End Function

	Private Shared Function below(p As Point, vi As Point, vj As Point) As Boolean
		Return isLeft(p, vi, vj) < 0
	End Function


	' apply f to the points in P in clockwise order around the point p
	Private Shared Sub clockwiseRadialSweep(p__1 As Point, P__2 As Point(), f As Action(Of Point))
		P__2.slice(0).sort(Function(a, b) Math.atan2(a.y - p__1.y, a.x - p__1.x) - Math.atan2(b.y - p__1.y, b.x - p__1.x)).forEach(f)
	End Sub

	Private Shared Function nextPolyPoint(p As PolyPoint, ps As PolyPoint()) As PolyPoint
		If p.polyIndex Is ps.length - 1 Then
			Return ps(0)
		End If
		Return ps(p.polyIndex + 1)
	End Function

	Private Shared Function prevPolyPoint(p As PolyPoint, ps As PolyPoint()) As PolyPoint
		If p.polyIndex = 0 Then
			Return ps(ps.length - 1)
		End If
		Return ps(p.polyIndex - 1)
	End Function

	' tangent_PointPolyC(): fast binary search for tangents to a convex polygon
	'    Input:  P = a 2D point (exterior to the polygon)
	'            n = number of polygon vertices
	'            V = array of vertices for a 2D convex polygon
	'    Output: rtan = index of rightmost tangent point V[rtan]
	'            ltan = index of leftmost tangent point V[ltan]
	Private Shared Function tangent_PointPolyC(P As Point, V As Point()) As tangentPoly
		' Rtangent_PointPolyC and Ltangent_PointPolyC require polygon to be
		' "closed" with the first vertex duplicated at end, so V[n-1] = V[0].
		Dim Vclosed = V.slice(0)
		' Copy V
		Vclosed.push(V(0))
		' Add V[0] at end
		Return New tangentPoly() With { _
			Key .rtan = Rtangent_PointPolyC(P, Vclosed), _
			Key .ltan = Ltangent_PointPolyC(P, Vclosed) _
		}
	End Function


	' Rtangent_PointPolyC(): binary search for convex polygon right tangent
	'    Input:  P = a 2D point (exterior to the polygon)
	'            n = number of polygon vertices
	'            V = array of vertices for a 2D convex polygon with first
	'                vertex duplicated as last, so V[n-1] = V[0]
	'    Return: index "i" of rightmost tangent point V[i]
	Private Shared Function Rtangent_PointPolyC(P As Point, V As Point()) As Integer
		Dim n = V.length - 1

		' use binary search for large convex polygons
		Dim a As Integer
		Dim b As Integer
		Dim c As Integer
		' indices for edge chain endpoints
		Dim upA As Boolean, dnC As Boolean
		' test for up direction of edges a and c
		' rightmost tangent = maximum for the isLeft() ordering
		' test if V[0] is a local maximum
		If below(P, V(1), V(0)) AndAlso Not above(P, V(n - 1), V(0)) Then
			Return 0
		End If
		' V[0] is the maximum tangent point
		a = 0
		b = n
		While True
			' start chain = [0,n] with V[n]=V[0]
			If b - a = 1 Then
				If above(P, V(a), V(b)) Then
					Return a
				Else
					Return b
				End If
			End If

			c = Math.floor((a + b) \ 2)
			' midpoint of [a,b], and 0<c<n
			dnC = below(P, V(c + 1), V(c))
			If dnC AndAlso Not above(P, V(c - 1), V(c)) Then
				Return c
			End If
			' V[c] is the maximum tangent point
			' no max yet, so continue with the binary search
			' pick one of the two subchains [a,c] or [c,b]
			upA = above(P, V(a + 1), V(a))
			If upA Then
				' edge a points up
				If dnC Then
					' edge c points down
					b = c
				Else
					' select [a,c]
					' edge c points up
					If above(P, V(a), V(c)) Then
						' V[a] above V[c]
						b = c
					Else
						' select [a,c]
						' V[a] below V[c]
						a = c
						' select [c,b]
					End If
				End If
			Else
				' edge a points down
				If Not dnC Then
					' edge c points up
					a = c
				Else
					' select [c,b]
					' edge c points down
					If below(P, V(a), V(c)) Then
						' V[a] below V[c]
						b = c
					Else
						' select [a,c]
						' V[a] above V[c]
						a = c
						' select [c,b]
					End If
				End If
			End If
		End While
	End Function

	' Ltangent_PointPolyC(): binary search for convex polygon left tangent
	'    Input:  P = a 2D point (exterior to the polygon)
	'            n = number of polygon vertices
	'            V = array of vertices for a 2D convex polygon with first
	'                vertex duplicated as last, so V[n-1] = V[0]
	'    Return: index "i" of leftmost tangent point V[i]
	Private Shared Function Ltangent_PointPolyC(P As Point, V As Point()) As Integer
		Dim n As Integer = V.length - 1
		' use binary search for large convex polygons
		Dim a As Integer
		Dim b As Integer
		Dim c As Integer
		' indices for edge chain endpoints
		Dim dnA As Boolean, dnC As Boolean
		' test for down direction of edges a and c
		' leftmost tangent = minimum for the isLeft() ordering
		' test if V[0] is a local minimum
		If above(P, V(n - 1), V(0)) AndAlso Not below(P, V(1), V(0)) Then
			Return 0
		End If
		' V[0] is the minimum tangent point
		a = 0
		b = n
		While True
			' start chain = [0,n] with V[n] = V[0]
			If b - a = 1 Then
				If below(P, V(a), V(b)) Then
					Return a
				Else
					Return b
				End If
			End If

			c = Math.floor((a + b) \ 2)
			' midpoint of [a,b], and 0<c<n
			dnC = below(P, V(c + 1), V(c))
			If above(P, V(c - 1), V(c)) AndAlso Not dnC Then
				Return c
			End If
			' V[c] is the minimum tangent point
			' no min yet, so continue with the binary search
			' pick one of the two subchains [a,c] or [c,b]
			dnA = below(P, V(a + 1), V(a))
			If dnA Then
				' edge a points down
				If Not dnC Then
					' edge c points up
					b = c
				Else
					' select [a,c]
					' edge c points down
					If below(P, V(a), V(c)) Then
						' V[a] below V[c]
						b = c
					Else
						' select [a,c]
						' V[a] above V[c]
						a = c
						' select [c,b]
					End If
				End If
			Else
				' edge a points up
				If dnC Then
					' edge c points down
					a = c
				Else
					' select [c,b]
					' edge c points up
					If above(P, V(a), V(c)) Then
						' V[a] above V[c]
						b = c
					Else
						' select [a,c]
						' V[a] below V[c]
						a = c
						' select [c,b]
					End If
				End If
			End If
		End While
	End Function

	Private Class BiTangent
		Public t1 As Integer, t2 As Integer


		Public Sub New()
		End Sub

		Public Sub New(t1 As Integer, t2 As Integer)
			Me.t1 = t1
			Me.t2 = t2
		End Sub
	End Class

	' RLtangent_PolyPolyC(): get the RL tangent between two convex polygons
	'    Input:  m = number of vertices in polygon 1
	'            V = array of vertices for convex polygon 1 with V[m]=V[0]
	'            n = number of vertices in polygon 2
	'            W = array of vertices for convex polygon 2 with W[n]=W[0]
	'    Output: *t1 = index of tangent point V[t1] for polygon 1
	'            *t2 = index of tangent point W[t2] for polygon 2
	Private Shared Function tangent_PolyPolyC(V As Point(), W As Point(), t1 As Func(Of Point, Point(), Integer), t2 As Func(Of Point, Point(), Integer), cmp1 As Func(Of Point, Point, Point, Boolean), cmp2 As Func(Of Point, Point, Point, Boolean)) As BiTangent
		Dim ix1 As Integer, ix2 As Integer
		' search indices for polygons 1 and 2
		' first get the initial vertex on each polygon
		ix1 = t1(W(0), V)
		' right tangent from W[0] to V
		ix2 = t2(V(ix1), W)
		' left tangent from V[ix1] to W
		' ping-pong linear search until it stabilizes
		Dim done = False
		' flag when done
		While Not done
			done = True
			' assume done until...
			While True
				If ix1 Is V.length - 1 Then
					ix1 = 0
				End If
				If cmp1(W(ix2), V(ix1), V(ix1 + 1)) Then
					Exit While
				End If
					' get Rtangent from W[ix2] to V
				ix1 += 1
			End While
			While True
				If ix2 = 0 Then
					ix2 = W.length - 1
				End If
				If cmp2(V(ix1), W(ix2), W(ix2 - 1)) Then
					Exit While
				End If
				ix2 -= 1
				' get Ltangent from V[ix1] to W
					' not done if had to adjust this
				done = False
			End While
		End While
		Return New BiTangent() With { _
			Key .t1 = ix1, _
			Key .t2 = ix2 _
		}
	End Function

	Private Shared Function LRtangent_PolyPolyC(V As Point(), W As Point()) As BiTangent
		Dim rl = RLtangent_PolyPolyC(W, V)
		Return New BiTangent() With { _
			Key .t1 = rl.t2, _
			Key .t2 = rl.t1 _
		}
	End Function

	Private Shared Function RLtangent_PolyPolyC(V As Point(), W As Point()) As BiTangent
		Return tangent_PolyPolyC(V, W, AddressOf Rtangent_PointPolyC, AddressOf Ltangent_PointPolyC, AddressOf above, AddressOf below)
	End Function

	Private Shared Function LLtangent_PolyPolyC(V As Point(), W As Point()) As BiTangent
		Return tangent_PolyPolyC(V, W, AddressOf Ltangent_PointPolyC, AddressOf Ltangent_PointPolyC, AddressOf below, AddressOf below)
	End Function

	Private Shared Function RRtangent_PolyPolyC(V As Point(), W As Point()) As BiTangent
		Return tangent_PolyPolyC(V, W, AddressOf Rtangent_PointPolyC, AddressOf Rtangent_PointPolyC, AddressOf above, AddressOf above)
	End Function


	Private Class BiTangents
		Public rl As BiTangent
		Public lr As BiTangent
		Public ll As BiTangent
		Public rr As BiTangent
	End Class

	Private Class TVGPoint
		Inherits Point
		Public vv As VisibilityVertex
	End Class

	Private Class VisibilityVertex

		Public id As number
		Public polyid As number
		Public polyvertid As number
		Public p As TVGPoint

		Public Sub New(id As number, polyid As number, polyvertid As number, p As TVGPoint)
			p.vv = Me
			Me.id = id
			Me.polyid = polyid
			Me.polyvertid = polyvertid
			Me.p = p
		End Sub
	End Class

	Private Class VisibilityEdge
		Public source As VisibilityVertex
		Public target As VisibilityVertex

		Private Sub New(source As VisibilityVertex, target As VisibilityVertex)
			Me.source = source

			Me.target = target
		End Sub
		Public ReadOnly Property length() As number
			Get
				Dim dx = Me.source.p.x - Me.target.p.x
				Dim dy = Me.source.p.y - Me.target.p.y
				Return Math.sqrt(dx * dx + dy * dy)
			End Get
		End Property
	End Class

	Private Class TangentVisibilityGraph
		Public V As VisibilityVertex() = {}
		Public E As VisibilityEdge() = {}
		Public P As TVGPoint()()

		Public Sub New(P__1 As TVGPoint()(), Optional g0 As any = Nothing)
			If g0 Is Nothing Then
				Dim n = P__1.length
				' For each node...
				For i As var = 0 To n - 1
					Dim p__2 = P__1(i)
					' For each node vertex.
					For j As Integer = 0 To p__2.length - 1
						Dim pj__3 = p__2(j)
						Dim vv = New VisibilityVertex(Me.V.length, i, j, pj__3)
						Me.V.push(vv)
						' For the every iteration but the first, generate an
						' edge from the previous visibility vertex to the
						' current one.
						If j > 0 Then
							Me.E.push(New VisibilityEdge(p__2(j - 1).vv, vv))
						End If
					Next
					' Add a visibility edge from the first vertex to the last.
					If p__2.length > 1 Then
						Me.E.push(New VisibilityEdge(p__2(0).vv, p__2(p__2.length - 1).vv))
					End If
				Next
				For i As var = 0 To n - 2
					Dim Pi = P__1(i)
					For j As var = i + 1 To n - 1
						Dim Pj__4 = P__1(j)
						Dim t = tangents(Pi, Pj__4)
						For Each q As var In t.keys
							Dim c = t(q)
							Dim source = Pi(c.t1)
							Dim target = Pj__4(c.t2)
							Me.addEdgeIfVisible(source, target, i, j)
						Next
					Next
				Next
			Else
				Me.V = g0.V.slice(0)
				Me.E = g0.E.slice(0)
			End If
		End Sub
		Private Sub addEdgeIfVisible(u As TVGPoint, v As TVGPoint, i1 As number, i2 As number)
			If Not Me.intersectsPolys(New LineSegment(u.x, u.y, v.x, v.y), i1, i2) Then
				Me.E.push(New VisibilityEdge(u.vv, v.vv))
			End If
		End Sub
		Private Function addPoint(p As TVGPoint, i1 As number) As VisibilityVertex
			Dim n = Me.P.length
			Me.V.push(New VisibilityVertex(Me.V.length, n, 0, p))
			For i As var = 0 To n - 1
				If i = i1 Then
					Continue For
				End If
				Dim poly = Me.P(i)
				Dim t = tangent_PointPolyC(p, poly)
				Me.addEdgeIfVisible(p, poly(t.ltan), i1, i)
				Me.addEdgeIfVisible(p, poly(t.rtan), i1, i)
			Next
			Return p.vv
		End Function
		Private Function intersectsPolys(l As LineSegment, i1 As number, i2 As number) As Boolean
			Dim i As Integer = 0, n As Integer = Me.P.length
			While i < n
				If i <> i1 AndAlso i <> i2 AndAlso intersects(l, Me.P(i)).length > 0 Then
					Return True
				End If
				i += 1
			End While
			Return False
		End Function
	End Class

	Private Shared Function intersects(l As LineSegment, P As Point()) As Integer()
		Dim ints As Integer() = {}
		Dim i As Integer = 1, n As Integer = P.length
		While i < n
			Dim int32 = Rectangle.lineIntersection(l.x1, l.y1, l.x2, l.y2, P(i - 1).x, P(i - 1).y, _
				P(i).x, P(i).y)
			If int32 Then
				ints.push(int32)
			End If
			i += 1
		End While
		Return ints
	End Function

	Private Shared Function tangents(V As Point(), W As Point()) As BiTangents
		Dim m = V.length - 1
		Dim n = W.length - 1
		Dim bt = New BiTangents()
		For i As var = 0 To m - 1
			For j As var = 0 To n - 1
				Dim v1 = V(If(i = 0, m - 1, i - 1))
				Dim v2 = V(i)
				Dim v3 = V(i + 1)
				Dim w1 = W(If(j = 0, n - 1, j - 1))
				Dim w2 = W(j)
				Dim w3 = W(j + 1)
				Dim v1v2w2 = isLeft(v1, v2, w2)
				Dim v2w1w2 = isLeft(v2, w1, w2)
				Dim v2w2w3 = isLeft(v2, w2, w3)
				Dim w1w2v2 = isLeft(w1, w2, v2)
				Dim w2v1v2 = isLeft(w2, v1, v2)
				Dim w2v2v3 = isLeft(w2, v2, v3)
				If v1v2w2 >= 0 AndAlso v2w1w2 >= 0 AndAlso v2w2w3 < 0 AndAlso w1w2v2 >= 0 AndAlso w2v1v2 >= 0 AndAlso w2v2v3 < 0 Then
					bt.ll = New BiTangent(i, j)
				ElseIf v1v2w2 <= 0 AndAlso v2w1w2 <= 0 AndAlso v2w2w3 > 0 AndAlso w1w2v2 <= 0 AndAlso w2v1v2 <= 0 AndAlso w2v2v3 > 0 Then
					bt.rr = New BiTangent(i, j)
				ElseIf v1v2w2 <= 0 AndAlso v2w1w2 > 0 AndAlso v2w2w3 <= 0 AndAlso w1w2v2 >= 0 AndAlso w2v1v2 < 0 AndAlso w2v2v3 >= 0 Then
					bt.rl = New BiTangent(i, j)
				ElseIf v1v2w2 >= 0 AndAlso v2w1w2 < 0 AndAlso v2w2w3 >= 0 AndAlso w1w2v2 <= 0 AndAlso w2v1v2 > 0 AndAlso w2v2v3 <= 0 Then
					bt.lr = New BiTangent(i, j)
				End If
			Next
		Next
		Return bt
	End Function

	Private Function isPointInsidePoly(p As Point, poly As Point()) As [Boolean]
		Dim i As Integer = 1, n As Integer = poly.length
		While i < n
			If below(poly(i - 1), poly(i), p) Then
				Return False
			End If
			i += 1
		End While
		Return True
	End Function

	Private Function isAnyPInQ(p As Point(), q As Point()) As Boolean
		Return Not p.every(Function(v) Not isPointInsidePoly(v, q))
	End Function

	Private Function polysOverlap(p As Point(), q As Point()) As Boolean
		If isAnyPInQ(p, q) Then
			Return True
		End If
		If isAnyPInQ(q, p) Then
			Return True
		End If
		Dim i As Integer = 1, n As Integer = p.length
		While i < n
			Dim v = p(i)
			Dim u = p(i - 1)
			If intersects(New LineSegment(u.x, u.y, v.x, v.y), q).length > 0 Then
				Return True
			End If
			i += 1
		End While
		Return False
	End Function
End Class
