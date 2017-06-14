Imports System.Drawing
Imports Vertex = Microsoft.VisualBasic.Imaging.Drawing3D.Point3D

Namespace Drawing2D.Math2D.ConcaveHull

    ''' <summary>
    ''' http://www.tuicool.com/articles/iUvMjm
    ''' </summary>
    Public Class DelaunayTriangulation

        Dim Vertex As Vertex(), Triangle As dTriangle()
        Dim maxTriangles%

        Sub New(vertex As IEnumerable(Of Vertex), Optional MaxTriangles% = 1000)
            Me.Vertex = vertex.ToArray
            Me.Triangle = New dTriangle(MaxTriangles - 1) {}
            Me.maxTriangles = MaxTriangles
        End Sub

        Private Function InCircle(xp As Long, yp As Long, x1 As Long, y1 As Long, x2 As Long, y2 As Long,
            x3 As Long, y3 As Long, xc As Double, yc As Double, r As Double) As Boolean
            Dim eps As Double
            Dim m1 As Double
            Dim m2 As Double
            Dim mx1 As Double
            Dim mx2 As Double
            Dim my1 As Double
            Dim my2 As Double
            Dim dx As Double
            Dim dy As Double
            Dim rsqr As Double
            Dim drsqr As Double
            eps = 0.000000001
            If Math.Abs(y1 - y2) < eps AndAlso Math.Abs(y2 - y3) < eps Then
                Return False
            End If
            If Math.Abs(y2 - y1) < eps Then
                m2 = (-(Convert.ToDouble(x3) - Convert.ToDouble(x2)) / (Convert.ToDouble(y3) - Convert.ToDouble(y2)))
                mx2 = Convert.ToDouble((x2 + x3) / 2.0)
                my2 = Convert.ToDouble((y2 + y3) / 2.0)
                xc = Convert.ToDouble((x2 + x1) / 2.0)
                yc = Convert.ToDouble(m2 * (xc - mx2) + my2)
            ElseIf Math.Abs(y3 - y2) < eps Then
                m1 = (-(Convert.ToDouble(x2) - Convert.ToDouble(x1)) / (Convert.ToDouble(y2) - Convert.ToDouble(y1)))
                mx1 = Convert.ToDouble((x1 + x2) / 2.0)
                my1 = Convert.ToDouble((y1 + y2) / 2.0)
                xc = Convert.ToDouble((x3 + x2) / 2.0)
                yc = Convert.ToDouble(m1 * (xc - mx1) + my1)
            Else
                m1 = (-(Convert.ToDouble(x2) - Convert.ToDouble(x1)) / (Convert.ToDouble(y2) - Convert.ToDouble(y1)))
                m2 = (-(Convert.ToDouble(x3) - Convert.ToDouble(x2)) / (Convert.ToDouble(y3) - Convert.ToDouble(y2)))
                mx1 = Convert.ToDouble((x1 + x2) / 2.0)
                mx2 = Convert.ToDouble((x2 + x3) / 2.0)
                my1 = Convert.ToDouble((y1 + y2) / 2.0)
                my2 = Convert.ToDouble((y2 + y3) / 2.0)
                xc = Convert.ToDouble((m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2))
                yc = Convert.ToDouble(m1 * (xc - mx1) + my1)
            End If
            dx = (Convert.ToDouble(x2) - Convert.ToDouble(xc))
            dy = (Convert.ToDouble(y2) - Convert.ToDouble(yc))
            rsqr = Convert.ToDouble(dx * dx + dy * dy)
            r = Convert.ToDouble(Math.Sqrt(rsqr))
            dx = Convert.ToDouble(xp - xc)
            dy = Convert.ToDouble(yp - yc)
            drsqr = Convert.ToDouble(dx * dx + dy * dy)
            If drsqr <= rsqr Then
                Return True
            End If
            Return False
        End Function
        Private Function WhichSide(xp As Long, yp As Long, x1 As Long, y1 As Long, x2 As Long, y2 As Long) As Integer
            Dim equation As Double
            equation = ((Convert.ToDouble(yp) - Convert.ToDouble(y1)) * (Convert.ToDouble(x2) - Convert.ToDouble(x1))) - ((Convert.ToDouble(y2) - Convert.ToDouble(y1)) * (Convert.ToDouble(xp) - Convert.ToDouble(x1)))
            If equation > 0 Then
                'WhichSide = -1;
                Return -1
            ElseIf equation = 0 Then
                Return 0
            Else
                Return 1
            End If
        End Function

        ''' <summary>
        ''' <paramref name="nvert"/>值必须要小于<see cref="Vertex"/>的数量
        ''' </summary>
        ''' <param name="nvert"></param>
        ''' <returns></returns>
        Public Function Triangulate(nvert As Integer) As Integer
            Dim Complete As Boolean() = New Boolean(maxTriangles - 1) {}
            Dim Edges As Long(,) = New Long(2, maxTriangles * 3) {}
            Dim Nedge As Long
            Dim xmin As Long
            Dim xmax As Long
            Dim ymin As Long
            Dim ymax As Long
            Dim xmid As Long
            Dim ymid As Long
            Dim dx As Double
            Dim dy As Double
            Dim dmax As Double
            Dim i As Integer
            Dim j As Integer
            Dim k As Integer
            Dim ntri As Integer
            Dim xc As Double = 0.0
            Dim yc As Double = 0.0
            Dim r As Double = 0.0
            Dim inc As Boolean
            xmin = Vertex(1).X
            ymin = Vertex(1).Y
            xmax = xmin
            ymax = ymin
            For i = 2 To nvert
                If Vertex(i).X < xmin Then
                    xmin = Vertex(i).X
                End If
                If Vertex(i).X > xmax Then
                    xmax = Vertex(i).X
                End If
                If Vertex(i).Y < ymin Then
                    ymin = Vertex(i).Y
                End If
                If Vertex(i).Y > ymax Then
                    ymax = Vertex(i).Y
                End If
            Next
            dx = Convert.ToDouble(xmax) - Convert.ToDouble(xmin)
            dy = Convert.ToDouble(ymax) - Convert.ToDouble(ymin)
            If dx > dy Then
                dmax = dx
            Else
                dmax = dy
            End If
            xmid = (xmax + xmin) \ 2
            ymid = (ymax + ymin) \ 2
            Vertex(nvert + 1).X = Convert.ToInt64(xmid - 2 * dmax)
            Vertex(nvert + 1).Y = Convert.ToInt64(ymid - dmax)
            Vertex(nvert + 2).X = xmid
            Vertex(nvert + 2).Y = Convert.ToInt64(ymid + 2 * dmax)
            Vertex(nvert + 3).X = Convert.ToInt64(xmid + 2 * dmax)
            Vertex(nvert + 3).Y = Convert.ToInt64(ymid - dmax)
            Triangle(1).vv0 = nvert + 1
            Triangle(1).vv1 = nvert + 2
            Triangle(1).vv2 = nvert + 3
            Complete(1) = False
            ntri = 1
            For i = 1 To nvert
                Nedge = 0
                j = 0
                Do
                    j = j + 1
                    If Complete(j) <> True Then
                        inc = InCircle(Vertex(i).X, Vertex(i).Y, Vertex(Triangle(j).vv0).X, Vertex(Triangle(j).vv0).Y, Vertex(Triangle(j).vv1).X, Vertex(Triangle(j).vv1).Y,
                            Vertex(Triangle(j).vv2).X, Vertex(Triangle(j).vv2).Y, xc, yc, r)
                        If inc Then
                            Edges(1, Nedge + 1) = Triangle(j).vv0
                            Edges(2, Nedge + 1) = Triangle(j).vv1
                            Edges(1, Nedge + 2) = Triangle(j).vv1
                            Edges(2, Nedge + 2) = Triangle(j).vv2
                            Edges(1, Nedge + 3) = Triangle(j).vv2
                            Edges(2, Nedge + 3) = Triangle(j).vv0
                            Nedge = Nedge + 3
                            Triangle(j).vv0 = Triangle(ntri).vv0
                            Triangle(j).vv1 = Triangle(ntri).vv1
                            Triangle(j).vv2 = Triangle(ntri).vv2
                            Complete(j) = Complete(ntri)
                            j = j - 1
                            ntri = ntri - 1
                        End If
                    End If
                Loop While j < ntri
                For j = 1 To Nedge - 1
                    If Edges(1, j) <> 0 AndAlso Edges(2, j) <> 0 Then
                        For k = j + 1 To Nedge
                            If Edges(1, k) <> 0 AndAlso Edges(2, k) <> 0 Then
                                If Edges(1, j) = Edges(2, k) Then
                                    If Edges(2, j) = Edges(1, k) Then
                                        Edges(1, j) = 0
                                        Edges(2, j) = 0
                                        Edges(1, k) = 0
                                        Edges(2, k) = 0
                                    End If
                                End If
                            End If
                        Next
                    End If
                Next
                For j = 1 To Nedge
                    If Edges(1, j) <> 0 AndAlso Edges(2, j) <> 0 Then
                        ntri = ntri + 1
                        Triangle(ntri).vv0 = Edges(1, j)
                        Triangle(ntri).vv1 = Edges(2, j)
                        Triangle(ntri).vv2 = i
                        Complete(ntri) = False
                    End If
                Next
            Next
            i = 0
            Do
                i = i + 1
                If Triangle(i).vv0 > nvert OrElse Triangle(i).vv1 > nvert OrElse Triangle(i).vv2 > nvert Then
                    Triangle(i).vv0 = Triangle(ntri).vv0
                    Triangle(i).vv1 = Triangle(ntri).vv1
                    Triangle(i).vv2 = Triangle(ntri).vv2
                    i = i - 1
                    ntri = ntri - 1
                End If
            Loop While i < ntri
            Return ntri
        End Function
        Public Shared Function Diameter(Ax As Double, Ay As Double, Bx As Double, By As Double, Cx As Double, Cy As Double) As Double
            Dim x As Double, y As Double
            Dim a__1 As Double = Ax
            Dim b__2 As Double = Bx
            Dim c As Double = Cx
            Dim m As Double = Ay
            Dim n As Double = By
            Dim k As Double = Cy
            Dim A__3 As Double = a__1 * b__2 * b__2 + a__1 * n * n + a__1 * a__1 * c - b__2 * b__2 * c + m * m * c - n * n * c - a__1 * c * c - a__1 * k * k - a__1 * a__1 * b__2 + b__2 * c * c - m * m * b__2 + b__2 * k * k
            Dim B__4 As Double = a__1 * n + m * c + k * b__2 - n * c - a__1 * k - b__2 * m
            y = A__3 / B__4 / 2
            Dim AA As Double = b__2 * b__2 * m + m * n * n + a__1 * a__1 * k - b__2 * b__2 * k + m * m * k - n * n * k - c * c * m - m * k * k - a__1 * a__1 * n + c * c * n - m * m * n + k * k * n
            Dim BB As Double = b__2 * m + a__1 * k + c * n - b__2 * k - c * m - a__1 * n
            x = AA / BB / 2
            Return Math.Sqrt((Ax - x) * (Ax - x) + (Ay - y) * (Ay - y))
        End Function
        Public Shared Function MaxEdge(Ax As Double, Ay As Double, Bx As Double, By As Double, Cx As Double, Cy As Double) As Double
            Dim len1 As Double = Math.Sqrt((Ax - Bx) * (Ax - Bx) + (Ay - By) * (Ay - By))
            Dim len2 As Double = Math.Sqrt((Cx - Bx) * (Cx - Bx) + (Cy - By) * (Cy - By))
            Dim len3 As Double = Math.Sqrt((Ax - Cx) * (Ax - Cx) + (Ay - Cy) * (Ay - Cy))
            Dim len As Double = If(len1 > len2, len1, len2)
            Return If(len > len3, len, len3)
        End Function
    End Class

    Public Structure dTriangle
        Public vv0 As Long
        Public vv1 As Long
        Public vv2 As Long

        Public Overrides Function ToString() As String
            Return {vv0, vv1, vv2}.JoinBy(" - ")
        End Function
    End Structure
    Public Structure OrTriangle
        Public p0 As Point
        Public p1 As Point
        Public p2 As Point
    End Structure
    Public Structure Triangle
        Public P0Index As Integer
        Public P1Index As Integer
        Public P2Index As Integer
        Public Index As Integer
        Public Sub New(p0index As Integer, p1index As Integer, p2index As Integer)
            Me.P0Index = p0index
            Me.P1Index = p1index
            Me.P2Index = p2index
            Me.Index = -1
        End Sub
        Public Sub New(p0index As Integer, p1index As Integer, p2index As Integer, index As Integer)
            Me.P0Index = p0index
            Me.P1Index = p1index
            Me.P2Index = p2index
            Me.Index = index
        End Sub
    End Structure
    Public Structure EdgeInfo
        Public P0Index As Integer
        Public P1Index As Integer
        Public AdjTriangle As List(Of Integer)
        Public Flag As Boolean
        Public Length As Double
        Public Function GetEdgeType() As Integer
            Return AdjTriangle.Count
        End Function
        Public Function IsValid() As Boolean
            Return P0Index <> -1
        End Function
        Public Sub New(d As Integer)
            P0Index = -1
            P1Index = -1
            Flag = False
            AdjTriangle = New List(Of Integer)()
            Length = -1
        End Sub
    End Structure
    Public Class DelaunayMesh2d
        Public Points As List(Of Point)
        Public Faces As List(Of Triangle)
        Public Edges As EdgeInfo(,)
        Public Sub New()
            Points = New List(Of Point)()
            Faces = New List(Of Triangle)()
        End Sub
        Public Function AddVertex(p As Point) As Integer
            Points.Add(p)
            Return Points.Count - 1
        End Function
        Public Function AddFace(t As Triangle) As Integer
            Faces.Add(t)
            Return Faces.Count - 1
        End Function
        Public Sub InitEdgesInfo()
            Edges = New EdgeInfo(Points.Count - 1, Points.Count - 1) {}
            For i As Integer = 0 To Points.Count - 1
                For j As Integer = 0 To Points.Count - 1
                    Edges(i, j) = New EdgeInfo(0)
                Next
            Next
            For i As Integer = 0 To Faces.Count - 1
                Dim t As Triangle = Faces(i)
                SetEdge(t, i)
            Next

        End Sub
        Private Sub SetEdge(t As Triangle, i As Integer)
            If t.P0Index < t.P1Index Then
                Edges(t.P0Index, t.P1Index).P0Index = t.P0Index
                Edges(t.P0Index, t.P1Index).P1Index = t.P1Index
                Edges(t.P0Index, t.P1Index).AdjTriangle.Add(i)
                Edges(t.P0Index, t.P1Index).Length = BallConcave.GetDistance(Points(t.P0Index), Points(t.P1Index))
            Else
                Edges(t.P1Index, t.P0Index).P0Index = t.P1Index
                Edges(t.P1Index, t.P0Index).P1Index = t.P0Index
                Edges(t.P1Index, t.P0Index).AdjTriangle.Add(i)
                Edges(t.P1Index, t.P0Index).Length = BallConcave.GetDistance(Points(t.P0Index), Points(t.P1Index))
            End If

            If t.P1Index < t.P2Index Then
                Edges(t.P1Index, t.P2Index).P0Index = t.P1Index
                Edges(t.P1Index, t.P2Index).P1Index = t.P2Index
                Edges(t.P1Index, t.P2Index).AdjTriangle.Add(i)
                Edges(t.P1Index, t.P2Index).Length = BallConcave.GetDistance(Points(t.P1Index), Points(t.P2Index))
            Else
                Edges(t.P2Index, t.P1Index).P0Index = t.P2Index
                Edges(t.P2Index, t.P1Index).P1Index = t.P1Index
                Edges(t.P2Index, t.P1Index).AdjTriangle.Add(i)
                Edges(t.P2Index, t.P1Index).Length = BallConcave.GetDistance(Points(t.P1Index), Points(t.P2Index))
            End If

            If t.P0Index < t.P2Index Then
                Edges(t.P0Index, t.P2Index).P0Index = t.P0Index
                Edges(t.P0Index, t.P2Index).P1Index = t.P2Index
                Edges(t.P0Index, t.P2Index).AdjTriangle.Add(i)
                Edges(t.P0Index, t.P2Index).Length = BallConcave.GetDistance(Points(t.P0Index), Points(t.P2Index))
            Else
                Edges(t.P2Index, t.P0Index).P0Index = t.P2Index
                Edges(t.P2Index, t.P0Index).P1Index = t.P0Index
                Edges(t.P2Index, t.P0Index).AdjTriangle.Add(i)
                Edges(t.P2Index, t.P0Index).Length = BallConcave.GetDistance(Points(t.P0Index), Points(t.P2Index))
            End If
        End Sub
        Public Sub ExecuteEdgeDecimation(length As Double)
            Dim queue As New Queue(Of EdgeInfo)()
            For i As Integer = 0 To Points.Count - 1
                For j As Integer = 0 To Points.Count - 1
                    If i < j AndAlso Edges(i, j).IsValid() Then
                        If Edges(i, j).GetEdgeType() = 0 Then
                            Throw New Exception()
                        End If
                        If Edges(i, j).Length > length AndAlso Edges(i, j).GetEdgeType() = 1 Then
                            queue.Enqueue(Edges(i, j))
                        End If
                    End If
                Next
            Next
            Dim opp1Temp As EdgeInfo() = New EdgeInfo(1) {}
            While queue.Count <> 0
                Dim info As EdgeInfo = queue.Dequeue()
                If info.AdjTriangle.Count <> 1 Then
                    Throw New Exception()
                End If
                Dim tindex As Integer = info.AdjTriangle(0)
                Dim t As Triangle = Faces(tindex)
                InitOppEdge(opp1Temp, t, info)
                SetInvalid(info.P0Index, info.P1Index)
                For i As Integer = 0 To 1
                    Dim e As EdgeInfo = opp1Temp(i)
                    e.AdjTriangle.Remove(tindex)
                    If e.GetEdgeType() = 0 Then
                        SetInvalid(e.P0Index, e.P1Index)
                    ElseIf e.GetEdgeType() = 1 AndAlso e.Length > length Then
                        queue.Enqueue(e)
                    End If
                Next
            End While
        End Sub
        Public Function GetBoundaryEdges() As List(Of EdgeInfo)
            Dim list As New List(Of EdgeInfo)()
            For i As Integer = 0 To Points.Count - 1
                For j As Integer = 0 To Points.Count - 1
                    If i < j Then
                        If Edges(i, j).GetEdgeType() = 1 Then
                            list.Add(Edges(i, j))
                        End If
                    End If
                Next
            Next
            Return list
        End Function
        Private Sub SetInvalid(i As Integer, j As Integer)
            Edges(i, j).AdjTriangle.Clear()
            Edges(i, j).Flag = True
            Edges(i, j).P0Index = -1
            Edges(i, j).P1Index = -1
        End Sub
        Private Sub InitOppEdge(opp1Temp As EdgeInfo(), t As Triangle, info As EdgeInfo)
            Dim vindex As Integer = t.P0Index + t.P1Index + t.P2Index - info.P0Index - info.P1Index
            If vindex < info.P0Index Then
                opp1Temp(0) = Edges(vindex, info.P0Index)
            Else
                opp1Temp(0) = Edges(info.P0Index, vindex)
            End If

            If vindex < info.P1Index Then
                opp1Temp(1) = Edges(vindex, info.P1Index)
            Else
                opp1Temp(1) = Edges(info.P1Index, vindex)
            End If
        End Sub
    End Class


    Public Class BallConcave

        Private Structure Point2dInfo
            Implements IComparable(Of Point2dInfo)
            Public Point As Point
            Public Index As Integer
            Public DistanceTo As Double
            Public Sub New(p As Point, i As Integer, dis As Double)
                Me.Point = p
                Me.Index = i
                Me.DistanceTo = dis
            End Sub
            Public Function CompareTo(other As Point2dInfo) As Integer Implements IComparable(Of Point2dInfo).CompareTo
                Return DistanceTo.CompareTo(other.DistanceTo)
            End Function
            Public Overrides Function ToString() As String
                Return Convert.ToString(Point) & "," & Index & "," & DistanceTo
            End Function
        End Structure
        Public Sub New(list As List(Of Point))
            Me.points = list
            points.Sort()
            flags = New Boolean(points.Count - 1) {}
            For i As Integer = 0 To flags.Length - 1
                flags(i) = False
            Next
            InitDistanceMap()
            InitNearestList()
        End Sub
        Private flags As Boolean()
        Private points As List(Of Point)
        Private distanceMap As Double(,)
        Private rNeigbourList As List(Of Integer)()
        Private Sub InitNearestList()
            rNeigbourList = New List(Of Integer)(points.Count - 1) {}
            For i As Integer = 0 To rNeigbourList.Length - 1
                rNeigbourList(i) = GetSortedNeighbours(i)
            Next
        End Sub
        Private Sub InitDistanceMap()
            distanceMap = New Double(points.Count - 1, points.Count - 1) {}
            For i As Integer = 0 To points.Count - 1
                For j As Integer = 0 To points.Count - 1
                    distanceMap(i, j) = GetDistance(points(i), points(j))
                Next
            Next
        End Sub
        Public Function GetRecomandedR() As Double
            Dim r As Double = Double.MinValue
            For i As Integer = 0 To points.Count - 1
                If distanceMap(i, rNeigbourList(i)(1)) > r Then
                    r = distanceMap(i, rNeigbourList(i)(1))
                End If
            Next
            Return r
        End Function
        Public Function GetMinEdgeLength() As Double
            Dim min As Double = Double.MaxValue
            For i As Integer = 0 To points.Count - 1
                For j As Integer = 0 To points.Count - 1
                    If i < j Then
                        If distanceMap(i, j) < min Then
                            min = distanceMap(i, j)
                        End If
                    End If
                Next
            Next
            Return min
        End Function
        Public Function GetConcave_Ball(radius As Double) As List(Of Point)
            Dim ret As New List(Of Point)()
            Dim adjs As List(Of Integer)() = GetInRNeighbourList(2 * radius)
            ret.Add(points(0))
            'flags[0] = true;
            Dim i As Integer = 0, j As Integer = -1, prev As Integer = -1
            While True
                j = GetNextPoint_BallPivoting(prev, i, adjs(i), radius)
                If j = -1 Then
                    Exit While
                End If
                Dim p As Point = BallConcave.GetCircleCenter(points(i), points(j), radius)
                ret.Add(points(j))
                flags(j) = True
                prev = i
                i = j
            End While
            Return ret
        End Function
        Public Function GetConcave_Edge(radius As Double) As List(Of Point)
            Dim ret As New List(Of Point)()
            Dim adjs As List(Of Integer)() = GetInRNeighbourList(2 * radius)
            ret.Add(points(0))
            Dim i As Integer = 0, j As Integer = -1, prev As Integer = -1
            While True
                j = GetNextPoint_EdgePivoting(prev, i, adjs(i), radius)
                If j = -1 Then
                    Exit While
                End If
                'Point2d p = BallConcave.GetCircleCenter(points[i], points[j], radius);
                ret.Add(points(j))
                flags(j) = True
                prev = i
                i = j
            End While
            Return ret
        End Function
        Private Function CheckValid(adjs As List(Of Integer)()) As Boolean
            For i As Integer = 0 To adjs.Length - 1
                If adjs(i).Count < 2 Then
                    Return False
                End If
            Next
            Return True
        End Function
        Public Function CompareAngel(a As Point, b As Point, m_origin As Point, m_dreference As Point) As Boolean

            Dim da As New Point(a.X - m_origin.X, a.Y - m_origin.Y)
            Dim db As New Point(b.X - m_origin.X, b.Y - m_origin.Y)
            Dim detb As Double = GetCross(m_dreference, db)

            ' nothing is less than zero degrees
            If detb = 0 AndAlso db.X * m_dreference.X + db.Y * m_dreference.Y >= 0 Then
                Return False
            End If

            Dim deta As Double = GetCross(m_dreference, da)

            ' zero degrees is less than anything else
            If deta = 0 AndAlso da.X * m_dreference.X + da.Y * m_dreference.Y >= 0 Then
                Return True
            End If

            If deta * detb >= 0 Then
                ' both on same side of reference, compare to each other
                Return GetCross(da, db) > 0
            End If

            ' vectors "less than" zero degrees are actually large, near 2 pi
            Return deta > 0
        End Function
        Public Function GetNextPoint_EdgePivoting(prev As Integer, current As Integer, list As List(Of Integer), radius As Double) As Integer
            If list.Count = 2 AndAlso prev <> -1 Then
                Return list(0) + list(1) - prev
            End If
            Dim dp As Point
            If prev = -1 Then
                dp = New Point(1, 0)
            Else
                dp = points(prev) - points(current)
            End If
            Dim min As Integer = -1
            For j As Integer = 0 To list.Count - 1
                If Not flags(list(j)) Then
                    If min = -1 Then
                        min = list(j)
                    Else
                        Dim t As Point = points(list(j))
                        If CompareAngel(points(min), t, points(current), dp) AndAlso GetDistance(t, points(current)) < radius Then
                            min = list(j)
                        End If
                    End If
                End If
            Next
            'main.ShowMessage("seek P" + points[min].Index);
            Return min
        End Function
        Public Function GetNextPoint_BallPivoting(prev As Integer, current As Integer, list As List(Of Integer), radius As Double) As Integer
            SortAdjListByAngel(list, prev, current)
            For j As Integer = 0 To list.Count - 1
                If flags(list(j)) Then
                    Continue For
                End If
                Dim adjIndex As Integer = list(j)
                Dim xianp As Point = points(adjIndex)
                Dim rightCirleCenter As Point = GetCircleCenter(points(current), xianp, radius)
                If Not HasPointsInCircle(list, rightCirleCenter, radius, adjIndex) Then
                    ' main.DrawCircleWithXian(rightCirleCenter, points(current), points(adjIndex), radius)
                    Return list(j)
                End If
            Next
            Return -1
        End Function
        Private Sub SortAdjListByAngel(list As List(Of Integer), prev As Integer, current As Integer)
            Dim origin As Point = points(current)
            Dim df As Point
            If prev <> -1 Then
                df = New Point(points(prev).X - origin.X, points(prev).Y - origin.Y)
            Else
                df = New Point(1, 0)
            End If
            Dim temp As Integer = 0
            For i As Integer = list.Count To 1 Step -1
                For j As Integer = 0 To i - 2
                    If CompareAngel(points(list(j)), points(list(j + 1)), origin, df) Then
                        temp = list(j)
                        list(j) = list(j + 1)
                        list(j + 1) = temp
                    End If
                Next
            Next
        End Sub
        Private Function HasPointsInCircle(adjPoints As List(Of Integer), center As Point, radius As Double, adjIndex As Integer) As Boolean
            For k As Integer = 0 To adjPoints.Count - 1
                If adjPoints(k) <> adjIndex Then
                    Dim index2 As Integer = adjPoints(k)
                    If IsInCircle(points(index2), center, radius) Then
                        Return True
                    End If
                End If
            Next
            Return False
        End Function
        Public Shared Function GetCircleCenter(a As Point, b As Point, r As Double) As Point
            Dim dx As Double = b.X - a.X
            Dim dy As Double = b.Y - a.Y
            Dim cx As Double = 0.5 * (b.X + a.X)
            Dim cy As Double = 0.5 * (b.Y + a.Y)
            If r * r / (dx * dx + dy * dy) - 0.25 < 0 Then
                Return New Point(-1, -1)
            End If
            Dim sqrt As Double = Math.Sqrt(r * r / (dx * dx + dy * dy) - 0.25)
            Return New Point(cx - dy * sqrt, cy + dx * sqrt)
        End Function
        Public Shared Function IsInCircle(p As Point, center As Point, r As Double) As Boolean
            Dim dis2 As Double = (p.X - center.X) * (p.X - center.X) + (p.Y - center.Y) * (p.Y - center.Y)
            Return dis2 < r * r
        End Function
        Public Function GetInRNeighbourList(radius As Double) As List(Of Integer)()
            Dim adjs As List(Of Integer)() = New List(Of Integer)(points.Count - 1) {}
            For i As Integer = 0 To points.Count - 1
                adjs(i) = New List(Of Integer)()
            Next
            For i As Integer = 0 To points.Count - 1

                For j As Integer = 0 To points.Count - 1
                    If i < j AndAlso distanceMap(i, j) < radius Then
                        adjs(i).Add(j)
                        adjs(j).Add(i)
                    End If
                Next
            Next
            Return adjs
        End Function
        Private Function GetSortedNeighbours(index As Integer) As List(Of Integer)
            Dim infos As New List(Of Point2dInfo)(points.Count)
            For i As Integer = 0 To points.Count - 1
                infos.Add(New Point2dInfo(points(i), i, distanceMap(index, i)))
            Next
            infos.Sort()
            Dim adj As New List(Of Integer)()
            For i As Integer = 1 To infos.Count - 1
                adj.Add(infos(i).Index)
            Next
            Return adj
        End Function
        Public Shared Function GetDistance(p1 As Point, p2 As Point) As Double
            Return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y))
        End Function
        Public Shared Function GetCross(a As Point, b As Point) As Double
            Return a.X * b.Y - a.Y * b.X
        End Function
    End Class
End Namespace