Imports Microsoft.VisualBasic.Imaging.Drawing3D.IsoMetric
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D

Namespace Drawing3D

    Public Class IsometricEngine

        Private ReadOnly angle, scale As Double

        Private transformation As Double()()

        Private originX, originY As Double

        Private items As IList(Of Item) = New List(Of Item)

        Private ReadOnly lightAngle As Point3D

        Private ReadOnly colorDifference As Double

        Private ReadOnly lightColor As HSLColor

        Public Sub New()
            Me.angle = Math.PI / 6
            Me.scale = 70
            Me.transformation = New Double() {{Me.scale * Math.Cos(Me.angle), Me.scale * Math.Sin(Me.angle)}, {Me.scale * Math.Cos(Math.PI - Me.angle), Me.scale * Math.Sin(Math.PI - Me.angle)}}
            Dim lightPosition As New Point3D(2, -1, 3)
            Me.lightAngle = lightPosition.Normalize()
            Me.colorDifference = 0.2
            Me.lightColor = New HSLColor(255, 255, 255)

        End Sub

        ''' <summary>
        ''' X rides along the angle extended from the origin
        ''' Y rides perpendicular to this angle (in isometric view: PI - angle)
        ''' Z affects the y coordinate of the drawn point
        ''' </summary>
        Public Overridable Function translatePoint(ByVal ___point As Point3D) As Point3D
            Return New Point3D(Me.originX + ___point.X * Me.transformation(0)(0) + ___point.Y * Me.transformation(1)(0), Me.originY - ___point.X * Me.transformation(0)(1) - ___point.Y * Me.transformation(1)(1) - (___point.Z * Me.scale))
        End Function

        Public Overridable Sub add(ByVal ___path As Path3D, ByVal color As HSLColor)
            addPath(___path, color)
        End Sub

        Public Overridable Sub add(ByVal paths As Path3D(), ByVal color As HSLColor)
            For Each ___path As Path3D In paths
                add(___path, color)
            Next ___path
        End Sub

        Public Overridable Sub add(ByVal ___shape As Shape3D, ByVal color As HSLColor)
            ' Fetch paths ordered by distance to prevent overlaps 
            Dim paths As Path3D() = ___shape.orderedPath3Ds()

            For Each ___path As Path3D In paths
                addPath(___path, color)
            Next ___path
        End Sub

        Public Overridable Sub clear()
            items.Clear()
        End Sub

        Private Sub addPath(ByVal ___path As Path3D, ByVal color As HSLColor)
            Me.items.Add(New Item(___path, transformColor(___path, color)))
        End Sub

        Private Function transformColor(ByVal ___path As Path3D, ByVal color As HSLColor) As HSLColor
            Dim p1 As Point3D = ___path.Points(1)
            Dim p2 As Point3D = ___path.Points(0)
            Dim i As Double = p2.X - p1.X
            Dim j As Double = p2.Y - p1.Y
            Dim k As Double = p2.Z - p1.Z
            p1 = ___path.Points(2)
            p2 = ___path.Points(1)
            Dim i2 As Double = p2.X - p1.X
            Dim j2 As Double = p2.Y - p1.Y
            Dim k2 As Double = p2.Z - p1.Z
            Dim i3 As Double = j * k2 - j2 * k
            Dim j3 As Double = -1 * (i * k2 - i2 * k)
            Dim k3 As Double = i * j2 - i2 * j
            Dim magnitude As Double = Math.Sqrt(i3 * i3 + j3 * j3 + k3 * k3)
            i = If(magnitude = 0, 0, i3 / magnitude)
            j = If(magnitude = 0, 0, j3 / magnitude)
            k = If(magnitude = 0, 0, k3 / magnitude)
            Dim brightness As Double = i * lightAngle.X + j * lightAngle.Y + k * lightAngle.Z
            Return color.Lighten(brightness * Me.colorDifference, Me.lightColor)
        End Function

        Public Overridable Sub measure(ByVal width As Integer, ByVal height As Integer, ByVal sort As Boolean)
            Me.originX = width \ 2
            Me.originY = height * 0.9

            For Each item As Item In items

                item.transformedPoints = New Point3D(item.path.points.Length - 1) {}

                If Not item.drawPath.Empty Then item.drawPath.rewind() 'Todo: test if .reset is not needed and rewind is enough
                Dim i As Integer = 0
                Dim ___point As Point3D
                For i As Integer = 0 To item.path.points.Length - 1
                    ___point = item.path.points(i)
                    item.transformedPoints(i) = translatePoint(___point)
                Next i

                item.drawPath.moveTo(CSng(item.transformedPoints(0).x), CSng(item.transformedPoints(0).y))


                Dim length As Integer = item.transformedPoints.Length

                i = 1
                Do While i < length
                    item.drawPath.lineTo(CSng(item.transformedPoints(i).x), CSng(item.transformedPoints(i).y))
                    i += 1
                Loop

                item.drawPath.close()
            Next item

            If sort Then Me.items = sortPaths()
        End Sub

        Private Function sortPaths() As IList(Of Item)
            Dim sortedItems As New List(Of Item)
            Dim observer As New Point3D(-10, -10, 20)
            Dim length As Integer = items.Count
            Dim drawBefore As IList(Of IList(Of Integer?)) = New List(Of IList(Of Integer?))(length)
            For i As Integer = 0 To length - 1
                drawBefore.Insert(i, New List(Of Integer?))
            Next i
            Dim itemA As Item
            Dim itemB As Item
            For i As Integer = 0 To length - 1
                itemA = items(i)
                For j As Integer = 0 To i - 1
                    itemB = items(j)
                    If hasIntersection(itemA.transformedPoints, itemB.transformedPoints) Then
                        Dim cmpPath As Integer = itemA.path.closerThan(itemB.path, observer)
                        If cmpPath < 0 Then
                            drawBefore(i).Add(j)
                        ElseIf cmpPath > 0 Then
                            drawBefore(j).Add(i)
                        End If
                    End If
                Next j
            Next i
            Dim drawThisTurn As Integer = 1
            Dim currItem As Item
            Dim integers As IList(Of Integer?)
            Do While drawThisTurn = 1
                drawThisTurn = 0
                For i As Integer = 0 To length - 1
                    currItem = items(i)
                    integers = drawBefore(i)
                    If currItem.drawn = 0 Then
                        Dim canDraw As Integer = 1
                        Dim j As Integer = 0
                        Dim lengthIntegers As Integer = integers.Count
                        Do While j < lengthIntegers
                            If items(integers(j)).drawn = 0 Then
                                canDraw = 0
                                Exit Do
                            End If
                            j += 1
                        Loop
                        If canDraw = 1 Then
                            Dim item As New Item(currItem)
                            sortedItems.Add(item)
                            currItem.drawn = 1
                            items(i) = currItem
                            drawThisTurn = 1
                        End If
                    End If
                Next i
            Loop

            For i As Integer = 0 To length - 1
                currItem = items(i)
                If currItem.drawn = 0 Then sortedItems.Add(New Item(currItem))
            Next i
            Return sortedItems
        End Function

        Public Overridable Sub draw(ByVal canvas As IGraphics)
            For Each item As Item In items
                '            this.ctx.globalAlpha = color.a;
                '            this.ctx.fillStyle = this.ctx.strokeStyle = color.toHex();
                '            this.ctx.stroke();
                '            this.ctx.fill();
                '            this.ctx.restore();
                canvas.DrawPath(item.drawPath, item.paint)
            Next item
        End Sub

        'Todo: use android.grphics region object to check if point is inside region
        'Todo: use path.op to check if the path intersects with another path
        'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        Public Overridable Function findItemForPosition(ByVal position As Point3D) As Item
            'Todo: reverse sorting for click detection, because hidden object is getting drawed first und will be returned as the first as well
            'Items are already sorted for depth sort so break should not be a problem here
            For Each item As Item In Me.items
                If item.transformedPoints Is Nothing Then Continue For
                Dim items As IList(Of Point3D) = New List(Of Point3D)
                Dim top As Point3D = Nothing, bottom As Point3D = Nothing, left As Point3D = Nothing, right As Point3D = Nothing
                For Each ___point As Point3D In item.transformedPoints
                    If top Is Nothing OrElse ___point.Y > top.Y Then
                        If top Is Nothing Then
                            top = New Point3D(___point.X, ___point.Y)
                        Else
                            top.Y = ___point.Y
                            top.X = ___point.X
                        End If
                    End If
                    If bottom Is Nothing OrElse ___point.Y < bottom.Y Then
                        If bottom Is Nothing Then
                            bottom = New Point3D(___point.X, ___point.Y)
                        Else
                            bottom.Y = ___point.Y
                            bottom.X = ___point.X
                        End If
                    End If
                    If left Is Nothing OrElse ___point.X < left.X Then
                        If left Is Nothing Then
                            left = New Point3D(___point.X, ___point.Y)
                        Else
                            left.X = ___point.X
                            left.Y = ___point.Y
                        End If
                    End If
                    If right Is Nothing OrElse ___point.X > right.X Then
                        If right Is Nothing Then
                            right = New Point3D(___point.X, ___point.Y)
                        Else
                            right.X = ___point.X
                            right.Y = ___point.Y
                        End If
                    End If
                Next ___point

                items.Add(left)
                items.Add(top)
                items.Add(right)
                items.Add(bottom)

                'search for equal points that are above or below for left and right or left and right for bottom and top
                For Each ___point As Point3D In item.transformedPoints
                    If ___point.X = left.X Then
                        If ___point.Y <> left.Y Then items.Add(___point)
                    End If
                    If ___point.X = right.X Then
                        If ___point.Y <> right.Y Then items.Add(___point)
                    End If
                    If ___point.Y = top.Y Then
                        If ___point.Y <> top.Y Then items.Add(___point)
                    End If
                    If ___point.Y = bottom.Y Then
                        If ___point.Y <> bottom.Y Then items.Add(___point)
                    End If
                Next ___point

                If isPointInPoly(items, position.X, position.Y) Then Return item
            Next item
            Return Nothing
        End Function

        Friend Class Item
            Friend path As Path3D
            Friend baseColor As HSLColor
            Friend paint As android.graphics.Paint
            Friend drawn As Integer
            Friend transformedPoints As Point3D()
            Friend drawPath As android.graphics.Path

            Friend Sub New(ByVal item As Item)
                transformedPoints = item.transformedPoints
                drawPath = item.drawPath
                drawn = item.drawn
                Me.paint = item.paint
                Me.path = item.path
                Me.baseColor = item.baseColor
            End Sub

            Friend Sub New(ByVal ___path As Path3D, ByVal baseColor As HSLColor)
                drawPath = New android.graphics.Path
                drawn = 0
                Me.paint = New android.graphics.Paint(android.graphics.Paint.ANTI_ALIAS_FLAG)
                Me.paint.Style = android.graphics.Paint.Style.FILL_AND_STROKE
                Me.paint.StrokeWidth = 1
                Me.path = ___path
                Me.baseColor = baseColor
                Me.paint.Color = android.graphics.Color.argb(CInt(Fix(baseColor.a)), CInt(Fix(baseColor.r)), CInt(Fix(baseColor.g)), CInt(Fix(baseColor.b)))
            End Sub
        End Class

        Private Function isPointInPoly(ByVal poly As IList(Of Point3D), ByVal x As Double, ByVal y As Double) As Boolean
            Dim c As Boolean = False
            Dim i As Integer = -1
            Dim l As Integer = poly.Count
            Dim j As Integer = l - 1
            'JAVA TO VB CONVERTER TODO TASK: Assignments within expressions are not supported in VB
            Do While i += 1 < l
				If ((poly(i).Y <= y AndAlso y < poly(j).Y) OrElse (poly(j).Y <= y AndAlso y < poly(i).Y)) AndAlso (x < (poly(j).X - poly(i).X) * (y - poly(i).Y) / (poly(j).Y - poly(i).Y) + poly(i).X) Then c = Not c
                j = i
            Loop
            Return c
        End Function

        Private Function isPointInPoly(ByVal poly As Point3D(), ByVal x As Double, ByVal y As Double) As Boolean
            Dim c As Boolean = False
            Dim i As Integer = -1
            Dim l As Integer = poly.Length
            Dim j As Integer = l - 1
            'JAVA TO VB CONVERTER TODO TASK: Assignments within expressions are not supported in VB
            Do While i += 1 < l
				If ((poly(i).Y <= y AndAlso y < poly(j).Y) OrElse (poly(j).Y <= y AndAlso y < poly(i).Y)) AndAlso (x < (poly(j).X - poly(i).X) * (y - poly(i).Y) / (poly(j).Y - poly(i).Y) + poly(i).X) Then c = Not c
                j = i
            Loop
            Return c
        End Function

        Private Function hasIntersection(ByVal pointsA As Point3D(), ByVal pointsB As Point3D()) As Boolean
            Dim i As Integer, j As Integer, lengthA As Integer = pointsA.Length, lengthB As Integer = pointsB.Length, lengthPolyA As Integer, lengthPolyB As Integer
            Dim AminX As Double = pointsA(0).X
            Dim AminY As Double = pointsA(0).Y
            Dim AmaxX As Double = AminX
            Dim AmaxY As Double = AminY
            Dim BminX As Double = pointsB(0).X
            Dim BminY As Double = pointsB(0).Y
            Dim BmaxX As Double = BminX
            Dim BmaxY As Double = BminY

            Dim ___point As Point3D

            For i = 0 To lengthA - 1
                ___point = pointsA(i)
                AminX = Math.Min(AminX, ___point.x)
                AminY = Math.Min(AminY, ___point.y)
                AmaxX = Math.Max(AmaxX, ___point.x)
                AmaxY = Math.Max(AmaxY, ___point.y)
            Next i
            For i = 0 To lengthB - 1
                ___point = pointsB(i)
                BminX = Math.Min(BminX, ___point.x)
                BminY = Math.Min(BminY, ___point.y)
                BmaxX = Math.Max(BmaxX, ___point.x)
                BmaxY = Math.Max(BmaxY, ___point.y)
            Next i

            If ((AminX <= BminX AndAlso BminX <= AmaxX) OrElse (BminX <= AminX AndAlso AminX <= BmaxX)) AndAlso ((AminY <= BminY AndAlso BminY <= AmaxY) OrElse (BminY <= AminY AndAlso AminY <= BmaxY)) Then
                ' now let's be more specific
                Dim polyA As Point3D() = Path.add(pointsA(0), pointsA)
                Dim polyB As Point3D() = Path.add(pointsB(0), pointsB)

                ' see if edges cross, or one contained in the other
                lengthPolyA = polyA.Length
                lengthPolyB = polyB.Length

                Dim deltaAX As Double() = New Double(lengthPolyA - 1) {}
                Dim deltaAY As Double() = New Double(lengthPolyA - 1) {}
                Dim deltaBX As Double() = New Double(lengthPolyB - 1) {}
                Dim deltaBY As Double() = New Double(lengthPolyB - 1) {}

                Dim rA As Double() = New Double(lengthPolyA - 1) {}
                Dim rB As Double() = New Double(lengthPolyB - 1) {}

                For i = 0 To lengthPolyA - 2
                    ___point = polyA(i)
                    deltaAX(i) = polyA(i + 1).x - ___point.x
                    deltaAY(i) = polyA(i + 1).y - ___point.y
                    'equation written as deltaY.x - deltaX.y + r = 0
                    rA(i) = deltaAX(i) * ___point.y - deltaAY(i) * ___point.x
                Next i

                For i = 0 To lengthPolyB - 2
                    ___point = polyB(i)
                    deltaBX(i) = polyB(i + 1).x - ___point.x
                    deltaBY(i) = polyB(i + 1).y - ___point.y
                    rB(i) = deltaBX(i) * ___point.y - deltaBY(i) * ___point.x
                Next i

                For i = 0 To lengthPolyA - 2
                    For j = 0 To lengthPolyB - 2
                        If deltaAX(i) * deltaBY(j) <> deltaAY(i) * deltaBX(j) Then
                            'case when vectors are colinear, or one polygon included in the other, is covered after
                            'two segments cross each other if and only if the points of the first are on each side of the line defined by the second and vice-versa
                            If (deltaAY(i) * polyB(j).x - deltaAX(i) * polyB(j).y + rA(i)) * (deltaAY(i) * polyB(j + 1).x - deltaAX(i) * polyB(j + 1).y + rA(i)) < -0.000000001 AndAlso (deltaBY(j) * polyA(i).x - deltaBX(j) * polyA(i).y + rB(j)) * (deltaBY(j) * polyA(i + 1).x - deltaBX(j) * polyA(i + 1).y + rB(j)) < -0.000000001 Then Return True
                        End If
                    Next j
                Next i

                For i = 0 To lengthPolyA - 2
                    ___point = polyA(i)
                    If isPointInPoly(polyB, ___point.x, ___point.y) Then Return True
                Next i
                For i = 0 To lengthPolyB - 2
                    ___point = polyB(i)
                    If isPointInPoly(polyA, ___point.x, ___point.y) Then Return True
                Next i

                Return False
            Else
                Return False
            End If
        End Function
    End Class

End Namespace